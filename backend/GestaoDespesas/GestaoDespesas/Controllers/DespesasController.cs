using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using ClosedXML.Excel;
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
    public class DespesasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DespesasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Despesas
        // Filtros: categoriaId, ano, mes, q (pesquisa)
        public async Task<IActionResult> Index(int? categoriaId, int? ano, int? mes, string? q, string sortOrder, int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            const int pageSize = 10;

            // Ordenação
            ViewBag.DataSort = sortOrder == "data_asc" ? "data_desc" : "data_asc";
            ViewBag.ValorSort = sortOrder == "valor_asc" ? "valor_desc" : "valor_asc";
            ViewBag.CategoriaSort = sortOrder == "cat_asc" ? "cat_desc" : "cat_asc";

            ViewBag.CurrentSort = sortOrder;

            var categorias = await _context.Categorias
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.Categorias = new SelectList(categorias, "CategoriaId", "Nome", categoriaId);
            ViewBag.Ano = ano;
            ViewBag.Mes = mes;
            ViewBag.Q = q;

            var query = _context.Despesas
                .Include(d => d.Categoria)
                .Where(d => d.UserId == userId)
                .AsQueryable();

            if (categoriaId.HasValue)
                query = query.Where(d => d.CategoriaId == categoriaId.Value);

            if (ano.HasValue)
                query = query.Where(d => d.Data.Year == ano.Value);

            if (mes.HasValue)
                query = query.Where(d => d.Data.Month == mes.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(d => d.Descricao.Contains(q));

            // ORDER BY DINÂMICO
            query = sortOrder switch
            {
                "data_asc" => query.OrderBy(d => d.Data),
                "data_desc" => query.OrderByDescending(d => d.Data),

                "valor_asc" => query.OrderBy(d => d.Valor),
                "valor_desc" => query.OrderByDescending(d => d.Valor),

                "cat_asc" => query.OrderBy(d => d.Categoria!.Nome),
                "cat_desc" => query.OrderByDescending(d => d.Categoria!.Nome),

                _ => query.OrderByDescending(d => d.Data)
            };

            var totalItems = await query.CountAsync();

            var despesas = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(despesas);
        }

        // GET: Despesas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var despesa = await _context.Despesas
                .Include(d => d.Categoria)
                .FirstOrDefaultAsync(d => d.DespesaId == id && d.UserId == userId);

            if (despesa == null) return NotFound();

            return View(despesa);
        }

        // GET: Despesas/Create
        public async Task<IActionResult> Create()
        {
            var userId = _userManager.GetUserId(User);

            var categorias = await _context.Categorias
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.Nome)
                .ToListAsync();

            if (!categorias.Any())
            {
                TempData["ToastWarning"] = "Primeiro cria (ou gera) categorias para poderes registar despesas.";
                return RedirectToAction("Index", "Categorias");
            }

            ViewData["CategoriaId"] = new SelectList(categorias, "CategoriaId", "Nome");
            return View();
        }

        // POST: Despesas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Descricao,Valor,Data,CategoriaId,Observacoes")] Despesa despesa)
        {
            var userId = _userManager.GetUserId(User);

            var categoriaOk = await _context.Categorias.AnyAsync(c => c.CategoriaId == despesa.CategoriaId && c.UserId == userId);
            if (!categoriaOk)
            {
                ModelState.AddModelError("CategoriaId", "Categoria inválida.");
            }

            if (despesa.Data > DateTime.Today)
            {
                ModelState.AddModelError("Data", "A data não pode ser futura.");
            }

            if (ModelState.IsValid)
            {
                despesa.UserId = userId;

                _context.Add(despesa);
                await _context.SaveChangesAsync();

                TempData["ToastSuccess"] = "Despesa criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ToastDanger"] = "Verifica os campos do formulário.";

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId",
                "Nome",
                despesa.CategoriaId
            );

            return View(despesa);
        }

        // GET: Despesas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var despesa = await _context.Despesas
                .FirstOrDefaultAsync(d => d.DespesaId == id && d.UserId == userId);

            if (despesa == null) return NotFound();

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId",
                "Nome",
                despesa.CategoriaId
            );

            return View(despesa);
        }

        // POST: Despesas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DespesaId,Descricao,Valor,Data,CategoriaId,Observacoes")] Despesa despesa)
        {
            if (id != despesa.DespesaId) return NotFound();

            var userId = _userManager.GetUserId(User);

            // Buscar a despesa real do utilizador
            var despesaDb = await _context.Despesas
                .FirstOrDefaultAsync(d => d.DespesaId == id && d.UserId == userId);

            if (despesaDb == null) return NotFound();

            if (despesa.Data > DateTime.Today)
            {
                ModelState.AddModelError("Data", "A data não pode ser futura.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Atualizar apenas campos editáveis
                    despesaDb.Descricao = despesa.Descricao;
                    despesaDb.Valor = despesa.Valor;
                    despesaDb.Data = despesa.Data;
                    despesaDb.CategoriaId = despesa.CategoriaId;
                    despesaDb.Observacoes = despesa.Observacoes;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DespesaExists(id, userId)) return NotFound();
                    throw;
                }

                TempData["ToastSuccess"] = "Despesa editada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoriaId"] = new SelectList(
                await _context.Categorias.Where(c => c.UserId == userId).OrderBy(c => c.Nome).ToListAsync(),
                "CategoriaId",
                "Nome",
                despesa.CategoriaId
            );

            return View(despesaDb);
        }

        // GET: Despesas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var despesa = await _context.Despesas
                .Include(d => d.Categoria)
                .FirstOrDefaultAsync(d => d.DespesaId == id && d.UserId == userId);

            if (despesa == null) return NotFound();

            return View(despesa);
        }

        // POST: Despesas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var despesa = await _context.Despesas
                .FirstOrDefaultAsync(d => d.DespesaId == id && d.UserId == userId);

            if (despesa == null) return NotFound();

            _context.Despesas.Remove(despesa);
            await _context.SaveChangesAsync();

            TempData["ToastSuccess"] = "Despesa eliminada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarCsv(
            int? categoriaId,
            int? ano,
            int? mes,
            string? q)
        {
            var userId = _userManager.GetUserId(User);

            var despesas = await GetDespesasFiltradas(userId, categoriaId, ano, mes, q)
                .ToListAsync();

            var builder = new StringBuilder();

            builder.AppendLine("Descrição;Categoria;Data;Valor;Observações");

            foreach (var d in despesas)
            {
                builder.AppendLine(
                    $"{d.Descricao};" +
                    $"{d.Categoria?.Nome};" +
                    $"{d.Data:dd/MM/yyyy};" +
                    $"{d.Valor.ToString(System.Globalization.CultureInfo.InvariantCulture)};" +
                    $"{d.Observacoes}"
                );
            }

            return File(
                Encoding.UTF8.GetBytes(builder.ToString()),
                "text/csv",
                $"despesas_filtradas_{DateTime.Now:yyyyMMdd}.csv"
            );
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarExcel(
    int? categoriaId,
    int? ano,
    int? mes,
    string? q)
        {
            var userId = _userManager.GetUserId(User);

            var despesas = await GetDespesasFiltradas(userId, categoriaId, ano, mes, q)
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Despesas");

            worksheet.Cell(1, 1).Value = "Descrição";
            worksheet.Cell(1, 2).Value = "Categoria";
            worksheet.Cell(1, 3).Value = "Data";
            worksheet.Cell(1, 4).Value = "Valor (€)";

            int row = 2;

            foreach (var d in despesas)
            {
                worksheet.Cell(row, 1).Value = d.Descricao;
                worksheet.Cell(row, 2).Value = d.Categoria?.Nome;
                worksheet.Cell(row, 3).Value = d.Data.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 4).Value = d.Valor;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"despesas_filtradas_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        private IQueryable<Despesa> GetDespesasFiltradas(
            string userId,
            int? categoriaId,
            int? ano,
            int? mes,
            string? q)
        {
            var query = _context.Despesas
                .Include(d => d.Categoria)
                .Where(d => d.UserId == userId)
                .AsQueryable();

            if (categoriaId.HasValue)
                query = query.Where(d => d.CategoriaId == categoriaId.Value);

            if (ano.HasValue)
                query = query.Where(d => d.Data.Year == ano.Value);

            if (mes.HasValue)
                query = query.Where(d => d.Data.Month == mes.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(d => d.Descricao.Contains(q));

            return query.OrderByDescending(d => d.Data);
        }

        private bool DespesaExists(int id, string userId)
        {
            return _context.Despesas.Any(e => e.DespesaId == id && e.UserId == userId);
        }
    }
}
