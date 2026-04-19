using Financist.Domain.Entities;

namespace Financist.Application.Abstractions.Persistence;

public interface IGoalRepository
{
    Task<IReadOnlyList<Goal>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Goal goal, CancellationToken cancellationToken = default);
}
