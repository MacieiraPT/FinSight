using FinSightPro.Application.Common;
using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using FinSightPro.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinSightPro.Application.Services;

public class IncomeService : IIncomeService
{
    private readonly IIncomeRepository _repo;
    private readonly IUnitOfWork _uow;

    public IncomeService(IIncomeRepository repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public async Task<List<IncomeDto>> ListAsync(string userId, int? year, int? month, CancellationToken ct = default)
    {
        var q = _repo.Query(userId);
        if (year.HasValue) q = q.Where(i => i.Date.Year == year.Value);
        if (month.HasValue) q = q.Where(i => i.Date.Month == month.Value);
        return await q.OrderByDescending(i => i.Date)
            .Select(i => new IncomeDto
            {
                Id = i.Id,
                Description = i.Description,
                Amount = i.Amount,
                Date = i.Date,
                CategoryId = i.CategoryId,
                CategoryName = i.Category != null ? i.Category.Name : null,
                IsFixed = i.IsFixed,
                IsRecurring = i.IsRecurring,
                RecurrenceType = i.RecurrenceType,
                Notes = i.Notes
            })
            .ToListAsync(ct);
    }

    public async Task<IncomeDto?> GetByIdAsync(string userId, int id, CancellationToken ct = default)
    {
        var i = await _repo.GetByIdAsync(id, userId, ct);
        if (i == null) return null;
        return new IncomeDto
        {
            Id = i.Id,
            Description = i.Description,
            Amount = i.Amount,
            Date = i.Date,
            CategoryId = i.CategoryId,
            CategoryName = i.Category?.Name,
            IsFixed = i.IsFixed,
            IsRecurring = i.IsRecurring,
            RecurrenceType = i.RecurrenceType,
            Notes = i.Notes
        };
    }

    public async Task<Result<int>> CreateAsync(string userId, IncomeCreateDto dto, CancellationToken ct = default)
    {
        if (dto.Amount <= 0) return Result<int>.Fail("O valor tem de ser positivo.");
        if (string.IsNullOrWhiteSpace(dto.Description)) return Result<int>.Fail("A descrição é obrigatória.");

        var entity = new Income
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Description = dto.Description.Trim(),
            Amount = dto.Amount,
            Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
            IsFixed = dto.IsFixed,
            IsRecurring = dto.IsRecurring,
            RecurrenceType = dto.IsRecurring ? dto.RecurrenceType : RecurrenceType.None,
            RecurrenceEndDate = dto.RecurrenceEndDate,
            Notes = dto.Notes
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<int>.Ok(entity.Id);
    }

    public async Task<Result> UpdateAsync(string userId, int id, IncomeCreateDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, userId, ct);
        if (entity == null) return Result.Fail("Receita não encontrada.");
        if (dto.Amount <= 0) return Result.Fail("O valor tem de ser positivo.");

        entity.Description = dto.Description.Trim();
        entity.Amount = dto.Amount;
        entity.Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc);
        entity.CategoryId = dto.CategoryId;
        entity.IsFixed = dto.IsFixed;
        entity.IsRecurring = dto.IsRecurring;
        entity.RecurrenceType = dto.IsRecurring ? dto.RecurrenceType : RecurrenceType.None;
        entity.RecurrenceEndDate = dto.RecurrenceEndDate;
        entity.Notes = dto.Notes;

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, userId, ct);
        if (entity == null) return Result.Fail("Receita não encontrada.");
        _repo.Remove(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
