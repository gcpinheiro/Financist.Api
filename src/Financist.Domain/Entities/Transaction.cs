using Financist.Domain.Common;
using Financist.Domain.Enums;
using Financist.Domain.ValueObjects;

namespace Financist.Domain.Entities;

public sealed class Transaction : AuditableEntity
{
    private Transaction()
    {
        Description = string.Empty;
        Amount = Money.Zero();
    }

    private Transaction(
        Guid userId,
        string description,
        Money amount,
        TransactionType type,
        DateOnly occurredOn,
        Guid? categoryId,
        Guid? cardId,
        string? notes)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Transaction user is required.");
        }

        UserId = userId;
        SetDescription(description);
        SetAmount(amount);
        SetType(type);
        SetOccurredOn(occurredOn);
        SetAssociations(categoryId, cardId);
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public Guid UserId { get; private set; }

    public string Description { get; private set; }

    public Money Amount { get; private set; }

    public TransactionType Type { get; private set; }

    public DateOnly OccurredOn { get; private set; }

    public Guid? CategoryId { get; private set; }

    public Guid? CardId { get; private set; }

    public string? Notes { get; private set; }

    public User? User { get; private set; }

    public Category? Category { get; private set; }

    public Card? Card { get; private set; }

    public static Transaction Create(
        Guid userId,
        string description,
        Money amount,
        TransactionType type,
        DateOnly occurredOn,
        Guid? categoryId,
        Guid? cardId,
        string? notes)
    {
        return new Transaction(userId, description, amount, type, occurredOn, categoryId, cardId, notes);
    }

    private void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Transaction description is required.");
        }

        Description = description.Trim();
    }

    private void SetAmount(Money amount)
    {
        if (!amount.GreaterThanZero())
        {
            throw new DomainException("Transaction amount must be greater than zero.");
        }

        Amount = amount;
    }

    private void SetType(TransactionType type)
    {
        if (type is not TransactionType.Income and not TransactionType.Expense)
        {
            throw new DomainException("Transaction type must be Income or Expense.");
        }

        Type = type;
    }

    private void SetOccurredOn(DateOnly occurredOn)
    {
        if (occurredOn > DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1))
        {
            throw new DomainException("Transaction date cannot be unreasonably far in the future.");
        }

        OccurredOn = occurredOn;
    }

    private void SetAssociations(Guid? categoryId, Guid? cardId)
    {
        if (cardId.HasValue && Type != TransactionType.Expense)
        {
            throw new DomainException("Only expense transactions can be linked to a card.");
        }

        CategoryId = categoryId;
        CardId = cardId;
    }
}
