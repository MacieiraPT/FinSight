using System.ComponentModel.DataAnnotations;

namespace FinSightPro.Domain.Entities;

public class Budget
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Range(0.01, 9999999.99)]
    public decimal MonthlyLimit { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(2000, 2100)]
    public int Year { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
