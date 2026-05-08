using FinSightPro.Application.Common;
using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinSightPro.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _repo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IUnitOfWork _uow;

    public BudgetService(
        IBudgetRepository repo,
        IExpenseRepository expenseRepo,
        ICategoryRepository categoryRepo,
        IUnitOfWork uow)
    {
        _repo = repo;
        _expenseRepo = expenseRepo;
        _categoryRepo = categoryRepo;
        _uow = uow;
    }

    public async Task<List<BudgetDto>> ListByMonthAsync(string userId, int year, int month, CancellationToken ct = default)
    {
        var budgets = await _repo.Query(userId)
            .Where(b => b.Year == year && b.Month == month)
            .Include(b => b.Category)
            .ToListAsync(ct);

        var first = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var last = first.AddMonths(1);

        var totals = await _expenseRepo.Query(userId)
            .Where(e => e.Date >= first && e.Date < last)
            .GroupBy(e => e.CategoryId)
            .Select(g => new { CategoryId = g.Key, Total = g.Sum(e => e.Amount) })
            .ToListAsync(ct);

        return budgets.Select(b =>
        {
            var spent = totals.FirstOrDefault(t => t.CategoryId == b.CategoryId)?.Total ?? 0m;
            return new BudgetDto
            {
                Id = b.Id,
                CategoryId = b.CategoryId,
                CategoryName = b.Category?.Name ?? string.Empty,
                CategoryIcon = b.Category?.Icon ?? "fa-tag",
                CategoryColor = b.Category?.Color ?? "#6c757d",
                MonthlyLimit = b.MonthlyLimit,
                Month = b.Month,
                Year = b.Year,
                Spent = spent
            };
        }).OrderByDescending(b => b.PercentUsed).ToList();
    }

    public async Task<BudgetDto?> GetByIdAsync(string userId, int id, CancellationToken ct = default)
    {
        var b = await _repo.GetByIdAsync(id, userId, ct);
        if (b == null) return null;
        return new BudgetDto
        {
            Id = b.Id,
            CategoryId = b.CategoryId,
            CategoryName = b.Category?.Name ?? string.Empty,
            CategoryIcon = b.Category?.Icon ?? "fa-tag",
            CategoryColor = b.Category?.Color ?? "#6c757d",
            MonthlyLimit = b.MonthlyLimit,
            Month = b.Month,
            Year = b.Year
        };
    }

    public async Task<Result<int>> CreateAsync(string userId, BudgetCreateDto dto, CancellationToken ct = default)
    {
        if (dto.MonthlyLimit <= 0) return Result<int>.Fail("O limite tem de ser positivo.");
        if (dto.Month < 1 || dto.Month > 12) return Result<int>.Fail("Mês inválido.");

        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId, userId, ct);
        if (category == null) return Result<int>.Fail("Categoria inválida.");

        if (await _repo.ExistsAsync(userId, dto.CategoryId, dto.Year, dto.Month, null, ct))
            return Result<int>.Fail("Já existe um orçamento para esta categoria neste mês.");

        var entity = new Budget
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            MonthlyLimit = dto.MonthlyLimit,
            Month = dto.Month,
            Year = dto.Year
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Ok(entity.Id);
    }

    public async Task<Result> UpdateAsync(string userId, int id, BudgetCreateDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, userId, ct);
        if (entity == null) return Result.Fail("Orçamento não encontrado.");
        if (dto.MonthlyLimit <= 0) return Result.Fail("O limite tem de ser positivo.");

        if (await _repo.ExistsAsync(userId, dto.CategoryId, dto.Year, dto.Month, id, ct))
            return Result.Fail("Já existe outro orçamento para esta categoria neste mês.");

        entity.CategoryId = dto.CategoryId;
        entity.MonthlyLimit = dto.MonthlyLimit;
        entity.Month = dto.Month;
        entity.Year = dto.Year;

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, userId, ct);
        if (entity == null) return Result.Fail("Orçamento não encontrado.");
        _repo.Remove(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
