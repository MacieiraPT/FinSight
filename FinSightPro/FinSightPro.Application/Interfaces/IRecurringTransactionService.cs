namespace FinSightPro.Application.Interfaces;

public interface IRecurringTransactionService
{
    Task<int> GenerateDueAsync(string userId, DateTime asOf, CancellationToken ct = default);
}
