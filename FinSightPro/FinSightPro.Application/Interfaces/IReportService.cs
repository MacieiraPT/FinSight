using FinSightPro.Application.DTOs;

namespace FinSightPro.Application.Interfaces;

public class MonthlyReportDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Balance => TotalIncome - TotalExpenses;
    public List<CategoryTotalDto> ByCategory { get; set; } = new();
    public decimal Projected { get; set; }
    public decimal AverageDaily { get; set; }
    public int DaysElapsed { get; set; }
    public int DaysInMonth { get; set; }
    public List<ExpenseDto> Expenses { get; set; } = new();
}

public class MonthComparisonDto
{
    public MonthlyReportDto Current { get; set; } = new();
    public MonthlyReportDto Previous { get; set; } = new();
    public decimal IncomeDeltaPercent { get; set; }
    public decimal ExpenseDeltaPercent { get; set; }
}

public interface IReportService
{
    Task<MonthlyReportDto> BuildMonthlyAsync(string userId, int year, int month, CancellationToken ct = default);
    Task<MonthComparisonDto> CompareMonthsAsync(string userId, int year, int month, CancellationToken ct = default);
    Task<byte[]> ExportMonthlyExcelAsync(string userId, int year, int month, CancellationToken ct = default);
    Task<byte[]> ExportMonthlyCsvAsync(string userId, int year, int month, CancellationToken ct = default);
    Task<byte[]> ExportExpensesCsvAsync(string userId, ExpenseFilter filter, CancellationToken ct = default);
    Task<byte[]> ExportExpensesExcelAsync(string userId, ExpenseFilter filter, CancellationToken ct = default);
}
