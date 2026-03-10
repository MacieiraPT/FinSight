using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoDespesas.Models;

public class DespesaRecorrente
{
    public int DespesaRecorrenteId { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(120)]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Range(0.01, 99999999, ErrorMessage = "O valor deve ser entre 0,01 e 99 999 999.")]
    public decimal Valor { get; set; }

    [Required]
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public string? Observacoes { get; set; }

    [Required(ErrorMessage = "A frequência é obrigatória.")]
    [StringLength(20)]
    [Display(Name = "Frequência")]
    public string Frequencia { get; set; } = "Mensal"; // Semanal, Mensal, Anual

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data de início")]
    public DateTime DataInicio { get; set; } = DateTime.UtcNow;

    [DataType(DataType.Date)]
    [Display(Name = "Data de fim (opcional)")]
    public DateTime? DataFim { get; set; }

    [Display(Name = "Ativa")]
    public bool Ativa { get; set; } = true;

    [DataType(DataType.Date)]
    [Display(Name = "Última geração")]
    public DateTime? UltimaGeracao { get; set; }

    [ScaffoldColumn(false)]
    public string UserId { get; set; } = string.Empty;
}
