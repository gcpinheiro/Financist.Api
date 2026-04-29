using System.Text;
using Financist.Application.Abstractions.Persistence;
using Financist.Application.Abstractions.Services;
using Financist.Application.AI.Interfaces;
using Microsoft.Extensions.Options;

namespace Financist.Infrastructure.AI.Rag;

public sealed class RagContextService : IRagContextService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDocumentChunkRepository _documentChunkRepository;
    private readonly RagOptions _options;

    public RagContextService(
        ICurrentUserService currentUserService,
        IDocumentChunkRepository documentChunkRepository,
        IOptions<RagOptions> options)
    {
        _currentUserService = currentUserService;
        _documentChunkRepository = documentChunkRepository;
        _options = options.Value;
    }

    public async Task<string?> BuildContextAsync(string message, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var userId = _currentUserService.GetRequiredUserId();
        var chunks = await _documentChunkRepository.SearchRelevantAsync(
            userId,
            message,
            _options.MaxChunks,
            cancellationToken);

        if (chunks.Count == 0)
        {
            return null;
        }

        var contextBuilder = new StringBuilder();
        var currentLength = 0;

        foreach (var chunk in chunks)
        {
            var documentName = chunk.DocumentImport?.OriginalFileName ?? chunk.DocumentImportId.ToString();
            var entry = $"[Documento {documentName}, id {chunk.DocumentImportId}, trecho {chunk.ChunkIndex + 1}]\n{chunk.Content}\n\n";
            if (currentLength + entry.Length > _options.MaxContextCharacters)
            {
                break;
            }

            contextBuilder.Append(entry);
            currentLength += entry.Length;
        }

        return contextBuilder.Length == 0 ? null : contextBuilder.ToString().Trim();
    }
}
