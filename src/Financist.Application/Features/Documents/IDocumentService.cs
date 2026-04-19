namespace Financist.Application.Features.Documents;

public interface IDocumentService
{
    Task<DocumentImportDto> UploadAsync(UploadDocumentRequest request, CancellationToken cancellationToken = default);
}
