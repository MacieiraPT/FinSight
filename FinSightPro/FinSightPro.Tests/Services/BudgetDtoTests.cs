using FinSightPro.Application.DTOs;

namespace FinSightPro.Tests.Services;

public class BudgetDtoTests
{
    [Fact]
    public void PercentUsed_IsZeroWhenLimitIsZero()
    {
        var b = new BudgetDto { MonthlyLimit = 0, Spent = 100 };
        Assert.Equal(0m, b.PercentUsed);
    }

    [Fact]
    public void PercentUsed_RoundsToOneDecimal()
    {
        var b = new BudgetDto { MonthlyLimit = 200, Spent = 50 };
        Assert.Equal(25.0m, b.PercentUsed);
    }

    [Fact]
    public void Remaining_IsLimitMinusSpent()
    {
        var b = new BudgetDto { MonthlyLimit = 100, Spent = 30 };
        Assert.Equal(70m, b.Remaining);
    }
}
