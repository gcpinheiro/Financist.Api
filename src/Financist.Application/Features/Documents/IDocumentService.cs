namespace Financist.Application.Features.Documents;

public interface IDocumentService
{
    Task<IReadOnlyList<DocumentImportDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<DocumentImportDto> UploadAsync(UploadDocumentRequest request, CancellationToken cancellationToken = default);
}
