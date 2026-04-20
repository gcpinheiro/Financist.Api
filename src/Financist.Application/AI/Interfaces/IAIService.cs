using Financist.Application.AI.DTOs;

namespace Financist.Application.AI.Interfaces;

public interface IAIService
{
    Task<ChatResponseDto> SendMessageAsync(
        ChatRequestDto request,
        CancellationToken cancellationToken = default);
}
