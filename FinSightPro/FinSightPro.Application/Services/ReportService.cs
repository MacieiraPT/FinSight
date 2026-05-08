using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinSightPro.Application.Services;

public class ReportService : IReportService
{
    private readonly IExpenseRepository _expenses;
    private readonly IIncomeRepository _incomes;
    private readonly IExpenseService _expenseService;
    private static readonly CultureInfo PtCulture = new("pt-PT");

    public ReportService(IExpenseRepository expenses, IIncomeRepository incomes, IExpenseService expenseService)
    {
        _expenses = expenses;
        _incomes = incomes;
        _expenseService = expenseService;
    }

    public async Task<MonthlyReportDto> BuildMonthlyAsync(string userId, int year, int month, CancellationToken ct = default)
    {
        var (start, end) = MonthBounds(year, month);
        var expenses = await _expenses.Query(userId)
            .Where(e => e.Date >= start && e.Date < end)
            .Include(e => e.Category)
            .ToListAsync(ct);
        var income = await _incomes.Query(userId)
            .Where(i => i.Date >= start && i.Date < end)
            .SumAsync(i => (decimal?)i.Amount, ct) ?? 0m;

        var totalExpenses = expenses.Sum(e => e.Amount);
        var byCat = expenses
            .GroupBy(e => new { e.CategoryId, Name = e.Category?.Name ?? "—", Color = e.Category?.Color ?? "#6c757d", Icon = e.Category?.Icon ?? "fa-tag" })
            .Select(g => new CategoryTotalDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.Name,
                Color = g.Key.Color,
                Icon = g.Key.Icon,
                Total = g.Sum(e => e.Amount)
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        var today = DateTime.UtcNow;
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var daysElapsed = (today.Year == year && today.Month == month) ? today.Day : daysInMonth;
        var avgDaily = daysElapsed > 0 ? totalExpenses / daysElapsed : 0m;

        return new MonthlyReportDto
        {
            Year = year,
            Month = month,
            TotalIncome = income,
            TotalExpenses = totalExpenses,
            ByCategory = byCat,
            AverageDaily = Math.Round(avgDaily, 2),
            Projected = Math.Round(avgDaily * daysInMonth, 2),
            DaysElapsed = daysElapsed,
            DaysInMonth = daysInMonth,
            Expenses = expenses.OrderByDescending(e => e.Date).Select(e => new ExpenseDto
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount,
                Date = e.Date,
                CategoryId = e.CategoryId,
                CategoryName = e.Category?.Name,
                PaymentMethod = e.PaymentMethod,
                Notes = e.Notes
            }).ToList()
        };
    }

    public async Task<MonthComparisonDto> CompareMonthsAsync(string userId, int year, int month, CancellationToken ct = default)
    {
        var current = await BuildMonthlyAsync(userId, year, month, ct);
        var prev = new DateTime(year, month, 1).AddMonths(-1);
        var previous = await BuildMonthlyAsync(userId, prev.Year, prev.Month, ct);
        return new MonthComparisonDto
        {
            Current = current,
            Previous = previous,
            IncomeDeltaPercent = previous.TotalIncome == 0 ? 0 : Math.Round((current.TotalIncome - previous.TotalIncome) / previous.TotalIncome * 100, 1),
            ExpenseDeltaPercent = previous.TotalExpenses == 0 ? 0 : Math.Round((current.TotalExpenses - previous.TotalExpenses) / previous.TotalExpenses * 100, 1)
        };
    }

    public async Task<byte[]> ExportMonthlyExcelAsync(string userId, int year, int month, CancellationToken ct = default)
    {
        var report = await BuildMonthlyAsync(userId, year, month, ct);
        using var wb = new XLWorkbook();
        var summary = wb.Worksheets.Add("Resumo");
        summary.Cell(1, 1).Value = $"Relatório Mensal — {month:D2}/{year}";
        summary.Cell(1, 1).Style.Font.Bold = true;
        summary.Cell(1, 1).Style.Font.FontSize = 16;
        summary.Cell(3, 1).Value = "Receitas Totais"; summary.Cell(3, 2).Value = report.TotalIncome;
        summary.Cell(4, 1).Value = "Despesas Totais"; summary.Cell(4, 2).Value = report.TotalExpenses;
        summary.Cell(5, 1).Value = "Saldo"; summary.Cell(5, 2).Value = report.Balance;
        summary.Cell(6, 1).Value = "Média Diária"; summary.Cell(6, 2).Value = report.AverageDaily;
        summary.Cell(7, 1).Value = "Projeção Mensal"; summary.Cell(7, 2).Value = report.Projected;
        summary.Range("B3:B7").Style.NumberFormat.Format = "#,##0.00 €";
        summary.Cell(5, 2).Style.Font.Bold = true;
        summary.Cell(5, 2).Style.Fill.BackgroundColor = report.Balance >= 0 ? XLColor.LightGreen : XLColor.LightSalmon;

        summary.Cell(9, 1).Value = "Categoria";
        summary.Cell(9, 2).Value = "Total";
        summary.Range("A9:B9").Style.Font.Bold = true;
        summary.Range("A9:B9").Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
        summary.Range("A9:B9").Style.Font.FontColor = XLColor.White;
        for (int i = 0; i < report.ByCategory.Count; i++)
        {
            summary.Cell(10 + i, 1).Value = report.ByCategory[i].CategoryName;
            summary.Cell(10 + i, 2).Value = report.ByCategory[i].Total;
            summary.Cell(10 + i, 2).Style.NumberFormat.Format = "#,##0.00 €";
        }
        summary.Columns().AdjustToContents();

        var detail = wb.Worksheets.Add("Despesas");
        detail.Cell(1, 1).Value = "Data";
        detail.Cell(1, 2).Value = "Descrição";
        detail.Cell(1, 3).Value = "Categoria";
        detail.Cell(1, 4).Value = "Método";
        detail.Cell(1, 5).Value = "Valor";
        detail.Range("A1:E1").Style.Font.Bold = true;
        detail.Range("A1:E1").Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
        detail.Range("A1:E1").Style.Font.FontColor = XLColor.White;
        for (int i = 0; i < report.Expenses.Count; i++)
        {
            var e = report.Expenses[i];
            detail.Cell(i + 2, 1).Value = e.Date;
            detail.Cell(i + 2, 1).Style.DateFormat.Format = "yyyy-MM-dd";
            detail.Cell(i + 2, 2).Value = e.Description;
            detail.Cell(i + 2, 3).Value = e.CategoryName;
            detail.Cell(i + 2, 4).Value = e.PaymentMethod.ToString();
            detail.Cell(i + 2, 5).Value = e.Amount;
            detail.Cell(i + 2, 5).Style.NumberFormat.Format = "#,##0.00 €";
        }
        detail.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportMonthlyCsvAsync(string userId, int year, int month, CancellationToken ct = default)
    {
        var report = await BuildMonthlyAsync(userId, year, month, ct);
        var sb = new StringBuilder();
        sb.AppendLine($"Relatório Mensal;{month:D2}/{year}");
        sb.AppendLine();
        sb.AppendLine("Receitas;Despesas;Saldo;Média Diária;Projeção");
        sb.AppendLine($"{report.TotalIncome.ToString(PtCulture)};{report.TotalExpenses.ToString(PtCulture)};{report.Balance.ToString(PtCulture)};{report.AverageDaily.ToString(PtCulture)};{report.Projected.ToString(PtCulture)}");
        sb.AppendLine();
        sb.AppendLine("Data;Descrição;Categoria;Método;Valor");
        foreach (var e in report.Expenses)
        {
            sb.AppendLine($"{e.Date:yyyy-MM-dd};{Escape(e.Description)};{Escape(e.CategoryName ?? "")};{e.PaymentMethod};{e.Amount.ToString(PtCulture)}");
        }
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<byte[]> ExportExpensesCsvAsync(string userId, ExpenseFilter filter, CancellationToken ct = default)
    {
        var items = await _expenseService.ListForExportAsync(userId, filter, ct);
        var sb = new StringBuilder();
        sb.AppendLine("Data;Descrição;Categoria;Método;Valor;Notas");
        foreach (var e in items)
        {
            sb.AppendLine($"{e.Date:yyyy-MM-dd};{Escape(e.Description)};{Escape(e.CategoryName ?? "")};{e.PaymentMethod};{e.Amount.ToString(PtCulture)};{Escape(e.Notes ?? "")}");
        }
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    public async Task<byte[]> ExportExpensesExcelAsync(string userId, ExpenseFilter filter, CancellationToken ct = default)
    {
        var items = await _expenseService.ListForExportAsync(userId, filter, ct);
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Despesas");
        ws.Cell(1, 1).Value = "Data";
        ws.Cell(1, 2).Value = "Descrição";
        ws.Cell(1, 3).Value = "Categoria";
        ws.Cell(1, 4).Value = "Método";
        ws.Cell(1, 5).Value = "Valor";
        ws.Cell(1, 6).Value = "Notas";
        ws.Range("A1:F1").Style.Font.Bold = true;
        ws.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
        ws.Range("A1:F1").Style.Font.FontColor = XLColor.White;
        for (int i = 0; i < items.Count; i++)
        {
            var e = items[i];
            ws.Cell(i + 2, 1).Value = e.Date;
            ws.Cell(i + 2, 1).Style.DateFormat.Format = "yyyy-MM-dd";
            ws.Cell(i + 2, 2).Value = e.Description;
            ws.Cell(i + 2, 3).Value = e.CategoryName;
            ws.Cell(i + 2, 4).Value = e.PaymentMethod.ToString();
            ws.Cell(i + 2, 5).Value = e.Amount;
            ws.Cell(i + 2, 5).Style.NumberFormat.Format = "#,##0.00 €";
            ws.Cell(i + 2, 6).Value = e.Notes;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static (DateTime start, DateTime end) MonthBounds(int year, int month)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        return (start, start.AddMonths(1));
    }

    private static string Escape(string value)
    {
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}
