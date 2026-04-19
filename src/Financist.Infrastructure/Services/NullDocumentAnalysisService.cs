using Financist.Application.Abstractions.Services;

namespace Financist.Infrastructure.Services;

public sealed class NullDocumentAnalysisService : IDocumentAnalysisService
{
    private static readonly HashSet<string> SupportedContentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public Task<DocumentAnalysisResult> AnalyzeAsync(DocumentAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        var isSupported = SupportedContentTypes.Contains(request.ContentType.Trim().ToLowerInvariant());
        var message = isSupported
            ? "Document stored successfully. AI extraction is not enabled in this environment."
            : "Document stored successfully. This file type is not yet supported by the AI extraction pipeline.";

        return Task.FromResult(new DocumentAnalysisResult(isSupported, message));
    }
}
