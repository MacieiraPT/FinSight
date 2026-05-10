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

    [StringLength(40)]
    [Display(Name = "Ícone")]
    public string Icone { get; set; } = "bi-tag";

    [StringLength(20)]
    [Display(Name = "Cor")]
    public string Cor { get; set; } = "#6c757d";

    [Display(Name = "Categoria-pai")]
    public int? CategoriaPaiId { get; set; }

    [JsonIgnore]
    public Categoria? CategoriaPai { get; set; }

    [JsonIgnore]
    public ICollection<Categoria> SubCategorias { get; set; } = new List<Categoria>();

    [ScaffoldColumn(false)]
    public string UserId { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();

    [JsonIgnore]
    public ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();
}
