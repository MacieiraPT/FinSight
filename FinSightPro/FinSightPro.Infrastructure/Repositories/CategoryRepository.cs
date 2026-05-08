using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using FinSightPro.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinSightPro.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _db;
    public CategoryRepository(ApplicationDbContext db) => _db = db;

    public IQueryable<Category> Query(string userId) =>
        _db.Categories.AsNoTracking().Include(c => c.ParentCategory).Where(c => c.UserId == userId);

    public Task<Category?> GetByIdAsync(int id, string userId, CancellationToken ct = default) =>
        _db.Categories.Include(c => c.ParentCategory).FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    public Task<List<Category>> ListAsync(string userId, CancellationToken ct = default) =>
        _db.Categories.AsNoTracking().Where(c => c.UserId == userId).OrderBy(c => c.Name).ToListAsync(ct);

    public Task<bool> NameExistsAsync(string userId, string name, int? excludingId, CancellationToken ct = default) =>
        _db.Categories.AnyAsync(c =>
            c.UserId == userId &&
            c.Name.ToLower() == name.ToLower() &&
            (!excludingId.HasValue || c.Id != excludingId.Value), ct);

    public Task<bool> HasExpensesAsync(int categoryId, string userId, CancellationToken ct = default) =>
        _db.Expenses.AnyAsync(e => e.CategoryId == categoryId && e.UserId == userId, ct);

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await _db.Categories.AddAsync(category, ct);

    public void Update(Category category) => _db.Categories.Update(category);
    public void Remove(Category category) => _db.Categories.Remove(category);
}
