using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinSightPro.Web.Controllers;

public class OrcamentosController : BaseAuthController
{
    private readonly IBudgetService _budgetService;
    private readonly ICategoryService _categoryService;

    public OrcamentosController(IBudgetService budgetService, ICategoryService categoryService)
    {
        _budgetService = budgetService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(int? year, int? month, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        var items = await _budgetService.ListByMonthAsync(CurrentUserId, y, m, ct);
        return View(new BudgetListViewModel { Year = y, Month = m, Items = items });
    }

    public async Task<IActionResult> Create(CancellationToken ct) =>
        View(new BudgetFormViewModel { Categories = await _categoryService.ListAsync(CurrentUserId, ct) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BudgetFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        var result = await _budgetService.CreateAsync(CurrentUserId, new BudgetCreateDto
        {
            CategoryId = vm.CategoryId,
            MonthlyLimit = vm.MonthlyLimit,
            Month = vm.Month,
            Year = vm.Year
        }, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        TempData["Success"] = "Orçamento criado.";
        return RedirectToAction(nameof(Index), new { year = vm.Year, month = vm.Month });
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var existing = await _budgetService.GetByIdAsync(CurrentUserId, id, ct);
        if (existing == null) return NotFound();
        return View(new BudgetFormViewModel
        {
            Id = existing.Id,
            CategoryId = existing.CategoryId,
            MonthlyLimit = existing.MonthlyLimit,
            Month = existing.Month,
            Year = existing.Year,
            Categories = await _categoryService.ListAsync(CurrentUserId, ct)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BudgetFormViewModel vm, CancellationToken ct)
    {
        if (!vm.Id.HasValue) return BadRequest();
        if (!ModelState.IsValid)
        {
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        var result = await _budgetService.UpdateAsync(CurrentUserId, vm.Id.Value, new BudgetCreateDto
        {
            CategoryId = vm.CategoryId,
            MonthlyLimit = vm.MonthlyLimit,
            Month = vm.Month,
            Year = vm.Year
        }, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        TempData["Success"] = "Orçamento atualizado.";
        return RedirectToAction(nameof(Index), new { year = vm.Year, month = vm.Month });
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var existing = await _budgetService.GetByIdAsync(CurrentUserId, id, ct);
        if (existing == null) return NotFound();
        return View(existing);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await _budgetService.DeleteAsync(CurrentUserId, id, ct);
        if (!result.Success) TempData["Error"] = result.Error;
        else TempData["Success"] = "Orçamento eliminado.";
        return RedirectToAction(nameof(Index));
    }
}
