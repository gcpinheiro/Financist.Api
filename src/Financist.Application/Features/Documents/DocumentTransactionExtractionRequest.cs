namespace Financist.Application.Features.Documents;

public sealed record DocumentTransactionExtractionRequest(
    Guid UserId,
    Guid DocumentImportId,
    string Text,
    DateOnly ReferenceDate,
    string DefaultCurrency);
