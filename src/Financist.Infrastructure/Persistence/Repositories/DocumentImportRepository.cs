using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Entities;

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
}
