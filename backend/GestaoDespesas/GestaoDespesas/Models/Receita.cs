using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoDespesas.Models;

public class Receita
{
    public int ReceitaId { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(120)]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O valor é obrigatório.")]
    [Range(0.01, 99999999, ErrorMessage = "O valor deve ser entre 0,01 e 99 999 999.")]
    public decimal Valor { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "O tipo é obrigatório.")]
    [StringLength(60)]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = string.Empty;

    [ScaffoldColumn(false)]
    public string UserId { get; set; } = string.Empty;
}
