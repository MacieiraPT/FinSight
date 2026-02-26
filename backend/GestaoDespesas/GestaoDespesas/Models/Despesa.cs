using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoDespesas.Models;

public class Despesa
{
    public int DespesaId { get; set; }

    [Required, StringLength(120)]
    public string Descricao { get; set; } = string.Empty;

    [Range(0.01, 99999999)]
    public decimal Valor { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime Data { get; set; } = DateTime.UtcNow;

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public string? Observacoes { get; set; }

    [ScaffoldColumn(false)]
    public string UserId { get; set; } = string.Empty;
}
