using Financist.Application.Abstractions.Persistence;
using Financist.Application.Abstractions.Services;
using Financist.Application.Abstractions.Storage;
using Financist.Application.Common.Exceptions;
using Financist.Domain.Entities;

namespace Financist.Application.Features.Documents;

public sealed class DocumentService : IDocumentService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDocumentAnalysisService _documentAnalysisService;
    private readonly IDocumentChunkRepository _documentChunkRepository;
    private readonly IDocumentImportRepository _documentImportRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly IDocumentTextExtractionService _documentTextExtractionService;
    private readonly DocumentTextChunker _documentTextChunker;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(
        IDocumentImportRepository documentImportRepository,
        IDocumentChunkRepository documentChunkRepository,
        IDocumentStorageService documentStorageService,
        IDocumentAnalysisService documentAnalysisService,
        IDocumentTextExtractionService documentTextExtractionService,
        DocumentTextChunker documentTextChunker,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _documentImportRepository = documentImportRepository;
        _documentChunkRepository = documentChunkRepository;
        _documentStorageService = documentStorageService;
        _documentAnalysisService = documentAnalysisService;
        _documentTextExtractionService = documentTextExtractionService;
        _documentTextChunker = documentTextChunker;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<DocumentImportDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var documentImports = await _documentImportRepository.ListByUserAsync(userId, cancellationToken);

        return documentImports.Select(ToDto).ToArray();
    }

    public async Task<DocumentImportDto> UploadAsync(UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Content is null)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["File content is required."]
            });
        }

        if (string.IsNullOrWhiteSpace(request.FileName) || request.SizeBytes <= 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["A valid file is required."]
            });
        }

        _ = await _documentAnalysisService.AnalyzeAsync(
            new DocumentAnalysisRequest(request.FileName, request.ContentType, request.SizeBytes),
            cancellationToken);

        var storedDocument = await _documentStorageService.SaveAsync(
            request.Content,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var userId = _currentUserService.GetRequiredUserId();
        var documentImport = DocumentImport.Create(
            userId,
            storedDocument.StoredFileName,
            request.FileName,
            request.ContentType,
            storedDocument.StoragePath,
            storedDocument.SizeBytes);

        await _documentImportRepository.AddAsync(documentImport, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await IndexDocumentAsync(documentImport, cancellationToken);

        return ToDto(documentImport);
    }

    private async Task IndexDocumentAsync(DocumentImport documentImport, CancellationToken cancellationToken)
    {
        try
        {
            documentImport.MarkProcessing();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await using var storedContent = await _documentStorageService.OpenReadAsync(
                documentImport.StoragePath,
                cancellationToken);

            var extractedText = await _documentTextExtractionService.ExtractTextAsync(
                storedContent,
                documentImport.OriginalFileName,
                documentImport.ContentType,
                cancellationToken);

            var chunks = _documentTextChunker
                .Split(extractedText)
                .Select((chunk, index) => DocumentChunk.Create(
                    documentImport.Id,
                    documentImport.UserId,
                    index,
                    chunk))
                .ToArray();

            if (chunks.Length == 0)
            {
                documentImport.MarkFailed("No readable text was extracted from this document.");
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            await _documentChunkRepository.AddRangeAsync(chunks, cancellationToken);
            documentImport.MarkCompleted();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            documentImport.MarkFailed(exception.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private static DocumentImportDto ToDto(DocumentImport documentImport)
    {
        return new DocumentImportDto(
            documentImport.Id,
            documentImport.StoredFileName,
            documentImport.OriginalFileName,
            documentImport.ContentType,
            documentImport.SizeBytes,
            documentImport.Status,
            documentImport.UploadedAtUtc);
    }
}
