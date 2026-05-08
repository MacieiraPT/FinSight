using FinSightPro.Application.Common;
using FinSightPro.Application.DTOs;

namespace FinSightPro.Application.Interfaces;

public interface IIncomeService
{
    Task<List<IncomeDto>> ListAsync(string userId, int? year, int? month, CancellationToken ct = default);
    Task<IncomeDto?> GetByIdAsync(string userId, int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(string userId, IncomeCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(string userId, int id, IncomeCreateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default);
}
