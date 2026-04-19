using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Entities;
using Financist.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Financist.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly FinancistDbContext _dbContext;

    public CategoryRepository(FinancistDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Category?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories
            .FirstOrDefaultAsync(category => category.Id == id && category.UserId == userId, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(Guid userId, string name, TransactionType type, CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.AnyAsync(
            category => category.UserId == userId && category.Name == name && category.Type == type,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId)
            .OrderBy(category => category.Type)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        return _dbContext.Categories.AddAsync(category, cancellationToken).AsTask();
    }
}
