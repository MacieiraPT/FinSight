using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinSightPro.Web.Controllers;

public class CategoriasController : BaseAuthController
{
    private readonly ICategoryService _service;

    public CategoriasController(ICategoryService service) => _service = service;

    public async Task<IActionResult> Index(CancellationToken ct) =>
        View(await _service.ListAsync(CurrentUserId, ct));

    public async Task<IActionResult> Create(CancellationToken ct) =>
        View(new CategoryFormViewModel { AvailableParents = await _service.ListAsync(CurrentUserId, ct) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.AvailableParents = await _service.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        var result = await _service.CreateAsync(CurrentUserId, new CategoryCreateDto
        {
            Name = vm.Name,
            Icon = vm.Icon,
            Color = vm.Color,
            ParentCategoryId = vm.ParentCategoryId
        }, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.AvailableParents = await _service.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        TempData["Success"] = "Categoria criada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var c = await _service.GetByIdAsync(CurrentUserId, id, ct);
        if (c == null) return NotFound();
        return View(new CategoryFormViewModel
        {
            Id = c.Id,
            Name = c.Name,
            Icon = c.Icon,
            Color = c.Color,
            ParentCategoryId = c.ParentCategoryId,
            IsSystem = c.IsSystem,
            AvailableParents = (await _service.ListAsync(CurrentUserId, ct)).Where(x => x.Id != id).ToList()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryFormViewModel vm, CancellationToken ct)
    {
        if (!vm.Id.HasValue) return BadRequest();
        if (!ModelState.IsValid)
        {
            vm.AvailableParents = await _service.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        var result = await _service.UpdateAsync(CurrentUserId, vm.Id.Value, new CategoryCreateDto
        {
            Name = vm.Name,
            Icon = vm.Icon,
            Color = vm.Color,
            ParentCategoryId = vm.ParentCategoryId
        }, ct);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.AvailableParents = await _service.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        TempData["Success"] = "Categoria atualizada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var c = await _service.GetByIdAsync(CurrentUserId, id, ct);
        if (c == null) return NotFound();
        return View(c);
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var c = await _service.GetByIdAsync(CurrentUserId, id, ct);
        if (c == null) return NotFound();
        return View(c);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await _service.DeleteAsync(CurrentUserId, id, ct);
        if (!result.Success) TempData["Error"] = result.Error;
        else TempData["Success"] = "Categoria eliminada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedDefault(CancellationToken ct)
    {
        await _service.SeedDefaultAsync(CurrentUserId, ct);
        TempData["Success"] = "Categorias por defeito criadas.";
        return RedirectToAction(nameof(Index));
    }
}
