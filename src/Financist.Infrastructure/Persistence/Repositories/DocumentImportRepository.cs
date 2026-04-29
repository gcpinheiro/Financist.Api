using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Financist.Infrastructure.Persistence.Repositories;

public sealed class DocumentImportRepository : IDocumentImportRepository
{
    private readonly FinancistDbContext _dbContext;

    public DocumentImportRepository(FinancistDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(DocumentImport documentImport, CancellationToken cancellationToken = default)
    {
        return _dbContext.DocumentImports.AddAsync(documentImport, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<DocumentImport>> ListByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DocumentImports
            .AsNoTracking()
            .Where(documentImport => documentImport.UserId == userId)
            .OrderByDescending(documentImport => documentImport.UploadedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
