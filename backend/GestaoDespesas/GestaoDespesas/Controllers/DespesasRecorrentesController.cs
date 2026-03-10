using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Data;
using GestaoDespesas.Models;
using GestaoDespesas.Services;

namespace GestaoDespesas.Controllers
{
    [Authorize]
    public class DespesasRecorrentesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditoriaService _auditoria;

        private static readonly string[] Frequencias = new[] { "Semanal", "Mensal", "Anual" };

        public DespesasRecorrentesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, AuditoriaService auditoria)
        {
            _context = context;
            _userManager = userManager;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var recorrentes = await _context.DespesasRecorrentes
                .Include(d => d.Categoria)
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.Ativa)
                .ThenByDescending(d => d.DataInicio)
                .ToListAsync();

            return View(recorrentes);
        }

        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            var categorias = await _context.Categorias
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            if (!categorias.Any())
            {
                TempData["ToastWarning"] = "Primeiro cria categorias para poderes registar despesas recorrentes.";
                return RedirectToAction("Index", "Categorias");
            }

            ViewData["CategoriaId"] = new SelectList(categorias, "CategoriaId", "Nome");
            ViewBag.Frequencias = Frequencias;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Descricao,Valor,CategoriaId,Observacoes,Frequencia,DataInicio,DataFim,Ativa")] DespesaRecorrente despesa)
        {
            var userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                despesa.UserId = userId;
                _context.Add(despesa);
                await _context.SaveChangesAsync();

                await _auditoria.RegistarAsync(userId!, "DespesaRecorrente", despesa.DespesaRecorrenteId, "Criar", $"Criada despesa recorrente: {despesa.Descricao}");

                TempData["ToastSuccess"] = "Despesa recorrente criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId", "Nome", despesa.CategoriaId);
            ViewBag.Frequencias = Frequencias;
            return View(despesa);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var despesa = await _context.DespesasRecorrentes
                .FirstOrDefaultAsync(d => d.DespesaRecorrenteId == id && d.UserId == userId);

            if (despesa == null) return NotFound();

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId", "Nome", despesa.CategoriaId);
            ViewBag.Frequencias = Frequencias;
            return View(despesa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DespesaRecorrenteId,Descricao,Valor,CategoriaId,Observacoes,Frequencia,DataInicio,DataFim,Ativa")] DespesaRecorrente despesa)
        {
            if (id != despesa.DespesaRecorrenteId) return NotFound();

            var userId = _userManager.GetUserId(User);
            var despesaDb = await _context.DespesasRecorrentes
                .FirstOrDefaultAsync(d => d.DespesaRecorrenteId == id && d.UserId == userId);

            if (despesaDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                despesaDb.Descricao = despesa.Descricao;
                despesaDb.Valor = despesa.Valor;
                despesaDb.CategoriaId = despesa.CategoriaId;
                despesaDb.Observacoes = despesa.Observacoes;
                despesaDb.Frequencia = despesa.Frequencia;
                despesaDb.DataInicio = despesa.DataInicio;
                despesaDb.DataFim = despesa.DataFim;
                despesaDb.Ativa = despesa.Ativa;

                await _context.SaveChangesAsync();
                await _auditoria.RegistarAsync(userId!, "DespesaRecorrente", id, "Editar", $"Editada: {despesa.Descricao}");

                TempData["ToastSuccess"] = "Despesa recorrente editada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId", "Nome", despesa.CategoriaId);
            ViewBag.Frequencias = Frequencias;
            return View(despesaDb);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var despesa = await _context.DespesasRecorrentes
                .Include(d => d.Categoria)
                .FirstOrDefaultAsync(d => d.DespesaRecorrenteId == id && d.UserId == userId);

            if (despesa == null) return NotFound();

            return View(despesa);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var despesa = await _context.DespesasRecorrentes
                .FirstOrDefaultAsync(d => d.DespesaRecorrenteId == id && d.UserId == userId);

            if (despesa == null) return NotFound();

            _context.DespesasRecorrentes.Remove(despesa);
            await _context.SaveChangesAsync();

            await _auditoria.RegistarAsync(userId!, "DespesaRecorrente", id, "Eliminar", $"Eliminada: {despesa.Descricao}");

            TempData["ToastSuccess"] = "Despesa recorrente eliminada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
