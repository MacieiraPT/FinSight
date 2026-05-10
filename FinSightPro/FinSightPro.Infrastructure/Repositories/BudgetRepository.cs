using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using FinSightPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinSightPro.Infrastructure.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly ApplicationDbContext _db;
    public BudgetRepository(ApplicationDbContext db) => _db = db;

    public IQueryable<Budget> Query(string userId) =>
        _db.Budgets.AsNoTracking().Include(b => b.Category).Where(b => b.UserId == userId);

    public Task<Budget?> GetByIdAsync(int id, string userId, CancellationToken ct = default) =>
        _db.Budgets.Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId, ct);

    public Task<List<Budget>> ListByMonthAsync(string userId, int year, int month, CancellationToken ct = default) =>
        _db.Budgets.AsNoTracking()
            .Include(b => b.Category)
            .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(string userId, int categoryId, int year, int month, int? excludingId, CancellationToken ct = default) =>
        _db.Budgets.AnyAsync(b =>
            b.UserId == userId && b.CategoryId == categoryId && b.Year == year && b.Month == month &&
            (!excludingId.HasValue || b.Id != excludingId.Value), ct);

    public async Task AddAsync(Budget budget, CancellationToken ct = default) =>
        await _db.Budgets.AddAsync(budget, ct);

    public void Update(Budget budget) => _db.Budgets.Update(budget);
    public void Remove(Budget budget) => _db.Budgets.Remove(budget);
}
