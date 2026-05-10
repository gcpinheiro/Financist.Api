using Financist.Domain.Enums;

namespace Financist.Application.Features.Documents;

public sealed record ImportDocumentTransactionCandidateRequest(
    string Description,
    decimal Amount,
    string Currency,
    TransactionType Type,
    DateOnly OccurredOn);
