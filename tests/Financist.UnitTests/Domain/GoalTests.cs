using Financist.Domain.Entities;
using Financist.Domain.ValueObjects;

namespace Financist.UnitTests.Domain;

public sealed class GoalTests
{
    [Fact]
    public void ProgressPercentage_ShouldBeCalculatedFromCurrentAndTargetAmounts()
    {
        var goal = Goal.Create(
            Guid.NewGuid(),
            "Emergency Fund",
            null,
            new Money(1000m, "USD"),
            new Money(250m, "USD"));

        Assert.Equal(25m, goal.ProgressPercentage);
    }

    [Fact]
    public void AddContribution_ShouldIncreaseCurrentAmount()
    {
        var goal = Goal.Create(
            Guid.NewGuid(),
            "Travel",
            null,
            new Money(2000m, "USD"),
            new Money(500m, "USD"));

        goal.AddContribution(new Money(250m, "USD"));

        Assert.Equal(750m, goal.CurrentAmount.Amount);
    }
}
