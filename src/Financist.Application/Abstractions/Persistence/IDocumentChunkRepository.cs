using Financist.Domain.Entities;

namespace Financist.Application.Abstractions.Persistence;

public interface IDocumentChunkRepository
{
    Task AddRangeAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentChunk>> SearchRelevantAsync(
        Guid userId,
        string query,
        int maxChunks,
        CancellationToken cancellationToken = default);
}
