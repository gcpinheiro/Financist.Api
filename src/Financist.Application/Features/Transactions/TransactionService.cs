using Financist.Application.Abstractions.Persistence;
using Financist.Application.Abstractions.Services;
using Financist.Application.Common.Exceptions;
using Financist.Domain.Entities;
using Financist.Domain.Enums;
using Financist.Domain.ValueObjects;

namespace Financist.Application.Features.Transactions;

public sealed class TransactionService : ITransactionService
{
    private readonly ICardRepository _cardRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransactionService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        ICardRepository cardRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _cardRepository = cardRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransactionDto> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var userId = _currentUserService.GetRequiredUserId();

        if (request.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId.Value, userId, cancellationToken);

            if (category is null)
            {
                throw new NotFoundException("Category was not found.");
            }

            if (category.Type != request.Type)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["categoryId"] = ["Category type must match the transaction type."]
                });
            }
        }

        if (request.CardId.HasValue)
        {
            var card = await _cardRepository.GetByIdAsync(request.CardId.Value, userId, cancellationToken);

            if (card is null)
            {
                throw new NotFoundException("Card was not found.");
            }

            if (request.Type != TransactionType.Expense)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    ["cardId"] = ["Only expense transactions can be linked to a card."]
                });
            }
        }

        var transaction = Transaction.Create(
            userId,
            request.Description.Trim(),
            new Money(request.Amount, request.Currency),
            request.Type,
            request.OccurredOn,
            request.CategoryId,
            request.CardId,
            request.Notes);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(transaction);
    }

    public async Task<IReadOnlyList<TransactionDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var transactions = await _transactionRepository.ListByUserAsync(userId, cancellationToken);
        return transactions.Select(Map).ToList();
    }

    private static void Validate(CreateTransactionRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors["description"] = ["Transaction description is required."];
        }

        if (request.Amount <= 0)
        {
            errors["amount"] = ["Transaction amount must be greater than zero."];
        }

        if (string.IsNullOrWhiteSpace(request.Currency))
        {
            errors["currency"] = ["Currency is required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static TransactionDto Map(Transaction transaction)
    {
        return new TransactionDto(
            transaction.Id,
            transaction.Description,
            transaction.Amount.Amount,
            transaction.Amount.Currency,
            transaction.Type,
            transaction.OccurredOn,
            transaction.CategoryId,
            transaction.CardId,
            transaction.Notes);
    }
}
