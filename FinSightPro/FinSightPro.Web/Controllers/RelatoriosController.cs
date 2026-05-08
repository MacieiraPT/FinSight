using FinSightPro.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinSightPro.Web.Controllers;

public class RelatoriosController : BaseAuthController
{
    private readonly IReportService _reports;

    public RelatoriosController(IReportService reports) => _reports = reports;

    public async Task<IActionResult> Index(int? year, int? month, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        var data = await _reports.CompareMonthsAsync(CurrentUserId, y, m, ct);
        ViewBag.Year = y;
        ViewBag.Month = m;
        return View(data);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportarMensalCsv(int year, int month, CancellationToken ct)
    {
        var bytes = await _reports.ExportMonthlyCsvAsync(CurrentUserId, year, month, ct);
        return File(bytes, "text/csv", $"relatorio-{year}-{month:D2}.csv");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportarMensalExcel(int year, int month, CancellationToken ct)
    {
        var bytes = await _reports.ExportMonthlyExcelAsync(CurrentUserId, year, month, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"relatorio-{year}-{month:D2}.xlsx");
    }
}
