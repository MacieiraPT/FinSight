using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using FinSightPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinSightPro.Infrastructure.Repositories;

public class IncomeRepository : IIncomeRepository
{
    private readonly ApplicationDbContext _db;
    public IncomeRepository(ApplicationDbContext db) => _db = db;

    public IQueryable<Income> Query(string userId) =>
        _db.Incomes.AsNoTracking().Include(i => i.Category).Where(i => i.UserId == userId);

    public Task<Income?> GetByIdAsync(int id, string userId, CancellationToken ct = default) =>
        _db.Incomes.Include(i => i.Category).FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId, ct);

    public async Task AddAsync(Income income, CancellationToken ct = default) =>
        await _db.Incomes.AddAsync(income, ct);

    public void Update(Income income) => _db.Incomes.Update(income);
    public void Remove(Income income) => _db.Incomes.Remove(income);

    public async Task<List<Income>> ListAsync(string userId, DateTime from, DateTime to, CancellationToken ct = default) =>
        await _db.Incomes.AsNoTracking()
            .Include(i => i.Category)
            .Where(i => i.UserId == userId && i.Date >= from && i.Date < to)
            .ToListAsync(ct);

    public async Task<List<Income>> ListRecurringTemplatesAsync(string userId, CancellationToken ct = default) =>
        await _db.Incomes
            .Where(i => i.UserId == userId && i.IsRecurring)
            .ToListAsync(ct);
}
