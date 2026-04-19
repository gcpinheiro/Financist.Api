using Financist.Domain.Entities;
using Financist.Domain.Enums;

namespace Financist.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(Guid userId, string name, TransactionType type, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Category>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Category category, CancellationToken cancellationToken = default);
}
