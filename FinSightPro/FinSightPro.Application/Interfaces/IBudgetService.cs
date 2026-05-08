using FinSightPro.Application.Common;
using FinSightPro.Application.DTOs;

namespace FinSightPro.Application.Interfaces;

public interface IBudgetService
{
    Task<List<BudgetDto>> ListByMonthAsync(string userId, int year, int month, CancellationToken ct = default);
    Task<BudgetDto?> GetByIdAsync(string userId, int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(string userId, BudgetCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(string userId, int id, BudgetCreateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default);
}
