using FinSightPro.Domain.Entities;

namespace FinSightPro.Application.Interfaces;

public interface IBudgetRepository
{
    IQueryable<Budget> Query(string userId);
    Task<Budget?> GetByIdAsync(int id, string userId, CancellationToken ct = default);
    Task<List<Budget>> ListByMonthAsync(string userId, int year, int month, CancellationToken ct = default);
    Task<bool> ExistsAsync(string userId, int categoryId, int year, int month, int? excludingId, CancellationToken ct = default);
    Task AddAsync(Budget budget, CancellationToken ct = default);
    void Update(Budget budget);
    void Remove(Budget budget);
}
