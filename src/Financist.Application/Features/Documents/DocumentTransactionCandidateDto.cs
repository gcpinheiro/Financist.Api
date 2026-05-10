using Financist.Domain.Enums;

namespace Financist.Application.Features.Documents;

public sealed record DocumentTransactionCandidateDto(
    Guid Id,
    Guid DocumentImportId,
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
    string ImportFingerprint,
    DocumentTransactionCandidateStatus Status,
    Guid? TransactionId);
