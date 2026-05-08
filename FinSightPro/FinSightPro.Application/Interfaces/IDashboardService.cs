using FinSightPro.Application.DTOs;

namespace FinSightPro.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> BuildAsync(string userId, int year, int month, CancellationToken ct = default);
}
