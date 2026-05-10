using FinSightPro.Application.Common;
using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using FinSightPro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinSightPro.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _repo;
    private readonly ICategoryRepository _categoryRepo;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IExpenseRepository repo,
        ICategoryRepository categoryRepo,
        IUnitOfWork uow,
        ILogger<ExpenseService> logger)
    {
        _repo = repo;
        _categoryRepo = categoryRepo;
        _uow = uow;
        _logger = logger;
    }

    public async Task<PagedResult<ExpenseDto>> GetPagedAsync(string userId, ExpenseFilter filter, CancellationToken ct = default)
    {
        var q = ApplyFilter(_repo.Query(userId), filter);

        var totalCount = await q.CountAsync(ct);
        q = ApplySort(q, filter);

        var items = await q
            .Skip((Math.Max(filter.Page, 1) - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount,
                Date = e.Date,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                CategoryIcon = e.Category != null ? e.Category.Icon : null,
                CategoryColor = e.Category != null ? e.Category.Color : null,
                PaymentMethod = e.PaymentMethod,
                Notes = e.Notes,
                AttachmentPath = e.AttachmentPath,
                IsRecurring = e.IsRecurring,
                RecurrenceType = e.RecurrenceType
            })
            .ToListAsync(ct);

        return new PagedResult<ExpenseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    public async Task<ExpenseDto?> GetByIdAsync(string userId, int id, CancellationToken ct = default)
    {
        var e = await _repo.Query(userId)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null) return null;
        return new ExpenseDto
        {
            Id = e.Id,
            Description = e.Description,
            Amount = e.Amount,
            Date = e.Date,
            CategoryId = e.CategoryId,
            CategoryName = e.Category?.Name,
            CategoryIcon = e.Category?.Icon,
            CategoryColor = e.Category?.Color,
            PaymentMethod = e.PaymentMethod,
            Notes = e.Notes,
            AttachmentPath = e.AttachmentPath,
            IsRecurring = e.IsRecurring,
            RecurrenceType = e.RecurrenceType
        };
    }

    public async Task<Result<int>> CreateAsync(string userId, ExpenseCreateDto dto, CancellationToken ct = default)
    {
        if (dto.Amount <= 0) return Result<int>.Fail("O valor tem de ser positivo.");
        if (dto.Date.Date > DateTime.UtcNow.Date) return Result<int>.Fail("A data não pode ser futura.");
        if (string.IsNullOrWhiteSpace(dto.Description)) return Result<int>.Fail("A descrição é obrigatória.");

        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId, userId, ct);
        if (category == null) return Result<int>.Fail("Categoria inválida.");

        var entity = new Expense
        {
            UserId = userId,
            CategoryId = dto.CategoryId,
            Description = dto.Description.Trim(),
            Amount = dto.Amount,
            Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
            PaymentMethod = dto.PaymentMethod,
            Notes = dto.Notes,
            AttachmentPath = dto.AttachmentPath,
            IsRecurring = dto.IsRecurring,
            RecurrenceType = dto.IsRecurring ? dto.RecurrenceType : RecurrenceType.None,
            RecurrenceEndDate = dto.RecurrenceEndDate
        };
        await _repo.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Expense {Id} created for user {User}", entity.Id, userId);
        return Result<int>.Ok(entity.Id);
    }

    public async Task<Result> UpdateAsync(string userId, int id, ExpenseCreateDto dto, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, userId, ct);
        if (entity == null) return Result.Fail("Despesa não encontrada.");
        if (dto.Amount <= 0) return Result.Fail("O valor tem de ser positivo.");
        if (dto.Date.Date > DateTime.UtcNow.Date) return Result.Fail("A data não pode ser futura.");

        var category = await _categoryRepo.GetByIdAsync(dto.CategoryId, userId, ct);
        if (category == null) return Result.Fail("Categoria inválida.");

        entity.Description = dto.Description.Trim();
        entity.Amount = dto.Amount;
        entity.Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc);
        entity.CategoryId = dto.CategoryId;
        entity.PaymentMethod = dto.PaymentMethod;
        entity.Notes = dto.Notes;
        if (!string.IsNullOrWhiteSpace(dto.AttachmentPath)) entity.AttachmentPath = dto.AttachmentPath;
        entity.IsRecurring = dto.IsRecurring;
        entity.RecurrenceType = dto.IsRecurring ? dto.RecurrenceType : RecurrenceType.None;
        entity.RecurrenceEndDate = dto.RecurrenceEndDate;

        _repo.Update(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdAsync(id, userId, ct);
        if (entity == null) return Result.Fail("Despesa não encontrada.");
        _repo.Remove(entity);
        await _uow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<List<ExpenseDto>> ListForExportAsync(string userId, ExpenseFilter filter, CancellationToken ct = default)
    {
        var q = ApplyFilter(_repo.Query(userId), filter);
        q = ApplySort(q, filter);
        return await q.Select(e => new ExpenseDto
        {
            Id = e.Id,
            Description = e.Description,
            Amount = e.Amount,
            Date = e.Date,
            CategoryId = e.CategoryId,
            CategoryName = e.Category != null ? e.Category.Name : null,
            PaymentMethod = e.PaymentMethod,
            Notes = e.Notes
        }).ToListAsync(ct);
    }

    private static IQueryable<Expense> ApplyFilter(IQueryable<Expense> q, ExpenseFilter f)
    {
        if (f.CategoryId.HasValue) q = q.Where(e => e.CategoryId == f.CategoryId.Value);
        if (f.PaymentMethod.HasValue) q = q.Where(e => e.PaymentMethod == f.PaymentMethod.Value);
        if (f.From.HasValue) q = q.Where(e => e.Date >= f.From.Value);
        if (f.To.HasValue) q = q.Where(e => e.Date <= f.To.Value);
        if (f.MinAmount.HasValue) q = q.Where(e => e.Amount >= f.MinAmount.Value);
        if (f.MaxAmount.HasValue) q = q.Where(e => e.Amount <= f.MaxAmount.Value);
        if (!string.IsNullOrWhiteSpace(f.Query))
        {
            var query = f.Query.Trim().ToLower();
            q = q.Where(e =>
                e.Description.ToLower().Contains(query) ||
                (e.Notes != null && e.Notes.ToLower().Contains(query)));
        }
        return q;
    }

    private static IQueryable<Expense> ApplySort(IQueryable<Expense> q, ExpenseFilter f)
    {
        return (f.SortBy?.ToLowerInvariant(), f.SortDesc) switch
        {
            ("amount", true) => q.OrderByDescending(e => e.Amount),
            ("amount", false) => q.OrderBy(e => e.Amount),
            ("description", true) => q.OrderByDescending(e => e.Description),
            ("description", false) => q.OrderBy(e => e.Description),
            ("category", true) => q.OrderByDescending(e => e.Category!.Name),
            ("category", false) => q.OrderBy(e => e.Category!.Name),
            (_, false) => q.OrderBy(e => e.Date),
            _ => q.OrderByDescending(e => e.Date)
        };
    }
}
