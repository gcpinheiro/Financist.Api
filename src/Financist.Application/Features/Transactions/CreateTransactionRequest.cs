using Financist.Domain.Enums;

namespace Financist.Application.Features.Transactions;

public sealed record CreateTransactionRequest(
    string Description,
    decimal Amount,
    string Currency,
    TransactionType Type,
    DateOnly OccurredOn,
    Guid? CategoryId,
    Guid? CardId,
    string? Notes);
