using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Financist.Infrastructure.Persistence.Repositories;

public sealed class DocumentTransactionCandidateRepository : IDocumentTransactionCandidateRepository
{
    private readonly FinancistDbContext _dbContext;

    public DocumentTransactionCandidateRepository(FinancistDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddRangeAsync(
        IEnumerable<DocumentTransactionCandidate> candidates,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DocumentTransactionCandidates.AddRangeAsync(candidates, cancellationToken);
    }

    public Task<DocumentTransactionCandidate?> GetByIdAsync(
        Guid documentImportId,
        Guid candidateId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DocumentTransactionCandidates
            .FirstOrDefaultAsync(
                candidate =>
                    candidate.Id == candidateId &&
                    candidate.DocumentImportId == documentImportId &&
                    candidate.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentTransactionCandidate>> ListByDocumentAsync(
        Guid documentImportId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DocumentTransactionCandidates
            .AsNoTracking()
            .Where(candidate => candidate.DocumentImportId == documentImportId && candidate.UserId == userId)
            .OrderBy(candidate => candidate.OccurredOn)
            .ThenBy(candidate => candidate.Description)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlySet<string>> ListExistingFingerprintsAsync(
        Guid userId,
        IReadOnlyCollection<string> fingerprints,
        CancellationToken cancellationToken = default)
    {
        if (fingerprints.Count == 0)
        {
            return new HashSet<string>(StringComparer.Ordinal);
        }

        var existingFingerprints = await _dbContext.DocumentTransactionCandidates
            .AsNoTracking()
            .Where(candidate => candidate.UserId == userId && fingerprints.Contains(candidate.ImportFingerprint))
            .Select(candidate => candidate.ImportFingerprint)
            .ToListAsync(cancellationToken);

        return existingFingerprints.ToHashSet(StringComparer.Ordinal);
    }
}
