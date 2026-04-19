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
    private readonly IDocumentImportRepository _documentImportRepository;
    private readonly IDocumentStorageService _documentStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(
        IDocumentImportRepository documentImportRepository,
        IDocumentStorageService documentStorageService,
        IDocumentAnalysisService documentAnalysisService,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _documentImportRepository = documentImportRepository;
        _documentStorageService = documentStorageService;
        _documentAnalysisService = documentAnalysisService;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
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
