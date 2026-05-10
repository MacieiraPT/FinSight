using FinSightPro.Application.Common;
using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinSightPro.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository repo, IUnitOfWork uow, ILogger<CategoryService> logger)
    {
        _repo = repo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> ListAsync(string userId, CancellationToken ct = default)
    {
        return await _repo.Query(userId)
            .OrderBy(c => c.ParentCategoryId == null ? 0 : 1)
            .ThenBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon,
                Color = c.Color,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                IsSystem = c.IsSystem
            })
            .ToListAsync(ct);
    }

    public async Task<CategoryDto?> GetByIdAsync(string userId, int id, CancellationToken ct = default)
    {
        var c = await _repo.GetByIdAsync(id, userId, ct);
        if (c == null) return null;
        return new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            Color = c.Color,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategory?.Name,
            IsSystem = c.IsSystem
        };
    }

    public async Task<Result<int>> CreateAsync(string userId, CategoryCreateDto dto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Result<int>.Fail("O nome é obrigatório.");

        if (await _repo.NameExistsAsync(userId, dto.Name.Trim(), null, ct))
            return Result<int>.Fail("Já existe uma categoria com este nome.");

        var entity = new Category
        {
            UserId = userId,
            Name = dto.Name.Trim(),
            Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "fa-tag" : dto.Icon,
            Color = string.IsNullOrWhiteSpace(dto.Color) ? "#6c757d" : dto.Color,
            ParentCategoryId = dto.ParentCategoryId,
            IsSystem = false
        };

        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Category {Id} created for user {User}", entity.Id, userId);
        return Result<int>.Ok(entity.Id);
    }

    public async Task<Result> UpdateAsync(string userId, int id, CategoryCreateDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, userId, ct);
        if (entity == null) return Result.Fail("Categoria não encontrada.");
        if (entity.IsSystem) return Result.Fail("Categorias do sistema não podem ser editadas.");

        if (!string.Equals(entity.Name, dto.Name.Trim(), StringComparison.OrdinalIgnoreCase) &&
            await _repo.NameExistsAsync(userId, dto.Name.Trim(), id, ct))
            return Result.Fail("Já existe uma categoria com este nome.");

        entity.Name = dto.Name.Trim();
        entity.Icon = string.IsNullOrWhiteSpace(dto.Icon) ? entity.Icon : dto.Icon;
        entity.Color = string.IsNullOrWhiteSpace(dto.Color) ? entity.Color : dto.Color;
        entity.ParentCategoryId = dto.ParentCategoryId;

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, userId, ct);
        if (entity == null) return Result.Fail("Categoria não encontrada.");
        if (entity.IsSystem) return Result.Fail("Categorias do sistema não podem ser eliminadas.");

        if (await _repo.HasExpensesAsync(id, userId, ct))
            return Result.Fail("Não é possível eliminar uma categoria com despesas associadas.");

        _repo.Remove(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> SeedDefaultAsync(string userId, CancellationToken ct = default)
    {
        var existing = await _repo.Query(userId).Select(c => c.Name).ToListAsync(ct);
        var defaults = DefaultCategories();
        var added = 0;
        foreach (var (name, icon, color) in defaults)
        {
            if (existing.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;
            await _repo.AddAsync(new Category
            {
                UserId = userId,
                Name = name,
                Icon = icon,
                Color = color,
                IsSystem = false
            }, ct);
            added++;
        }
        if (added > 0) await _uow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public static IEnumerable<(string Name, string Icon, string Color)> DefaultCategories() => new[]
    {
        ("Alimentação", "fa-utensils", "#e74c3c"),
        ("Transporte", "fa-car", "#3498db"),
        ("Saúde", "fa-heart-pulse", "#e84393"),
        ("Lazer", "fa-gamepad", "#9b59b6"),
        ("Habitação", "fa-house", "#16a085"),
        ("Educação", "fa-graduation-cap", "#f39c12"),
        ("Compras", "fa-bag-shopping", "#2ecc71"),
        ("Serviços", "fa-screwdriver-wrench", "#34495e"),
        ("Subscrições", "fa-rectangle-list", "#8e44ad"),
        ("Outros", "fa-ellipsis", "#7f8c8d")
    };
}
