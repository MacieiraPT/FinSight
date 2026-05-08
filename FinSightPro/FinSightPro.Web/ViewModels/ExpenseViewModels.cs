using System.ComponentModel.DataAnnotations;
using FinSightPro.Application.Common;
using FinSightPro.Application.DTOs;
using FinSightPro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace FinSightPro.Web.ViewModels;

public class ExpenseListViewModel
{
    public PagedResult<ExpenseDto> Page { get; set; } = new();
    public ExpenseFilter Filter { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = new();
}

public class ExpenseFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(200)]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;

    [Required, Range(0.01, 9999999.99, ErrorMessage = "Valor inválido.")]
    [Display(Name = "Valor (€)")]
    public decimal Amount { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Data")]
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [Display(Name = "Categoria")]
    public int CategoryId { get; set; }

    [Display(Name = "Método de Pagamento")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cartao;

    [StringLength(1000)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    [Display(Name = "Comprovativo")]
    public IFormFile? Attachment { get; set; }

    public string? ExistingAttachmentPath { get; set; }

    [Display(Name = "Recorrente")]
    public bool IsRecurring { get; set; }

    [Display(Name = "Periodicidade")]
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Monthly;

    [DataType(DataType.Date)]
    [Display(Name = "Termina em")]
    public DateTime? RecurrenceEndDate { get; set; }

    public List<CategoryDto> Categories { get; set; } = new();
}
