using FinSightPro.Application.Services;
using FinSightPro.Domain.Enums;

namespace FinSightPro.Tests.Services;

public class RecurringTransactionServiceTests
{
    [Theory]
    [InlineData(RecurrenceType.Daily, 1)]
    [InlineData(RecurrenceType.Weekly, 7)]
    public void NextOccurrence_AddsCorrectInterval(RecurrenceType type, int days)
    {
        var from = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var next = RecurringTransactionService.NextOccurrence(from, type);
        Assert.Equal(from.AddDays(days), next);
    }

    [Fact]
    public void NextOccurrence_Monthly_AddsOneMonth()
    {
        var from = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            RecurringTransactionService.NextOccurrence(from, RecurrenceType.Monthly));
    }

    [Fact]
    public void NextOccurrence_None_ReturnsMaxValue()
    {
        Assert.Equal(DateTime.MaxValue,
            RecurringTransactionService.NextOccurrence(DateTime.UtcNow, RecurrenceType.None));
    }
}
