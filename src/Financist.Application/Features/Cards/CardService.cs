using Financist.Application.Abstractions.Persistence;
using Financist.Application.Abstractions.Services;
using Financist.Application.Common.Exceptions;
using Financist.Domain.Entities;
using Financist.Domain.ValueObjects;

namespace Financist.Application.Features.Cards;

public sealed class CardService : ICardService
{
    private readonly ICardRepository _cardRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CardService(
        ICardRepository cardRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _cardRepository = cardRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<CardDto> CreateAsync(CreateCardRequest request, CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = ["Card name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Last4Digits))
        {
            errors["last4Digits"] = ["Card last 4 digits are required."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        var userId = _currentUserService.GetRequiredUserId();
        var card = Card.Create(
            userId,
            request.Name.Trim(),
            request.Last4Digits.Trim(),
            new Money(request.LimitAmount, request.Currency),
            request.ClosingDay,
            request.DueDay);

        await _cardRepository.AddAsync(card, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(card);
    }

    public async Task<IReadOnlyList<CardDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var cards = await _cardRepository.ListByUserAsync(userId, cancellationToken);
        return cards.Select(Map).ToList();
    }

    private static CardDto Map(Card card)
    {
        return new CardDto(
            card.Id,
            card.Name,
            card.Last4Digits,
            card.Limit.Amount,
            card.Limit.Currency,
            card.ClosingDay,
            card.DueDay);
    }
}
