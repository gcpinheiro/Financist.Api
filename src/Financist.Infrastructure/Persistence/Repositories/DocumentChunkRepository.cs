using Financist.Application.Abstractions.Persistence;
using Financist.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Financist.Infrastructure.Persistence.Repositories;

public sealed class DocumentChunkRepository : IDocumentChunkRepository
{
    private readonly FinancistDbContext _dbContext;

    public DocumentChunkRepository(FinancistDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddRangeAsync(IEnumerable<DocumentChunk> chunks, CancellationToken cancellationToken = default)
    {
        return _dbContext.DocumentChunks.AddRangeAsync(chunks, cancellationToken);
    }

    public async Task<IReadOnlyList<DocumentChunk>> SearchRelevantAsync(
        Guid userId,
        string query,
        int maxChunks,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || maxChunks <= 0)
        {
            return [];
        }

        var fileNameMatches = await SearchByFileNameAsync(userId, query, maxChunks, cancellationToken);
        if (fileNameMatches.Count > 0)
        {
            return fileNameMatches;
        }

        var fullTextMatches = await _dbContext.DocumentChunks
            .FromSqlInterpolated($"""
                SELECT dc.*
                FROM document_chunks dc
                WHERE dc.user_id = {userId}
                  AND to_tsvector('portuguese', dc.content) @@ plainto_tsquery('portuguese', {query})
                ORDER BY ts_rank_cd(
                    to_tsvector('portuguese', dc.content),
                    plainto_tsquery('portuguese', {query})
                ) DESC,
                dc.created_at_utc DESC
                LIMIT {maxChunks}
                """)
            .AsNoTracking()
            .Include(documentChunk => documentChunk.DocumentImport)
            .ToListAsync(cancellationToken);

        if (fullTextMatches.Count > 0)
        {
            return fullTextMatches;
        }

        return await SearchByTermsAsync(userId, query, maxChunks, cancellationToken);
    }

    private async Task<IReadOnlyList<DocumentChunk>> SearchByFileNameAsync(
        Guid userId,
        string query,
        int maxChunks,
        CancellationToken cancellationToken)
    {
        var fileNameCandidates = ExtractFileNameCandidates(query);
        if (fileNameCandidates.Count == 0)
        {
            return [];
        }

        var allMatches = new List<DocumentChunk>();

        foreach (var fileNameCandidate in fileNameCandidates)
        {
            var pattern = $"%{fileNameCandidate}%";
            var matches = await _dbContext.DocumentChunks
                .AsNoTracking()
                .Include(documentChunk => documentChunk.DocumentImport)
                .Where(documentChunk =>
                    documentChunk.UserId == userId &&
                    documentChunk.DocumentImport != null &&
                    EF.Functions.ILike(documentChunk.DocumentImport.OriginalFileName, pattern))
                .OrderByDescending(documentChunk => documentChunk.DocumentImport!.UploadedAtUtc)
                .ThenBy(documentChunk => documentChunk.ChunkIndex)
                .Take(80)
                .ToListAsync(cancellationToken);

            allMatches.AddRange(matches);
        }

        if (allMatches.Count == 0)
        {
            return [];
        }

        var terms = ExtractSearchTerms(query);
        var rankedMatches = allMatches
            .DistinctBy(documentChunk => documentChunk.Id)
            .Select(documentChunk => new
            {
                DocumentChunk = documentChunk,
                Score = terms.Count(term => documentChunk.Content.Contains(term, StringComparison.OrdinalIgnoreCase))
            })
            .OrderByDescending(match => match.Score)
            .ThenByDescending(match => match.DocumentChunk.DocumentImport?.UploadedAtUtc)
            .ThenBy(match => match.DocumentChunk.ChunkIndex)
            .Take(maxChunks)
            .Select(match => match.DocumentChunk)
            .ToArray();

        return rankedMatches;
    }

    private async Task<IReadOnlyList<DocumentChunk>> SearchByTermsAsync(
        Guid userId,
        string query,
        int maxChunks,
        CancellationToken cancellationToken)
    {
        var terms = query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(term => term.Length >= 3)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToArray();

        if (terms.Length == 0)
        {
            return [];
        }

        var chunksQuery = _dbContext.DocumentChunks
            .AsNoTracking()
            .Include(documentChunk => documentChunk.DocumentImport)
            .Where(documentChunk => documentChunk.UserId == userId);

        foreach (var term in terms)
        {
            var pattern = $"%{term}%";
            chunksQuery = chunksQuery.Where(documentChunk => EF.Functions.ILike(documentChunk.Content, pattern));
        }

        return await chunksQuery
            .OrderByDescending(documentChunk => documentChunk.CreatedAtUtc)
            .Take(maxChunks)
            .ToListAsync(cancellationToken);
    }

    private static IReadOnlyList<string> ExtractFileNameCandidates(string query)
    {
        return Regex.Matches(query, @"[\p{L}\p{N}_\-.]+\.pdf", RegexOptions.IgnoreCase)
            .Select(match => match.Value.Trim())
            .Where(fileName => fileName.Length > ".pdf".Length)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<string> ExtractSearchTerms(string query)
    {
        var ignoredTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "acesse",
            "arquivo",
            "chamado",
            "documento",
            "fatura",
            "gasto",
            "para",
            "qual",
            "veja"
        };

        return Regex.Matches(query, @"[\p{L}\p{N}]+", RegexOptions.IgnoreCase)
            .Select(match => match.Value.Trim())
            .Where(term => term.Length >= 3)
            .Where(term => !ignoredTerms.Contains(term))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToArray();
    }
}
