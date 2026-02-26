using GestaoDespesas.Data;
using GestaoDespesas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GestaoDespesas.Controllers
{
    [Authorize]
    public class OrcamentosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public OrcamentosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orcamentos
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var orcamentos = await _context.Orcamentos
                .Include(o => o.Categoria)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.Ano)
                .ThenByDescending(o => o.Mes)
                .ThenBy(o => o.Categoria!.Nome)
                .ToListAsync();

            return View(orcamentos);
        }

        // GET: Orcamentos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var orcamento = await _context.Orcamentos
                .Include(o => o.Categoria)
                .FirstOrDefaultAsync(o => o.OrcamentoId == id && o.UserId == userId);

            if (orcamento == null) return NotFound();

            return View(orcamento);
        }

        // GET: Orcamentos/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId",
                "Nome"
            );

            return View();
        }

        // POST: Orcamentos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrcamentoId,Ano,Mes,CategoriaId,Limite")] Orcamento orcamento)
        {
            var userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                orcamento.UserId = userId;

                _context.Add(orcamento);
                await _context.SaveChangesAsync();
                TempData["ToastSuccess"] = "Orçamento criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId",
                "Nome",
                orcamento.CategoriaId
            );

            return View(orcamento);
        }

        // GET: Orcamentos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var orcamento = await _context.Orcamentos
                .FirstOrDefaultAsync(o => o.OrcamentoId == id && o.UserId == userId);

            if (orcamento == null) return NotFound();

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId",
                "Nome",
                orcamento.CategoriaId
            );

            return View(orcamento);
        }

        // POST: Orcamentos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrcamentoId,Ano,Mes,CategoriaId,Limite")] Orcamento orcamento)
        {
            if (id != orcamento.OrcamentoId) return NotFound();

            var userId = _userManager.GetUserId(User);

            var orcamentoDb = await _context.Orcamentos
                .FirstOrDefaultAsync(o => o.OrcamentoId == id && o.UserId == userId);

            if (orcamentoDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    orcamentoDb.Ano = orcamento.Ano;
                    orcamentoDb.Mes = orcamento.Mes;
                    orcamentoDb.CategoriaId = orcamento.CategoriaId;
                    orcamentoDb.Limite = orcamento.Limite;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrcamentoExists(id, userId)) return NotFound();
                    throw;
                }

                TempData["ToastSuccess"] = "Orçamento editado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId",
                "Nome",
                orcamento.CategoriaId
            );

            return View(orcamentoDb);
        }

        // GET: Orcamentos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var orcamento = await _context.Orcamentos
                .Include(o => o.Categoria)
                .FirstOrDefaultAsync(o => o.OrcamentoId == id && o.UserId == userId);

            if (orcamento == null) return NotFound();

            return View(orcamento);
        }

        // POST: Orcamentos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var orcamento = await _context.Orcamentos
                .FirstOrDefaultAsync(o => o.OrcamentoId == id && o.UserId == userId);

            if (orcamento == null) return NotFound();

            _context.Orcamentos.Remove(orcamento);
            await _context.SaveChangesAsync();

            TempData["ToastSuccess"] = "Orçamento eliminado com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        private bool OrcamentoExists(int id, string userId)
        {
            return _context.Orcamentos.Any(e => e.OrcamentoId == id && e.UserId == userId);
        }
    }
}
