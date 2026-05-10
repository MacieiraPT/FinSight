using FinSightPro.Domain.Entities;

namespace FinSightPro.Application.Interfaces;

public interface ICategoryRepository
{
    IQueryable<Category> Query(string userId);
    Task<Category?> GetByIdAsync(int id, string userId, CancellationToken ct = default);
    Task<List<Category>> ListAsync(string userId, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string userId, string name, int? excludingId, CancellationToken ct = default);
    Task<bool> HasExpensesAsync(int categoryId, string userId, CancellationToken ct = default);
    Task AddAsync(Category category, CancellationToken ct = default);
    void Update(Category category);
    void Remove(Category category);
}
