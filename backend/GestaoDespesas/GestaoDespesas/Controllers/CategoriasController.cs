using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // Conjunto de ícones Bootstrap Icons disponíveis na UI
        public static readonly string[] IconesDisponiveis = new[]
        {
            "bi-tag", "bi-cup-straw", "bi-house", "bi-car-front", "bi-heart-pulse",
            "bi-controller", "bi-cart3", "bi-bag", "bi-mortarboard", "bi-shield-check",
            "bi-piggy-bank", "bi-credit-card", "bi-receipt", "bi-tools", "bi-airplane",
            "bi-tv", "bi-phone", "bi-bicycle", "bi-stars", "bi-three-dots"
        };

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
                .Include(c => c.CategoriaPai)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.CategoriaPaiId == null ? 0 : 1)
                .ThenBy(c => c.Nome)
                .ToListAsync();

            return View(categorias);
        }

        // POST: Categorias/SeedDefault
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SeedDefault()
        {
            var userId = _userManager.GetUserId(User);

            var categoriasDefault = new (string Nome, string Icone, string Cor)[]
            {
                ("Alimentação", "bi-cup-straw", "#e74c3c"),
                ("Habitação", "bi-house", "#16a085"),
                ("Transportes", "bi-car-front", "#3498db"),
                ("Saúde", "bi-heart-pulse", "#e84393"),
                ("Educação", "bi-mortarboard", "#f39c12"),
                ("Lazer", "bi-controller", "#9b59b6"),
                ("Compras", "bi-bag", "#2ecc71"),
                ("Subscrições", "bi-tv", "#8e44ad"),
                ("Seguros", "bi-shield-check", "#34495e"),
                ("Outros", "bi-three-dots", "#7f8c8d")
            };

            foreach (var (nome, icone, cor) in categoriasDefault)
            {
                bool exists = await _context.Categorias
                    .AnyAsync(c => c.UserId == userId && c.Nome == nome);

                if (!exists)
                {
                    _context.Categorias.Add(new Categoria
                    {
                        Nome = nome,
                        Icone = icone,
                        Cor = cor,
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
                .Include(c => c.CategoriaPai)
                .FirstOrDefaultAsync(c => c.CategoriaId == id && c.UserId == userId);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        // GET: Categorias/Create
        public async Task<IActionResult> Create()
        {
            await PreencherViewBagsAsync(null);
            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Icone,Cor,CategoriaPaiId")] Categoria categoria)
        {
            var userId = _userManager.GetUserId(User);

            // Validar categoria-pai (se indicada, tem de pertencer ao utilizador)
            if (categoria.CategoriaPaiId.HasValue)
            {
                var paiOk = await _context.Categorias
                    .AnyAsync(c => c.CategoriaId == categoria.CategoriaPaiId.Value && c.UserId == userId);
                if (!paiOk) ModelState.AddModelError(nameof(categoria.CategoriaPaiId), "Categoria-pai inválida.");
            }

            if (ModelState.IsValid)
            {
                categoria.UserId = userId!;
                if (string.IsNullOrWhiteSpace(categoria.Icone)) categoria.Icone = "bi-tag";
                if (string.IsNullOrWhiteSpace(categoria.Cor)) categoria.Cor = "#6c757d";

                _context.Add(categoria);
                await _context.SaveChangesAsync();
                TempData["ToastSuccess"] = "Categoria criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            await PreencherViewBagsAsync(categoria.CategoriaPaiId);
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

            await PreencherViewBagsAsync(categoria.CategoriaPaiId, excluirId: id.Value);
            return View(categoria);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoriaId,Nome,Icone,Cor,CategoriaPaiId")] Categoria categoria)
        {
            if (id != categoria.CategoriaId) return NotFound();

            var userId = _userManager.GetUserId(User);

            var categoriaDb = await _context.Categorias
                .FirstOrDefaultAsync(c => c.CategoriaId == id && c.UserId == userId);

            if (categoriaDb == null) return NotFound();

            // Não pode ser pai dela própria
            if (categoria.CategoriaPaiId.HasValue && categoria.CategoriaPaiId.Value == id)
                ModelState.AddModelError(nameof(categoria.CategoriaPaiId), "Uma categoria não pode ser pai dela própria.");

            if (categoria.CategoriaPaiId.HasValue)
            {
                var paiOk = await _context.Categorias
                    .AnyAsync(c => c.CategoriaId == categoria.CategoriaPaiId.Value && c.UserId == userId);
                if (!paiOk) ModelState.AddModelError(nameof(categoria.CategoriaPaiId), "Categoria-pai inválida.");

                // Evitar ciclos: o pai não pode ser descendente desta categoria
                if (await EhDescendenteAsync(categoria.CategoriaPaiId.Value, id, userId!))
                    ModelState.AddModelError(nameof(categoria.CategoriaPaiId), "Não podes escolher uma sub-categoria como pai.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    categoriaDb.Nome = categoria.Nome;
                    categoriaDb.Icone = string.IsNullOrWhiteSpace(categoria.Icone) ? "bi-tag" : categoria.Icone;
                    categoriaDb.Cor = string.IsNullOrWhiteSpace(categoria.Cor) ? "#6c757d" : categoria.Cor;
                    categoriaDb.CategoriaPaiId = categoria.CategoriaPaiId;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(id, userId!)) return NotFound();
                    throw;
                }

                TempData["ToastSuccess"] = "Categoria editada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            await PreencherViewBagsAsync(categoria.CategoriaPaiId, excluirId: id);
            return View(categoriaDb);
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var categoria = await _context.Categorias
                .Include(c => c.CategoriaPai)
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

            if (categoria == null) return NotFound();

            var temDespesas = await _context.Despesas
                .AnyAsync(d => d.CategoriaId == id && d.UserId == userId);
            var temSubs = await _context.Categorias
                .AnyAsync(c => c.CategoriaPaiId == id && c.UserId == userId);

            if (temDespesas)
            {
                TempData["ToastDanger"] =
                    "Não podes eliminar esta categoria porque existem despesas associadas.";
                return RedirectToAction(nameof(Index));
            }

            if (temSubs)
            {
                TempData["ToastDanger"] =
                    "Não podes eliminar esta categoria porque tem sub-categorias associadas.";
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
                    "Não é possível eliminar esta categoria porque existem registos associados.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PreencherViewBagsAsync(int? selecionado, int? excluirId = null)
        {
            var userId = _userManager.GetUserId(User);

            var query = _context.Categorias.Where(c => c.UserId == userId);
            if (excluirId.HasValue)
                query = query.Where(c => c.CategoriaId != excluirId.Value);

            var pais = await query.OrderBy(c => c.Nome).ToListAsync();

            ViewBag.CategoriasPais = new SelectList(pais, "CategoriaId", "Nome", selecionado);
            ViewBag.IconesDisponiveis = IconesDisponiveis;
        }

        // Verifica se candidatoId é descendente de raizId (na sub-árvore de raizId)
        private async Task<bool> EhDescendenteAsync(int candidatoId, int raizId, string userId)
        {
            var todas = await _context.Categorias
                .Where(c => c.UserId == userId)
                .Select(c => new { c.CategoriaId, c.CategoriaPaiId })
                .ToListAsync();

            var visitado = new HashSet<int>();
            var fila = new Queue<int>();
            fila.Enqueue(raizId);

            while (fila.Count > 0)
            {
                var atual = fila.Dequeue();
                if (!visitado.Add(atual)) continue;

                var filhos = todas.Where(c => c.CategoriaPaiId == atual).Select(c => c.CategoriaId);
                foreach (var f in filhos)
                {
                    if (f == candidatoId) return true;
                    fila.Enqueue(f);
                }
            }

            return false;
        }

        private bool CategoriaExists(int id, string userId)
        {
            return _context.Categorias.Any(e => e.CategoriaId == id && e.UserId == userId);
        }
    }
}
