using System.Text;
using Financist.Application.Abstractions.Services;
using Financist.Application.Common.Exceptions;
using UglyToad.PdfPig;

namespace Financist.Infrastructure.Services;

public sealed class PdfDocumentTextExtractionService : IDocumentTextExtractionService
{
    public Task<string> ExtractTextAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!IsPdf(fileName, contentType))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["file"] = ["Only PDF files can be indexed for RAG right now."]
            });
        }

        using var document = PdfDocument.Open(content);
        var textBuilder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.IsNullOrWhiteSpace(page.Text))
            {
                textBuilder.AppendLine(page.Text);
            }
        }

        return Task.FromResult(textBuilder.ToString());
    }

    private static bool IsPdf(string fileName, string contentType)
    {
        return string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase)
            || string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase);
    }
}
