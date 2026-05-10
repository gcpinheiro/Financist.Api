using Financist.Domain.Enums;

namespace Financist.Application.Features.Documents;

public sealed record ExtractedDocumentTransactionCandidate(
    string Description,
    decimal Amount,
    string Currency,
    TransactionType Type,
    DateOnly OccurredOn,
    string RawText,
    decimal Confidence,
    int? InstallmentNumber,
    int? InstallmentCount,
    string? InstallmentGroupKey,
    string ImportFingerprint);
