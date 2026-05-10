using System.ComponentModel.DataAnnotations;
using FinSightPro.Application.DTOs;

namespace FinSightPro.Web.ViewModels;

public class CategoryFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(60)]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Ícone (FontAwesome)")]
    [StringLength(40)]
    public string Icon { get; set; } = "fa-tag";

    [Display(Name = "Cor")]
    [StringLength(20)]
    public string Color { get; set; } = "#6c757d";

    [Display(Name = "Categoria-Pai")]
    public int? ParentCategoryId { get; set; }

    public bool IsSystem { get; set; }

    public List<CategoryDto> AvailableParents { get; set; } = new();
}
