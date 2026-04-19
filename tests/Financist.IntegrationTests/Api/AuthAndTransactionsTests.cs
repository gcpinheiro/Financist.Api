using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Financist.Application.Features.Auth;
using Financist.Application.Features.Transactions;
using Financist.Domain.Enums;
using Financist.Infrastructure.Persistence.Seed;

namespace Financist.IntegrationTests.Api;

public sealed class AuthAndTransactionsTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public AuthAndTransactionsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ShouldReturnJwtToken_ForSeededDevelopmentUser()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(
            DevelopmentDataSeeder.DefaultEmail,
            DevelopmentDataSeeder.DefaultPassword));

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
        Assert.Equal(DevelopmentDataSeeder.DefaultEmail, payload.Email);
    }

    [Fact]
    public async Task GetTransactions_ShouldRequireAuthentication()
    {
        var response = await _client.GetAsync("/api/v1/transactions");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateTransaction_ShouldPersist_WhenAuthenticated()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(
            DevelopmentDataSeeder.DefaultEmail,
            DevelopmentDataSeeder.DefaultPassword));

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
        Assert.NotNull(loginPayload);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginPayload.AccessToken);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/transactions", new CreateTransactionRequest(
            "Integration test expense",
            42.50m,
            "USD",
            TransactionType.Expense,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            null,
            "Created by integration test"));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/v1/transactions");
        listResponse.EnsureSuccessStatusCode();

        var payload = await listResponse.Content.ReadFromJsonAsync<List<TransactionDto>>(JsonOptions);

        Assert.NotNull(payload);
        Assert.Contains(payload, transaction => transaction.Description == "Integration test expense");
    }
}
