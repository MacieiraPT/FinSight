using System.ComponentModel.DataAnnotations;
using FinSightPro.Application.DTOs;

namespace FinSightPro.Web.ViewModels;

public class BudgetListViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<BudgetDto> Items { get; set; } = new();
    public decimal TotalLimit => Items.Sum(b => b.MonthlyLimit);
    public decimal TotalSpent => Items.Sum(b => b.Spent);
}

public class BudgetFormViewModel
{
    public int? Id { get; set; }

    [Required, Display(Name = "Categoria")]
    public int CategoryId { get; set; }

    [Required, Range(0.01, 9999999.99), Display(Name = "Limite Mensal (€)")]
    public decimal MonthlyLimit { get; set; }

    [Required, Range(1, 12), Display(Name = "Mês")]
    public int Month { get; set; } = DateTime.UtcNow.Month;

    [Required, Range(2000, 2100), Display(Name = "Ano")]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    public List<CategoryDto> Categories { get; set; } = new();
}
