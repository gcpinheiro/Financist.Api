using Financist.Domain.Entities;

namespace Financist.Application.Abstractions.Persistence;

public interface ICardRepository
{
    Task<Card?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Card>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Card card, CancellationToken cancellationToken = default);
}
