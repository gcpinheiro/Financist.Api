using Financist.Application.Abstractions.Persistence;
using Financist.Application.Abstractions.Services;
using Financist.Application.Abstractions.Storage;
using Financist.Application.Common.Exceptions;
using Financist.Domain.Entities;
using Financist.Domain.Enums;
using Financist.Domain.ValueObjects;

namespace Financist.Application.Features.Documents;

public sealed class DocumentService : IDocumentService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDocumentAnalysisService _documentAnalysisService;
    private readonly IDocumentChunkRepository _documentChunkRepository;
    private readonly IDocumentImportRepository _documentImportRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly IDocumentTextExtractionService _documentTextExtractionService;
    private readonly IDocumentTransactionCandidateRepository _documentTransactionCandidateRepository;
    private readonly IDocumentTransactionExtractionService _documentTransactionExtractionService;
    private readonly DocumentTextChunker _documentTextChunker;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(
        IDocumentImportRepository documentImportRepository,
        IDocumentChunkRepository documentChunkRepository,
        IDocumentTransactionCandidateRepository documentTransactionCandidateRepository,
        IDocumentStorageService documentStorageService,
        IDocumentAnalysisService documentAnalysisService,
        IDocumentTextExtractionService documentTextExtractionService,
        IDocumentTransactionExtractionService documentTransactionExtractionService,
        DocumentTextChunker documentTextChunker,
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _documentImportRepository = documentImportRepository;
        _documentChunkRepository = documentChunkRepository;
        _documentTransactionCandidateRepository = documentTransactionCandidateRepository;
        _documentStorageService = documentStorageService;
        _documentAnalysisService = documentAnalysisService;
        _documentTextExtractionService = documentTextExtractionService;
        _documentTransactionExtractionService = documentTransactionExtractionService;
        _documentTextChunker = documentTextChunker;
        _transactionRepository = transactionRepository;
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

    public async Task<IReadOnlyList<DocumentTransactionCandidateDto>> ListTransactionCandidatesAsync(
        Guid documentImportId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var documentImport = await _documentImportRepository.GetByIdAsync(documentImportId, userId, cancellationToken);
        if (documentImport is null)
        {
            throw new NotFoundException("Document import was not found.");
        }

        var candidates = await _documentTransactionCandidateRepository.ListByDocumentAsync(
            documentImportId,
            userId,
            cancellationToken);

        return candidates.Select(ToCandidateDto).ToArray();
    }

    public async Task<DocumentTransactionCandidateDto> ImportTransactionCandidateAsync(
        Guid documentImportId,
        Guid candidateId,
        ImportDocumentTransactionCandidateRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateImportRequest(request);

        var userId = _currentUserService.GetRequiredUserId();
        var candidate = await _documentTransactionCandidateRepository.GetByIdAsync(
            documentImportId,
            candidateId,
            userId,
            cancellationToken);

        if (candidate is null)
        {
            throw new NotFoundException("Document transaction candidate was not found.");
        }

        if (candidate.Status != DocumentTransactionCandidateStatus.PendingReview)
        {
            throw new ConflictException($"Only pending candidates can be imported. Current status: {candidate.Status}.");
        }

        candidate.ApplyReview(
            request.Description,
            new Money(request.Amount, request.Currency),
            request.Type,
            request.OccurredOn);

        var transaction = Transaction.Create(
            userId,
            candidate.Description,
            new Money(candidate.Amount.Amount, candidate.Amount.Currency),
            candidate.Type,
            candidate.OccurredOn,
            null,
            null,
            BuildImportedTransactionNotes(candidate));

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        candidate.MarkImported(transaction.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToCandidateDto(candidate);
    }

    public async Task<DocumentTransactionCandidateDto> RejectTransactionCandidateAsync(
        Guid documentImportId,
        Guid candidateId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var candidate = await _documentTransactionCandidateRepository.GetByIdAsync(
            documentImportId,
            candidateId,
            userId,
            cancellationToken);

        if (candidate is null)
        {
            throw new NotFoundException("Document transaction candidate was not found.");
        }

        if (candidate.Status != DocumentTransactionCandidateStatus.PendingReview)
        {
            throw new ConflictException($"Only pending candidates can be rejected. Current status: {candidate.Status}.");
        }

        candidate.Reject();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToCandidateDto(candidate);
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
            await ExtractTransactionCandidatesAsync(documentImport, extractedText, cancellationToken);
            documentImport.MarkCompleted();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            documentImport.MarkFailed(exception.Message);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task ExtractTransactionCandidatesAsync(
        DocumentImport documentImport,
        string extractedText,
        CancellationToken cancellationToken)
    {
        var extractedCandidates = _documentTransactionExtractionService.Extract(
            new DocumentTransactionExtractionRequest(
                documentImport.UserId,
                documentImport.Id,
                extractedText,
                DateOnly.FromDateTime(documentImport.UploadedAtUtc),
                "BRL"));

        if (extractedCandidates.Count == 0)
        {
            return;
        }

        var existingFingerprints = await _documentTransactionCandidateRepository.ListExistingFingerprintsAsync(
            documentImport.UserId,
            extractedCandidates.Select(candidate => candidate.ImportFingerprint).ToArray(),
            cancellationToken);

        var candidates = extractedCandidates
            .Where(candidate => !existingFingerprints.Contains(candidate.ImportFingerprint))
            .Select(candidate => DocumentTransactionCandidate.Create(
                documentImport.Id,
                documentImport.UserId,
                candidate.Description,
                new Money(candidate.Amount, candidate.Currency),
                candidate.Type,
                candidate.OccurredOn,
                candidate.RawText,
                candidate.Confidence,
                candidate.ImportFingerprint,
                candidate.InstallmentNumber,
                candidate.InstallmentCount,
                candidate.InstallmentGroupKey))
            .ToArray();

        if (candidates.Length > 0)
        {
            await _documentTransactionCandidateRepository.AddRangeAsync(candidates, cancellationToken);
        }
    }

    private static string BuildImportedTransactionNotes(DocumentTransactionCandidate candidate)
    {
        var installment = candidate.InstallmentNumber.HasValue
            ? $" Installment {candidate.InstallmentNumber}/{candidate.InstallmentCount}."
            : string.Empty;

        var notes = $"Imported from document {candidate.DocumentImportId}.{installment} Source: {candidate.RawText}";
        return notes.Length <= 1000 ? notes : notes[..1000].Trim();
    }

    private static void ValidateImportRequest(ImportDocumentTransactionCandidateRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors["description"] = ["Transaction description is required."];
        }

        if (request.Amount <= 0)
        {
            errors["amount"] = ["Transaction amount must be greater than zero."];
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors["currency"] = ["Currency is required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
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

    private static DocumentTransactionCandidateDto ToCandidateDto(DocumentTransactionCandidate candidate)
    {
        return new DocumentTransactionCandidateDto(
            candidate.Id,
            candidate.DocumentImportId,
            candidate.Description,
            candidate.Amount.Amount,
            candidate.Amount.Currency,
            candidate.Type,
            candidate.OccurredOn,
            candidate.RawText,
            candidate.Confidence,
            candidate.InstallmentNumber,
            candidate.InstallmentCount,
            candidate.InstallmentGroupKey,
            candidate.ImportFingerprint,
            candidate.Status,
            candidate.TransactionId);
    }
}
