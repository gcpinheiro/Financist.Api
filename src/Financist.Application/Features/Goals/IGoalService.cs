namespace Financist.Application.Features.Goals;

public interface IGoalService
{
    Task<GoalDto> CreateAsync(CreateGoalRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GoalDto>> ListAsync(CancellationToken cancellationToken = default);
}
