using Financist.Domain.Common;
using Financist.Domain.ValueObjects;

namespace Financist.Domain.Entities;

public sealed class Goal : AuditableEntity
{
    private Goal()
    {
        Name = string.Empty;
        TargetAmount = Money.Zero();
        CurrentAmount = Money.Zero();
    }

    private Goal(Guid userId, string name, string? description, Money targetAmount, Money currentAmount)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("Goal user is required.");
        }

        UserId = userId;
        SetName(name);
        Description = description?.Trim();
        SetTargetAmount(targetAmount);
        SetCurrentAmount(currentAmount);
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public Money TargetAmount { get; private set; }

    public Money CurrentAmount { get; private set; }

    public User? User { get; private set; }

    public decimal ProgressPercentage =>
        TargetAmount.Amount == 0
            ? 0
            : decimal.Round(Math.Min(100m, CurrentAmount.Amount / TargetAmount.Amount * 100m), 2, MidpointRounding.AwayFromZero);

    public static Goal Create(Guid userId, string name, string? description, Money targetAmount, Money currentAmount)
    {
        return new Goal(userId, name, description, targetAmount, currentAmount);
    }

    public void AddContribution(Money contribution)
    {
        if (!contribution.GreaterThanZero())
        {
            throw new DomainException("Goal contribution must be greater than zero.");
        }

        EnsureSameCurrency(contribution);
        CurrentAmount = CurrentAmount.Add(contribution);
        Touch();
    }

    public void SetCurrentAmount(Money currentAmount)
    {
        EnsureSameCurrency(currentAmount);
        CurrentAmount = currentAmount;
        Touch();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Goal name is required.");
        }

        Name = name.Trim();
    }

    private void SetTargetAmount(Money targetAmount)
    {
        if (!targetAmount.GreaterThanZero())
        {
            throw new DomainException("Goal target amount must be greater than zero.");
        }

        TargetAmount = targetAmount;
    }

    private void EnsureSameCurrency(Money money)
    {
        if (!string.Equals(TargetAmount.Currency, money.Currency, StringComparison.Ordinal))
        {
            throw new DomainException("Goal amounts must use the same currency.");
        }
    }
}
