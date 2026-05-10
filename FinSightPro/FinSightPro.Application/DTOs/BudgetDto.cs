namespace FinSightPro.Application.DTOs;

public class BudgetDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = "fa-tag";
    public string CategoryColor { get; set; } = "#6c757d";
    public decimal MonthlyLimit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining => MonthlyLimit - Spent;
    public decimal PercentUsed => MonthlyLimit == 0 ? 0 : Math.Round(Spent / MonthlyLimit * 100m, 1);
}

public class BudgetCreateDto
{
    public int CategoryId { get; set; }
    public decimal MonthlyLimit { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
}
