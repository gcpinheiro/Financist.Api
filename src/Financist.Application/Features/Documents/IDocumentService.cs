namespace Financist.Application.Features.Documents;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentImportDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<DocumentImportDto> UploadAsync(UploadDocumentRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DocumentTransactionCandidateDto>> ListTransactionCandidatesAsync(
        Guid documentImportId,
        CancellationToken cancellationToken = default);

    Task<DocumentTransactionCandidateDto> ImportTransactionCandidateAsync(
        Guid documentImportId,
        Guid candidateId,
        ImportDocumentTransactionCandidateRequest request,
        CancellationToken cancellationToken = default);

    Task<DocumentTransactionCandidateDto> RejectTransactionCandidateAsync(
        Guid documentImportId,
        Guid candidateId,
        CancellationToken cancellationToken = default);
}
