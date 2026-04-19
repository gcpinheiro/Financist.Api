using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Financist.Infrastructure.Persistence.Repositories;

public sealed class GoalRepository : IGoalRepository
{
    private readonly FinancistDbContext _dbContext;

    public GoalRepository(FinancistDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Goal>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == userId)
            .OrderBy(goal => goal.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Goal goal, CancellationToken cancellationToken = default)
    {
        return _dbContext.Goals.AddAsync(goal, cancellationToken).AsTask();
    }
}
