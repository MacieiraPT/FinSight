namespace FinSightPro.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-tag";
    public string Color { get; set; } = "#6c757d";
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsSystem { get; set; }
}

public class CategoryCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-tag";
    public string Color { get; set; } = "#6c757d";
    public int? ParentCategoryId { get; set; }
}
