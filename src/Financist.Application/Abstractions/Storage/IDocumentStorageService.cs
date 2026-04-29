namespace Financist.Application.Abstractions.Storage;

public interface IDocumentStorageService
{
    Task<StoredDocument> SaveAsync(
        Stream content,
        string originalFileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}
