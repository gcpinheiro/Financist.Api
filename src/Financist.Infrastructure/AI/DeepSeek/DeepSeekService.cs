using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Financist.Application.AI.DTOs;
using Financist.Application.AI.Interfaces;
using Financist.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Financist.Infrastructure.AI.DeepSeek;

public sealed class DeepSeekService : IAIService
{
    private const string DefaultSystemPrompt = "Voce e um assistente financeiro util, claro e direto da Financist. Responda sempre em portugues do Brasil.";

    private readonly HttpClient _httpClient;
    private readonly ILogger<DeepSeekService> _logger;
    private readonly DeepSeekOptions _options;
    private readonly IRagContextService _ragContextService;

    public DeepSeekService(
        HttpClient httpClient,
        IOptions<DeepSeekOptions> options,
        ILogger<DeepSeekService> logger,
        IRagContextService ragContextService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
        _ragContextService = ragContextService;
    }

    public async Task<ChatResponseDto> SendMessageAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var userMessage = request.Message.Trim();
        var ragContext = await _ragContextService.BuildContextAsync(userMessage, cancellationToken);
        var systemPrompt = BuildSystemPrompt(request.SystemPrompt, ragContext);
        var providerRequest = new DeepSeekChatCompletionRequest(
            _options.Model,
            [
                new DeepSeekMessage("system", systemPrompt),
                new DeepSeekMessage("user", userMessage)
            ]);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
        {
            Content = JsonContent.Create(providerRequest)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        _logger.LogInformation(
            "Sending DeepSeek chat completion request. Model: {Model}, MessageLength: {MessageLength}",
            _options.Model,
            userMessage.Length);

        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var providerError = TryExtractProviderError(errorBody);

                _logger.LogWarning(
                    "DeepSeek chat completion failed with status code {StatusCode}. ProviderError: {ProviderError}",
                    (int)response.StatusCode,
                    providerError ?? Truncate(errorBody, 500));

                throw new AiProviderException(BuildProviderErrorMessage(response.StatusCode, providerError));
            }

            var payload = await response.Content.ReadFromJsonAsync<DeepSeekChatCompletionResponse>(cancellationToken);
            var content = payload?.Choices?.FirstOrDefault()?.Message?.Content?.Trim();

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("DeepSeek chat completion returned an empty response.");
                throw new AiProviderException("The AI service returned an empty response.");
            }

            _logger.LogInformation("DeepSeek chat completion succeeded. Model: {Model}", _options.Model);

            return new ChatResponseDto(content);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("DeepSeek chat completion timed out.");
            throw new AiProviderException("The AI service did not respond in time.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "DeepSeek chat completion failed due to an HTTP error.");
            throw new AiProviderException("The AI service is temporarily unavailable.", exception);
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogError("DeepSeek API key is not configured.");
            throw new AiProviderException("The AI service is not configured.");
        }
    }

    private static string BuildSystemPrompt(string? systemPrompt, string? ragContext)
    {
        var prompt = string.IsNullOrWhiteSpace(systemPrompt)
            ? DefaultSystemPrompt
            : $"{systemPrompt.Trim()}\n\nResponda sempre em portugues do Brasil.";

        if (string.IsNullOrWhiteSpace(ragContext))
        {
            return prompt;
        }

        return $"""
            {prompt}

            Use o contexto recuperado abaixo quando ele for relevante para a pergunta do usuario.
            Se o contexto nao for suficiente, diga isso com clareza e nao invente dados de faturas.

            Contexto recuperado:
            {ragContext}
            """;
    }

    private static string BuildProviderErrorMessage(System.Net.HttpStatusCode statusCode, string? providerError)
    {
        return statusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => "The AI provider rejected the request. Check the configured model and request payload.",
            System.Net.HttpStatusCode.Unauthorized => "The AI provider credentials are invalid or missing.",
            System.Net.HttpStatusCode.Forbidden => "The AI provider denied access to this resource.",
            (System.Net.HttpStatusCode)429 => "The AI provider rate limit or quota was exceeded.",
            System.Net.HttpStatusCode.ServiceUnavailable => "The AI provider is temporarily unavailable.",
            System.Net.HttpStatusCode.GatewayTimeout => "The AI provider timed out while processing the request.",
            _ when !string.IsNullOrWhiteSpace(providerError) => $"The AI provider returned an error: {providerError}",
            _ => "The AI service could not process the request right now."
        };
    }

    private static string? TryExtractProviderError(string? errorBody)
    {
        if (string.IsNullOrWhiteSpace(errorBody))
        {
            return null;
        }

        try
        {
            var payload = System.Text.Json.JsonSerializer.Deserialize<DeepSeekErrorResponse>(errorBody);
            return payload?.Error?.Message?.Trim();
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return $"{value[..maxLength]}...";
    }

    private sealed record DeepSeekChatCompletionRequest(string Model, IReadOnlyList<DeepSeekMessage> Messages);

    private sealed record DeepSeekMessage(string Role, string Content);

    private sealed record DeepSeekChatCompletionResponse(
        [property: JsonPropertyName("choices")] IReadOnlyList<DeepSeekChoice>? Choices);

    private sealed record DeepSeekChoice(
        [property: JsonPropertyName("message")] DeepSeekAssistantMessage? Message);

    private sealed record DeepSeekAssistantMessage(
        [property: JsonPropertyName("content")] string? Content);

    private sealed record DeepSeekErrorResponse(
        [property: JsonPropertyName("error")] DeepSeekError? Error);

    private sealed record DeepSeekError(
        [property: JsonPropertyName("message")] string? Message);
}
