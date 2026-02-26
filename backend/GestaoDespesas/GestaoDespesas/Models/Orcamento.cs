using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoDespesas.Models;

public class Orcamento
{
    public int OrcamentoId { get; set; }

    [Range(2000, 2100)]
    public int Ano { get; set; }

    [Range(1, 12)]
    public int Mes { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    [Range(0, 99999999)]
    public decimal Limite { get; set; }

    [ScaffoldColumn(false)]
    public string UserId { get; set; } = string.Empty;
}
