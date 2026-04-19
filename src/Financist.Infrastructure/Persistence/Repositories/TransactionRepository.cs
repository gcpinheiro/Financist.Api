using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Financist.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly FinancistDbContext _dbContext;

    public TransactionRepository(FinancistDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Transaction>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId)
            .OrderByDescending(transaction => transaction.OccurredOn)
            .ThenByDescending(transaction => transaction.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        return _dbContext.Transactions.AddAsync(transaction, cancellationToken).AsTask();
    }
}
