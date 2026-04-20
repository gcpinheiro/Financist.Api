using Asp.Versioning;
using Financist.Application.AI.DTOs;
using Financist.Application.AI.Interfaces;
using Financist.Application.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Financist.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize]
[Route("api/v{version:apiVersion}/ai")]
public sealed class AIController : ControllerBase
{
    private readonly IAIService _aiService;

    public AIController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("chat")]
    [ProducesResponseType(typeof(ChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ChatResponseDto>> Chat([FromBody] ChatRequestDto request, CancellationToken cancellationToken)
    {
        Validate(request);

        var normalizedRequest = request with
        {
            Message = request.Message.Trim(),
            SystemPrompt = string.IsNullOrWhiteSpace(request.SystemPrompt) ? null : request.SystemPrompt.Trim()
        };

        var response = await _aiService.SendMessageAsync(normalizedRequest, cancellationToken);
        return Ok(response);
    }

    private static void Validate(ChatRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            errors["message"] = ["Message is required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
