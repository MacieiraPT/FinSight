using FinSightPro.Application.Common;
using FinSightPro.Application.DTOs;

namespace FinSightPro.Application.Interfaces;

public interface IExpenseService
{
    Task<PagedResult<ExpenseDto>> GetPagedAsync(string userId, ExpenseFilter filter, CancellationToken ct = default);
    Task<ExpenseDto?> GetByIdAsync(string userId, int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(string userId, ExpenseCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(string userId, int id, ExpenseCreateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default);
    Task<List<ExpenseDto>> ListForExportAsync(string userId, ExpenseFilter filter, CancellationToken ct = default);
}
