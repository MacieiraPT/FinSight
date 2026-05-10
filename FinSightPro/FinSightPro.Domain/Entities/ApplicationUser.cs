using Microsoft.AspNetCore.Identity;

namespace FinSightPro.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;
    public decimal MonthlySalary { get; set; }
    public string Currency { get; set; } = "EUR";
    public string? ProfilePicturePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Theme { get; set; } = "light";
    public bool EnableBudgetAlerts { get; set; } = true;
    public int BudgetAlertThreshold { get; set; } = 90;

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Income> Incomes { get; set; } = new List<Income>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
