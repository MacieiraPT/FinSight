using FinSightPro.Application.Common;
using FinSightPro.Application.DTOs;

namespace FinSightPro.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> ListAsync(string userId, CancellationToken ct = default);
    Task<CategoryDto?> GetByIdAsync(string userId, int id, CancellationToken ct = default);
    Task<Result<int>> CreateAsync(string userId, CategoryCreateDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(string userId, int id, CategoryCreateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default);
    Task<Result> SeedDefaultAsync(string userId, CancellationToken ct = default);
}
