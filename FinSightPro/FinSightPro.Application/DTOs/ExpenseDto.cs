using FinSightPro.Domain.Enums;

namespace FinSightPro.Application.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryIcon { get; set; }
    public string? CategoryColor { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
}

public class ExpenseCreateDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cartao;
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public DateTime? RecurrenceEndDate { get; set; }
}

public class ExpenseFilter
{
    public int? CategoryId { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? Query { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "date";
    public bool SortDesc { get; set; } = true;
}
