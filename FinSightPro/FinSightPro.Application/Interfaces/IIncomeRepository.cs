using FinSightPro.Domain.Entities;

namespace FinSightPro.Application.Interfaces;

public interface IIncomeRepository
{
    IQueryable<Income> Query(string userId);
    Task<Income?> GetByIdAsync(int id, string userId, CancellationToken ct = default);
    Task AddAsync(Income income, CancellationToken ct = default);
    void Update(Income income);
    void Remove(Income income);
    Task<List<Income>> ListAsync(string userId, DateTime from, DateTime to, CancellationToken ct = default);
    Task<List<Income>> ListRecurringTemplatesAsync(string userId, CancellationToken ct = default);
}
