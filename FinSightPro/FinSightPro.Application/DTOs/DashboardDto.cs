namespace FinSightPro.Application.DTOs;

public class DashboardDto
{
    public decimal IncomeMonth { get; set; }
    public decimal ExpensesMonth { get; set; }
    public decimal Balance => IncomeMonth - ExpensesMonth;
    public decimal ExpensesPreviousMonth { get; set; }
    public decimal MonthOverMonthPercent { get; set; }
    public decimal SavingsRate { get; set; }
    public List<MonthlyTotalDto> Last12Months { get; set; } = new();
    public List<CategoryTotalDto> ExpensesByCategory { get; set; } = new();
    public List<MonthlyTotalDto> BalanceTrend { get; set; } = new();
    public List<CategoryTotalDto> TopCategories { get; set; } = new();
    public List<ExpenseDto> RecentExpenses { get; set; } = new();
    public List<BudgetDto> Budgets { get; set; } = new();
    public List<FinancialAlertDto> Alerts { get; set; } = new();
    public decimal ProjectedMonthSpend { get; set; }
}

public class MonthlyTotalDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Income { get; set; }
    public decimal Expenses { get; set; }
    public decimal Balance => Income - Expenses;
}

public class CategoryTotalDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Color { get; set; } = "#6c757d";
    public string Icon { get; set; } = "fa-tag";
    public decimal Total { get; set; }
}

public class FinancialAlertDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "info";
    public string Icon { get; set; } = "fa-circle-info";
}
