using System.Globalization;
using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinSightPro.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IExpenseRepository _expenses;
    private readonly IIncomeRepository _incomes;
    private readonly IBudgetService _budgetService;

    public DashboardService(
        IExpenseRepository expenses,
        IIncomeRepository incomes,
        IBudgetService budgetService)
    {
        _expenses = expenses;
        _incomes = incomes;
        _budgetService = budgetService;
    }

    public async Task<DashboardDto> BuildAsync(string userId, int year, int month, CancellationToken ct = default)
    {
        var ptCulture = new CultureInfo("pt-PT");

        var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var prevStart = monthStart.AddMonths(-1);
        var twelveMonthsAgo = monthStart.AddMonths(-11);

        var monthExpenses = await _expenses.Query(userId)
            .Where(e => e.Date >= monthStart && e.Date < monthEnd)
            .Include(e => e.Category)
            .ToListAsync(ct);

        var monthIncomes = await _incomes.Query(userId)
            .Where(i => i.Date >= monthStart && i.Date < monthEnd)
            .ToListAsync(ct);

        var prevExpensesTotal = await _expenses.Query(userId)
            .Where(e => e.Date >= prevStart && e.Date < monthStart)
            .SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;

        var totalExpenses = monthExpenses.Sum(e => e.Amount);
        var totalIncome = monthIncomes.Sum(i => i.Amount);

        var monthlyExpenses = await _expenses.Query(userId)
            .Where(e => e.Date >= twelveMonthsAgo && e.Date < monthEnd)
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(e => e.Amount) })
            .ToListAsync(ct);

        var monthlyIncomes = await _incomes.Query(userId)
            .Where(i => i.Date >= twelveMonthsAgo && i.Date < monthEnd)
            .GroupBy(i => new { i.Date.Year, i.Date.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(i => i.Amount) })
            .ToListAsync(ct);

        var months = new List<MonthlyTotalDto>();
        for (int i = 0; i < 12; i++)
        {
            var d = twelveMonthsAgo.AddMonths(i);
            var inc = monthlyIncomes.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0m;
            var exp = monthlyExpenses.FirstOrDefault(x => x.Year == d.Year && x.Month == d.Month)?.Total ?? 0m;
            months.Add(new MonthlyTotalDto
            {
                Year = d.Year,
                Month = d.Month,
                Label = d.ToString("MMM yy", ptCulture),
                Income = inc,
                Expenses = exp
            });
        }

        var byCategory = monthExpenses
            .GroupBy(e => new { e.CategoryId, Name = e.Category?.Name ?? "Sem categoria", Color = e.Category?.Color ?? "#6c757d", Icon = e.Category?.Icon ?? "fa-tag" })
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

        var topCategories = byCategory.Take(5).ToList();

        var recent = monthExpenses
            .OrderByDescending(e => e.Date)
            .Take(5)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Description = e.Description,
                Amount = e.Amount,
                Date = e.Date,
                CategoryId = e.CategoryId,
                CategoryName = e.Category?.Name,
                CategoryIcon = e.Category?.Icon,
                CategoryColor = e.Category?.Color,
                PaymentMethod = e.PaymentMethod
            })
            .ToList();

        var budgets = await _budgetService.ListByMonthAsync(userId, year, month, ct);

        var alerts = BuildAlerts(budgets, totalExpenses, prevExpensesTotal, totalIncome);

        var momPct = prevExpensesTotal == 0
            ? 0m
            : Math.Round((totalExpenses - prevExpensesTotal) / prevExpensesTotal * 100m, 1);

        var savingsRate = totalIncome <= 0
            ? 0m
            : Math.Round((totalIncome - totalExpenses) / totalIncome * 100m, 1);

        var today = DateTime.UtcNow;
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var daysElapsed = (year == today.Year && month == today.Month) ? today.Day : daysInMonth;
        var avgDaily = daysElapsed > 0 ? totalExpenses / daysElapsed : 0;
        var projected = Math.Round(avgDaily * daysInMonth, 2);

        return new DashboardDto
        {
            IncomeMonth = totalIncome,
            ExpensesMonth = totalExpenses,
            ExpensesPreviousMonth = prevExpensesTotal,
            MonthOverMonthPercent = momPct,
            SavingsRate = savingsRate,
            Last12Months = months,
            ExpensesByCategory = byCategory,
            BalanceTrend = months.Select(m => new MonthlyTotalDto
            {
                Year = m.Year,
                Month = m.Month,
                Label = m.Label,
                Income = m.Income,
                Expenses = m.Expenses
            }).ToList(),
            TopCategories = topCategories,
            RecentExpenses = recent,
            Budgets = budgets,
            Alerts = alerts,
            ProjectedMonthSpend = projected
        };
    }

    private static List<FinancialAlertDto> BuildAlerts(List<BudgetDto> budgets, decimal totalExpenses, decimal prevExpenses, decimal income)
    {
        var alerts = new List<FinancialAlertDto>();

        foreach (var b in budgets.Where(b => b.PercentUsed >= 75))
        {
            var sev = b.PercentUsed >= 100 ? "danger" : (b.PercentUsed >= 90 ? "warning" : "info");
            alerts.Add(new FinancialAlertDto
            {
                Title = $"Orçamento — {b.CategoryName}",
                Message = $"Já utilizaste {b.PercentUsed}% ({b.Spent:C} de {b.MonthlyLimit:C}).",
                Severity = sev,
                Icon = b.PercentUsed >= 100 ? "fa-circle-exclamation" : "fa-triangle-exclamation"
            });
        }

        if (prevExpenses > 0 && totalExpenses > prevExpenses * 1.2m)
        {
            alerts.Add(new FinancialAlertDto
            {
                Title = "Gastos acima da média",
                Message = $"Este mês já gastaste {(totalExpenses - prevExpenses):C} mais do que no mês anterior.",
                Severity = "warning",
                Icon = "fa-arrow-trend-up"
            });
        }

        if (income > 0 && totalExpenses > income)
        {
            alerts.Add(new FinancialAlertDto
            {
                Title = "Saldo negativo",
                Message = "As despesas deste mês ultrapassam as receitas registadas.",
                Severity = "danger",
                Icon = "fa-circle-exclamation"
            });
        }

        return alerts;
    }
}
