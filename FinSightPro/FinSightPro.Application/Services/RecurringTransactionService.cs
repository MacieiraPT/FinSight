using FinSightPro.Application.Interfaces;
using FinSightPro.Domain.Entities;
using FinSightPro.Domain.Enums;

namespace FinSightPro.Application.Services;

public class RecurringTransactionService : IRecurringTransactionService
{
    private readonly IExpenseRepository _expenses;
    private readonly IIncomeRepository _incomes;
    private readonly IUnitOfWork _uow;

    public RecurringTransactionService(IExpenseRepository expenses, IIncomeRepository incomes, IUnitOfWork uow)
    {
        _expenses = expenses;
        _incomes = incomes;
        _uow = uow;
    }

    public async Task<int> GenerateDueAsync(string userId, DateTime asOf, CancellationToken ct = default)
    {
        var generated = 0;
        var asOfUtc = DateTime.SpecifyKind(asOf.Date, DateTimeKind.Utc);

        var expenseTemplates = await _expenses.ListRecurringTemplatesAsync(userId, ct);
        foreach (var tpl in expenseTemplates)
        {
            var next = NextOccurrence(tpl.LastGeneratedAt ?? tpl.Date, tpl.RecurrenceType);
            while (next <= asOfUtc && (tpl.RecurrenceEndDate is null || next <= tpl.RecurrenceEndDate))
            {
                await _expenses.AddAsync(new Expense
                {
                    UserId = userId,
                    CategoryId = tpl.CategoryId,
                    Description = tpl.Description,
                    Amount = tpl.Amount,
                    Date = next,
                    PaymentMethod = tpl.PaymentMethod,
                    Notes = tpl.Notes,
                    IsRecurring = false,
                    RecurrenceType = RecurrenceType.None,
                    ParentRecurringId = tpl.Id
                }, ct);
                tpl.LastGeneratedAt = next;
                generated++;
                next = NextOccurrence(next, tpl.RecurrenceType);
            }
        }

        var incomeTemplates = await _incomes.ListRecurringTemplatesAsync(userId, ct);
        foreach (var tpl in incomeTemplates)
        {
            var next = NextOccurrence(tpl.LastGeneratedAt ?? tpl.Date, tpl.RecurrenceType);
            while (next <= asOfUtc && (tpl.RecurrenceEndDate is null || next <= tpl.RecurrenceEndDate))
            {
                await _incomes.AddAsync(new Income
                {
                    UserId = userId,
                    CategoryId = tpl.CategoryId,
                    Description = tpl.Description,
                    Amount = tpl.Amount,
                    Date = next,
                    IsFixed = tpl.IsFixed,
                    IsRecurring = false,
                    RecurrenceType = RecurrenceType.None,
                    Notes = tpl.Notes,
                    ParentRecurringId = tpl.Id
                }, ct);
                tpl.LastGeneratedAt = next;
                generated++;
                next = NextOccurrence(next, tpl.RecurrenceType);
            }
        }

        if (generated > 0) await _uow.SaveChangesAsync(ct);
        return generated;
    }

    public static DateTime NextOccurrence(DateTime from, RecurrenceType type) => type switch
    {
        RecurrenceType.Daily => from.AddDays(1),
        RecurrenceType.Weekly => from.AddDays(7),
        RecurrenceType.Monthly => from.AddMonths(1),
        RecurrenceType.Yearly => from.AddYears(1),
        _ => DateTime.MaxValue
    };
}
