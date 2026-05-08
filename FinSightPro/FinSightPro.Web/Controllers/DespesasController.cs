using FinSightPro.Application.DTOs;
using FinSightPro.Application.Interfaces;
using FinSightPro.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinSightPro.Web.Controllers;

public class DespesasController : BaseAuthController
{
    private readonly IExpenseService _expenseService;
    private readonly ICategoryService _categoryService;
    private readonly IReportService _reportService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<DespesasController> _logger;

    private static readonly string[] AllowedAttachmentExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".webp" };
    private const long MaxAttachmentBytes = 5 * 1024 * 1024;

    public DespesasController(
        IExpenseService expenseService,
        ICategoryService categoryService,
        IReportService reportService,
        IWebHostEnvironment env,
        ILogger<DespesasController> logger)
    {
        _expenseService = expenseService;
        _categoryService = categoryService;
        _reportService = reportService;
        _env = env;
        _logger = logger;
    }

    public async Task<IActionResult> Index([FromQuery] ExpenseFilter filter, CancellationToken ct)
    {
        filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
        filter.Page = filter.Page <= 0 ? 1 : filter.Page;
        var page = await _expenseService.GetPagedAsync(CurrentUserId, filter, ct);
        var vm = new ExpenseListViewModel
        {
            Page = page,
            Filter = filter,
            Categories = await _categoryService.ListAsync(CurrentUserId, ct)
        };
        return View(vm);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        var vm = new ExpenseFormViewModel
        {
            Categories = await _categoryService.ListAsync(CurrentUserId, ct)
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExpenseFormViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }

        string? attachmentPath = null;
        if (vm.Attachment != null)
        {
            var saved = await SaveAttachmentAsync(vm.Attachment, ct);
            if (saved.Error != null)
            {
                ModelState.AddModelError(nameof(vm.Attachment), saved.Error);
                vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
                return View(vm);
            }
            attachmentPath = saved.Path;
        }

        var result = await _expenseService.CreateAsync(CurrentUserId, new ExpenseCreateDto
        {
            Description = vm.Description,
            Amount = vm.Amount,
            Date = vm.Date,
            CategoryId = vm.CategoryId,
            PaymentMethod = vm.PaymentMethod,
            Notes = vm.Notes,
            AttachmentPath = attachmentPath,
            IsRecurring = vm.IsRecurring,
            RecurrenceType = vm.RecurrenceType,
            RecurrenceEndDate = vm.RecurrenceEndDate
        }, ct);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        TempData["Success"] = "Despesa criada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var existing = await _expenseService.GetByIdAsync(CurrentUserId, id, ct);
        if (existing == null) return NotFound();

        return View(new ExpenseFormViewModel
        {
            Id = existing.Id,
            Description = existing.Description,
            Amount = existing.Amount,
            Date = existing.Date,
            CategoryId = existing.CategoryId,
            PaymentMethod = existing.PaymentMethod,
            Notes = existing.Notes,
            ExistingAttachmentPath = existing.AttachmentPath,
            IsRecurring = existing.IsRecurring,
            RecurrenceType = existing.RecurrenceType,
            Categories = await _categoryService.ListAsync(CurrentUserId, ct)
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ExpenseFormViewModel vm, CancellationToken ct)
    {
        if (!vm.Id.HasValue) return BadRequest();
        if (!ModelState.IsValid)
        {
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }

        string? attachmentPath = vm.ExistingAttachmentPath;
        if (vm.Attachment != null)
        {
            var saved = await SaveAttachmentAsync(vm.Attachment, ct);
            if (saved.Error != null)
            {
                ModelState.AddModelError(nameof(vm.Attachment), saved.Error);
                vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
                return View(vm);
            }
            attachmentPath = saved.Path;
        }

        var result = await _expenseService.UpdateAsync(CurrentUserId, vm.Id.Value, new ExpenseCreateDto
        {
            Description = vm.Description,
            Amount = vm.Amount,
            Date = vm.Date,
            CategoryId = vm.CategoryId,
            PaymentMethod = vm.PaymentMethod,
            Notes = vm.Notes,
            AttachmentPath = attachmentPath,
            IsRecurring = vm.IsRecurring,
            RecurrenceType = vm.RecurrenceType,
            RecurrenceEndDate = vm.RecurrenceEndDate
        }, ct);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            vm.Categories = await _categoryService.ListAsync(CurrentUserId, ct);
            return View(vm);
        }
        TempData["Success"] = "Despesa atualizada.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var item = await _expenseService.GetByIdAsync(CurrentUserId, id, ct);
        if (item == null) return NotFound();
        return View(item);
    }

    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await _expenseService.GetByIdAsync(CurrentUserId, id, ct);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken ct)
    {
        var result = await _expenseService.DeleteAsync(CurrentUserId, id, ct);
        if (!result.Success) TempData["Error"] = result.Error;
        else TempData["Success"] = "Despesa eliminada.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportarCsv([FromForm] ExpenseFilter filter, CancellationToken ct)
    {
        var bytes = await _reportService.ExportExpensesCsvAsync(CurrentUserId, filter, ct);
        return File(bytes, "text/csv", $"despesas-{DateTime.UtcNow:yyyyMMddHHmm}.csv");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ExportarExcel([FromForm] ExpenseFilter filter, CancellationToken ct)
    {
        var bytes = await _reportService.ExportExpensesExcelAsync(CurrentUserId, filter, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"despesas-{DateTime.UtcNow:yyyyMMddHHmm}.xlsx");
    }

    private async Task<(string? Path, string? Error)> SaveAttachmentAsync(IFormFile file, CancellationToken ct)
    {
        if (file.Length > MaxAttachmentBytes) return (null, "O ficheiro não pode exceder 5 MB.");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedAttachmentExtensions.Contains(ext)) return (null, "Formato não suportado. Permitidos: PDF, PNG, JPG, WEBP.");

        var folder = Path.Combine(_env.WebRootPath, "uploads", CurrentUserId);
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, ct);
        }
        var relative = $"/uploads/{CurrentUserId}/{fileName}";
        _logger.LogInformation("Attachment saved at {Path} for user {User}", relative, CurrentUserId);
        return (relative, null);
    }
}
