using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Common;
using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Financist.Infrastructure.Persistence;

public sealed class FinancistDbContext : DbContext, IUnitOfWork
{
    public FinancistDbContext(DbContextOptions<FinancistDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Card> Cards => Set<Card>();

    public DbSet<Goal> Goals => Set<Goal>();

    public DbSet<DocumentImport> DocumentImports => Set<DocumentImport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancistDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker
            .Entries<AuditableEntity>()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added && entry.Entity.CreatedAtUtc == default)
            {
                entry.Property(entity => entity.CreatedAtUtc).CurrentValue = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.UpdatedAtUtc).CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
