namespace Financist.Application.Abstractions.Services;

public interface IDocumentTextExtractionService
{
    Task<string> ExtractTextAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
