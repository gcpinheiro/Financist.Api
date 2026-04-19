namespace Financist.Application.Abstractions.Services;

public interface IDocumentAnalysisService
{
    Task<DocumentAnalysisResult> AnalyzeAsync(DocumentAnalysisRequest request, CancellationToken cancellationToken = default);
}
