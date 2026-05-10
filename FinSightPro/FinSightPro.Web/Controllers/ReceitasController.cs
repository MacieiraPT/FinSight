using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinSightPro.Web.Controllers;

public class ReceitasController : BaseAuthController
{
    private readonly IIncomeService _incomeService;
    private readonly ICategoryService _categoryService;

    public ReceitasController(IIncomeService incomeService, ICategoryService categoryService)
    {
        _incomeService = incomeService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(int? year, int? month, CancellationToken ct)
    {
        var items = await _incomeService.ListAsync(CurrentUserId, year, month, ct);
        return View(new IncomeListViewModel { Items = items, Year = year, Month = month });
    }

    public async Task<IActionResult> Create(CancellationToken ct) =>
        View(new IncomeFormViewModel { Categories = await _categoryService.ListAsync(CurrentUserId, ct) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IncomeFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        var result = await _incomeService.CreateAsync(CurrentUserId, new IncomeCreateDto
        {
            Description = vm.Description,
            Amount = vm.Amount,
            Date = vm.Date,
            CategoryId = vm.CategoryId,
            IsFixed = vm.IsFixed,
            IsRecurring = vm.IsRecurring,
            RecurrenceType = vm.RecurrenceType,
            RecurrenceEndDate = vm.RecurrenceEndDate,
            Notes = vm.Notes
        }, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        TempData["Success"] = "Receita registada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var existing = await _incomeService.GetByIdAsync(CurrentUserId, id, ct);
        if (existing == null) return NotFound();
        return View(new IncomeFormViewModel
        {
            Id = existing.Id,
            Description = existing.Description,
            Amount = existing.Amount,
            Date = existing.Date,
            CategoryId = existing.CategoryId,
            IsFixed = existing.IsFixed,
            IsRecurring = existing.IsRecurring,
            RecurrenceType = existing.RecurrenceType,
            Notes = existing.Notes,
            Categories = await _categoryService.ListAsync(CurrentUserId, ct)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(IncomeFormViewModel vm, CancellationToken ct)
    {
        if (!vm.Id.HasValue) return BadRequest();
        if (!ModelState.IsValid)
        {
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        var result = await _incomeService.UpdateAsync(CurrentUserId, vm.Id.Value, new IncomeCreateDto
        {
            Description = vm.Description,
            Amount = vm.Amount,
            Date = vm.Date,
            CategoryId = vm.CategoryId,
            IsFixed = vm.IsFixed,
            IsRecurring = vm.IsRecurring,
            RecurrenceType = vm.RecurrenceType,
            RecurrenceEndDate = vm.RecurrenceEndDate,
            Notes = vm.Notes
        }, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        TempData["Success"] = "Receita atualizada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _incomeService.GetByIdAsync(CurrentUserId, id, ct);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await _incomeService.DeleteAsync(CurrentUserId, id, ct);
        if (!result.Success) TempData["Error"] = result.Error;
        else TempData["Success"] = "Receita eliminada.";
        return RedirectToAction(nameof(Index));
    }
}
