using FinSightPro.Application.DTOs;
using FluentValidation;

namespace FinSightPro.Application.Validators;

public class ExpenseCreateValidator : AbstractValidator<ExpenseCreateDto>
{
    public ExpenseCreateValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A descrição é obrigatória.")
            .MaximumLength(200);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("O valor tem de ser positivo.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Categoria inválida.");

        RuleFor(x => x.Date)
            .Must(d => d.Date <= DateTime.UtcNow.Date)
            .WithMessage("A data não pode ser futura.");
    }
}

public class IncomeCreateValidator : AbstractValidator<IncomeCreateDto>
{
    public IncomeCreateValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class CategoryCreateValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Color).MaximumLength(20);
        RuleFor(x => x.Icon).MaximumLength(40);
    }
}

public class BudgetCreateValidator : AbstractValidator<BudgetCreateDto>
{
    public BudgetCreateValidator()
    {
        RuleFor(x => x.MonthlyLimit).GreaterThan(0);
        RuleFor(x => x.Month).InclusiveBetween(1, 12);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}
