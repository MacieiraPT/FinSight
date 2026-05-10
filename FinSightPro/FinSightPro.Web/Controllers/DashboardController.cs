using FinSightPro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinSightPro.Web.Controllers;

public class DashboardController : BaseAuthController
{
    private readonly IDashboardService _dashboard;
    private readonly IRecurringTransactionService _recurring;

    public DashboardController(IDashboardService dashboard, IRecurringTransactionService recurring)
    {
        _dashboard = dashboard;
        _recurring = recurring;
    }

    public async Task<IActionResult> Index(int? year, int? month, CancellationToken ct)
    {
        await _recurring.GenerateDueAsync(CurrentUserId, DateTime.UtcNow, ct);

        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        var data = await _dashboard.BuildAsync(CurrentUserId, y, m, ct);
        ViewBag.Year = y;
        ViewBag.Month = m;
        return View(data);
    }
}
