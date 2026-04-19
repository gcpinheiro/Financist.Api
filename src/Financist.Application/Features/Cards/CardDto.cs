namespace Financist.Application.Features.Cards;

public sealed record CardDto(
    Guid Id,
    string Name,
    string Last4Digits,
    decimal LimitAmount,
    string Currency,
    int ClosingDay,
    int DueDay);
