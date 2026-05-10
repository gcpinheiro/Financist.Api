using Financist.Domain.Entities;

namespace Financist.Application.Abstractions.Persistence;

public interface IDocumentTransactionCandidateRepository
{
    Task AddRangeAsync(
        IEnumerable<DocumentTransactionCandidate> candidates,
        CancellationToken cancellationToken = default);

    Task<DocumentTransactionCandidate?> GetByIdAsync(
        Guid documentImportId,
        Guid candidateId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentTransactionCandidate>> ListByDocumentAsync(
        Guid documentImportId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> ListExistingFingerprintsAsync(
        Guid userId,
        IReadOnlyCollection<string> fingerprints,
        CancellationToken cancellationToken = default);
}
