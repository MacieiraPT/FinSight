using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestaoDespesas.Models;

public class Categoria
{
    public int CategoriaId { get; set; }

    [Required, StringLength(60)]
    public string Nome { get; set; } = string.Empty;

    [ScaffoldColumn(false)]
    public string UserId { get; set; } = string.Empty;

    public ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
    public ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();
}
