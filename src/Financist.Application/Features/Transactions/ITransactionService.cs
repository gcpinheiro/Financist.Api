namespace Financist.Application.Features.Transactions;

public interface ITransactionService
{
    Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TransactionDto>> ListAsync(CancellationToken cancellationToken = default);
}
