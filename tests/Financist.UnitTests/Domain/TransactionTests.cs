using Financist.Domain.Common;
using Financist.Domain.Entities;
using Financist.Domain.Enums;
using Financist.Domain.ValueObjects;

namespace Financist.UnitTests.Domain;

public sealed class TransactionTests
{
    [Fact]
    public void Create_ShouldThrow_WhenAmountIsZero()
    {
        var action = () => Transaction.Create(
            Guid.NewGuid(),
            "Salary",
            new Money(0m, "USD"),
            TransactionType.Income,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            null,
            null);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Transaction amount must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Create_ShouldThrow_WhenIncomeUsesCard()
    {
        var action = () => Transaction.Create(
            Guid.NewGuid(),
            "Salary",
            new Money(100m, "USD"),
            TransactionType.Income,
            DateOnly.FromDateTime(DateTime.UtcNow),
            null,
            Guid.NewGuid(),
            null);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal("Only expense transactions can be linked to a card.", exception.Message);
    }
}
