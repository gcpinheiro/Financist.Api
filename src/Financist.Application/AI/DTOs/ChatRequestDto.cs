namespace Financist.Application.AI.DTOs;

public sealed record ChatRequestDto(string Message, string? SystemPrompt);
