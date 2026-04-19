using Financist.Domain.Common;
using Financist.Domain.ValueObjects;

namespace Financist.UnitTests.Domain;

public sealed class MoneyTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenAmountIsNegative()
    {
        var action = () => new Money(-1m, "USD");

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Money amount cannot be negative.", exception.Message);
    }

    [Fact]
    public void Add_ShouldReturnCombinedAmount_WhenCurrencyMatches()
    {
        var left = new Money(10m, "USD");
        var right = new Money(2.35m, "USD");

        var result = left.Add(right);

        Assert.Equal(12.35m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Subtract_ShouldThrow_WhenResultWouldBeNegative()
    {
        var action = () => new Money(5m, "USD").Subtract(new Money(10m, "USD"));

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Money subtraction cannot result in a negative amount.", exception.Message);
    }
}
