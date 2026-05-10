using System.ComponentModel.DataAnnotations;
using FinSightPro.Application.DTOs;
using FinSightPro.Domain.Enums;

namespace FinSightPro.Web.ViewModels;

public class IncomeListViewModel
{
    public List<IncomeDto> Items { get; set; } = new();
    public int? Year { get; set; }
    public int? Month { get; set; }
    public decimal Total => Items.Sum(i => i.Amount);
}

public class IncomeFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(200)]
    [Display(Name = "Descrição")]
    public string Description { get; set; } = string.Empty;

    [Required, Range(0.01, 9999999.99)]
    [Display(Name = "Valor (€)")]
    public decimal Amount { get; set; }

    [Required, DataType(DataType.Date)]
    [Display(Name = "Data")]
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;

    [Display(Name = "Categoria")]
    public int? CategoryId { get; set; }

    [Display(Name = "Rendimento Fixo")]
    public bool IsFixed { get; set; }

    [Display(Name = "Recorrente")]
    public bool IsRecurring { get; set; }

    [Display(Name = "Periodicidade")]
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.Monthly;

    [DataType(DataType.Date)]
    [Display(Name = "Termina em")]
    public DateTime? RecurrenceEndDate { get; set; }

    [StringLength(1000)]
    [Display(Name = "Notas")]
    public string? Notes { get; set; }

    public List<CategoryDto> Categories { get; set; } = new();
}
