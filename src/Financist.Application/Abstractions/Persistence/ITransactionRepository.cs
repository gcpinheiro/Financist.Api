using Financist.Domain.Entities;

namespace Financist.Application.Abstractions.Persistence;

public interface ITransactionRepository
{
    Task<IReadOnlyList<Transaction>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
}
