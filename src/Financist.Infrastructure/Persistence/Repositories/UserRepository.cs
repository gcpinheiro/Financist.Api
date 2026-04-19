using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Financist.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly FinancistDbContext _dbContext;

    public UserRepository(FinancistDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.AddAsync(user, cancellationToken).AsTask();
    }
}
