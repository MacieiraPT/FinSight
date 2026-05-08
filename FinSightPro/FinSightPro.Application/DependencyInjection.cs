using FinSightPro.Application.Interfaces;
using FinSightPro.Application.Services;
using FinSightPro.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinSightPro.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IIncomeService, IncomeService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();

        services.AddValidatorsFromAssemblyContaining<ExpenseCreateValidator>();

        return services;
    }
}
