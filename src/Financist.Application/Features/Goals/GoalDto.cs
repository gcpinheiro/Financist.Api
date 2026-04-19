namespace Financist.Application.Features.Goals;

public sealed record GoalDto(
    Guid Id,
    string Name,
    string? Description,
    decimal TargetAmount,
    decimal CurrentAmount,
    string Currency,
    decimal ProgressPercentage);
