using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Financist.Infrastructure.Persistence.Repositories;

public sealed class CardRepository : ICardRepository
{
    private readonly FinancistDbContext _dbContext;

    public CardRepository(FinancistDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Card?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Cards.FirstOrDefaultAsync(card => card.Id == id && card.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Card>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cards
            .AsNoTracking()
            .Where(card => card.UserId == userId)
            .OrderBy(card => card.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Card card, CancellationToken cancellationToken = default)
    {
        return _dbContext.Cards.AddAsync(card, cancellationToken).AsTask();
    }
}
