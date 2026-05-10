using System.ComponentModel.DataAnnotations;
using FinSightPro.Domain.Enums;

namespace FinSightPro.Domain.Entities;

public class Income
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required, StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 9999999.99)]
    public decimal Amount { get; set; }

    public DateTime Date { get; set; }

    public bool IsFixed { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public DateTime? RecurrenceEndDate { get; set; }
    public int? ParentRecurringId { get; set; }
    public Income? ParentRecurring { get; set; }
    public DateTime? LastGeneratedAt { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
