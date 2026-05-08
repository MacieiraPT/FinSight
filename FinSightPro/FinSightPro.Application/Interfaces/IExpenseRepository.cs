using FinSightPro.Domain.Entities;

namespace FinSightPro.Application.Interfaces;

public interface IExpenseRepository
{
    IQueryable<Expense> Query(string userId);
    Task<Expense?> GetByIdAsync(int id, string userId, CancellationToken ct = default);
    Task AddAsync(Expense expense, CancellationToken ct = default);
    void Update(Expense expense);
    void Remove(Expense expense);
    Task<List<Expense>> ListAsync(string userId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<List<Expense>> ListRecurringTemplatesAsync(string userId, CancellationToken ct = default);
}
