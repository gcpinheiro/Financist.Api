using Financist.Domain.Common;
using Financist.Domain.ValueObjects;

namespace Financist.Domain.Entities;

public sealed class Card : AuditableEntity
{
    private Card()
    {
        Name = string.Empty;
        Last4Digits = string.Empty;
        Limit = Money.Zero();
    }

    private Card(Guid userId, string name, string last4Digits, Money limit, int closingDay, int dueDay)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Card user is required.");
        }

        UserId = userId;
        Rename(name);
        SetLast4Digits(last4Digits);
        SetLimit(limit);
        SetBillingCycle(closingDay, dueDay);
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public string Last4Digits { get; private set; }

    public Money Limit { get; private set; }

    public int ClosingDay { get; private set; }

    public int DueDay { get; private set; }

    public User? User { get; private set; }

    public static Card Create(Guid userId, string name, string last4Digits, Money limit, int closingDay, int dueDay)
    {
        return new Card(userId, name, last4Digits, limit, closingDay, dueDay);
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Card name is required.");
        }

        Name = name.Trim();
        Touch();
    }

    public void ChangeLimit(Money limit)
    {
        SetLimit(limit);
        Touch();
    }

    public void ChangeBillingCycle(int closingDay, int dueDay)
    {
        SetBillingCycle(closingDay, dueDay);
        Touch();
    }

    private void SetLast4Digits(string last4Digits)
    {
        if (string.IsNullOrWhiteSpace(last4Digits) || last4Digits.Trim().Length != 4 || !last4Digits.All(char.IsDigit))
        {
            throw new DomainException("Card last four digits must be exactly 4 numeric characters.");
        }

        Last4Digits = last4Digits.Trim();
    }

    private void SetLimit(Money limit)
    {
        if (!limit.GreaterThanZero())
        {
            throw new DomainException("Card limit must be greater than zero.");
        }

        Limit = limit;
    }

    private void SetBillingCycle(int closingDay, int dueDay)
    {
        if (closingDay < 1 || closingDay > 31)
        {
            throw new DomainException("Card closing day must be between 1 and 31.");
        }

        if (dueDay < 1 || dueDay > 31)
        {
            throw new DomainException("Card due day must be between 1 and 31.");
        }

        ClosingDay = closingDay;
        DueDay = dueDay;
    }
}
