namespace Financist.Application.Features.Goals;

public sealed record CreateGoalRequest(
    string Name,
    string? Description,
    decimal TargetAmount,
    string Currency,
    decimal InitialAmount);
