namespace Financist.Application.Features.Documents;

public interface IDocumentTransactionExtractionService
{
    IReadOnlyList<ExtractedDocumentTransactionCandidate> Extract(DocumentTransactionExtractionRequest request);
}
