using Financist.Domain.Common;

namespace Financist.Domain.ValueObjects;

public sealed class Money : IEquatable<Money>
{
    private Money()
    {
        Currency = string.Empty;
    }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new DomainException("Money amount cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        currency = currency.Trim().ToUpperInvariant();

        if (currency.Length != 3)
        {
            throw new DomainException("Currency must be a 3-letter ISO code.");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; }

    public static Money Zero(string currency = "USD")
    {
        return new Money(0m, currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);

        if (other.Amount > Amount)
        {
            throw new DomainException("Money subtraction cannot result in a negative amount.");
        }

        return new Money(Amount - other.Amount, Currency);
    }

    public bool GreaterThanZero()
    {
        return Amount > 0;
    }

    public bool Equals(Money? other)
    {
        return other is not null &&
               Amount == other.Amount &&
               string.Equals(Currency, other.Currency, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Money);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            throw new DomainException("Money operations require the same currency.");
        }
    }
}
