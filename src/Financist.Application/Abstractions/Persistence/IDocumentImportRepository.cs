using Financist.Domain.Entities;

namespace Financist.Application.Abstractions.Persistence;

public interface IDocumentImportRepository
{
    Task AddAsync(DocumentImport documentImport, CancellationToken cancellationToken = default);
}
