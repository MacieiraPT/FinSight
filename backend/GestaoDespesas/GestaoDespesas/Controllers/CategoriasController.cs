using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Data;
using GestaoDespesas.Models;

namespace GestaoDespesas.Controllers
{
    [Authorize]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CategoriasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var categorias = await _context.Categorias
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            return View(categorias);
        }

        // POST: Categorias/SeedDefault
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedDefault()
        {
            var userId = _userManager.GetUserId(User);

            var categoriasDefault = new[]
            {
                "Alimentação",
                "Habitação",
                "Transportes",
                "Saúde",
                "Educação",
                "Lazer",
                "Compras",
                "Subscrições",
                "Seguros",
                "Outros"
            };

            foreach (var nome in categoriasDefault)
            {
                bool exists = await _context.Categorias
                    .AnyAsync(c => c.UserId == userId && c.Nome == nome);

                if (!exists)
                {
                    _context.Categorias.Add(new Categoria
                    {
                        Nome = nome,
                        UserId = userId
                    });
                }
            }

            await _context.SaveChangesAsync();

            TempData["ToastSuccess"] = "Categorias padrão adicionadas!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Categorias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CategoriaId == id && c.UserId == userId);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                categoria.UserId = _userManager.GetUserId(User);

                _context.Add(categoria);
                await _context.SaveChangesAsync();
                TempData["ToastSuccess"] = "Categoria criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(categoria);
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CategoriaId == id && c.UserId == userId);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,Nome")] Categoria categoria)
        {
            if (id != categoria.CategoriaId) return NotFound();

            var userId = _userManager.GetUserId(User);

            // Garante que a categoria pertence ao utilizador logado
            var categoriaDb = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CategoriaId == id && c.UserId == userId);

            if (categoriaDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Atualizar apenas o que é editável
                    categoriaDb.Nome = categoria.Nome;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(id, userId)) return NotFound();
                    throw;
                }

                TempData["ToastSuccess"] = "Categoria editada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(categoriaDb);
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CategoriaId == id && c.UserId == userId);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var categoria = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CategoriaId == id && c.UserId == userId);

            var temDespesas = await _context.Despesas
                .AnyAsync(d => d.CategoriaId == id);

            if (categoria == null)
                return NotFound();

            if (temDespesas)
            {
                TempData["ToastDanger"] =
                    "Não podes eliminar esta categoria porque existem despesas associadas.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();

                TempData["ToastSuccess"] = "Categoria eliminada com sucesso!";
            }
            catch (DbUpdateException)
            {
                TempData["ToastDanger"] =
                    "Não é possível eliminar esta categoria porque existem despesas associadas.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id, string userId)
        {
            return _context.Categorias.Any(e => e.CategoriaId == id && e.UserId == userId);
        }
    }
}
