using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using FinSightPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinSightPro.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly ApplicationDbContext _db;
    public ExpenseRepository(ApplicationDbContext db) => _db = db;

    public IQueryable<Expense> Query(string userId) =>
        _db.Expenses.AsNoTracking().Include(e => e.Category).Where(e => e.UserId == userId);

    public Task<Expense?> GetByIdAsync(int id, string userId, CancellationToken ct = default) =>
        _db.Expenses.Include(e => e.Category).FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId, ct);

    public async Task AddAsync(Expense expense, CancellationToken ct = default) =>
        await _db.Expenses.AddAsync(expense, ct);

    public void Update(Expense expense) => _db.Expenses.Update(expense);
    public void Remove(Expense expense) => _db.Expenses.Remove(expense);

    public async Task<List<Expense>> ListAsync(string userId, DateTime from, DateTime to, CancellationToken ct = default) =>
        await _db.Expenses.AsNoTracking()
            .Include(e => e.Category)
            .Where(e => e.UserId == userId && e.Date >= from && e.Date < to)
            .OrderBy(e => e.Date)
            .ToListAsync(ct);

    public async Task<List<Expense>> ListRecurringTemplatesAsync(string userId, CancellationToken ct = default) =>
        await _db.Expenses
            .Where(e => e.UserId == userId && e.IsRecurring)
            .ToListAsync(ct);
}
