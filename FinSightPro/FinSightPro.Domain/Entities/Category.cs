using System.ComponentModel.DataAnnotations;

namespace FinSightPro.Domain.Entities;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [StringLength(40)]
    public string Icon { get; set; } = "fa-tag";

    [StringLength(20)]
    public string Color { get; set; } = "#6c757d";

    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public bool IsSystem { get; set; }

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Income> Incomes { get; set; } = new List<Income>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
