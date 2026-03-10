using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Data;
using GestaoDespesas.Models;

namespace GestaoDespesas.Controllers
{
    [Authorize]
    public class ReceitasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private static readonly string[] TiposReceita = new[]
        {
            "Salário",
            "Freelance",
            "Bónus",
            "Vendas",
            "Investimentos",
            "Outros"
        };

        public ReceitasController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Receitas
        public async Task<IActionResult> Index(int? ano, int? mes, string? q, string sortOrder, int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            const int pageSize = 10;

            ViewBag.DataSort = sortOrder == "data_asc" ? "data_desc" : "data_asc";
            ViewBag.ValorSort = sortOrder == "valor_asc" ? "valor_desc" : "valor_asc";
            ViewBag.TipoSort = sortOrder == "tipo_asc" ? "tipo_desc" : "tipo_asc";
            ViewBag.CurrentSort = sortOrder;

            ViewBag.Ano = ano;
            ViewBag.Mes = mes;
            ViewBag.Q = q;

            var query = _context.Receitas
                .Where(r => r.UserId == userId)
                .AsQueryable();

            if (ano.HasValue)
                query = query.Where(r => r.Data.Year == ano.Value);

            if (mes.HasValue)
                query = query.Where(r => r.Data.Month == mes.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(r => r.Descricao.Contains(q));

            query = sortOrder switch
            {
                "data_asc" => query.OrderBy(r => r.Data),
                "data_desc" => query.OrderByDescending(r => r.Data),
                "valor_asc" => query.OrderBy(r => r.Valor),
                "valor_desc" => query.OrderByDescending(r => r.Valor),
                "tipo_asc" => query.OrderBy(r => r.Tipo),
                "tipo_desc" => query.OrderByDescending(r => r.Tipo),
                _ => query.OrderByDescending(r => r.Data)
            };

            var totalItems = await query.CountAsync();

            var receitas = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(receitas);
        }

        // GET: Receitas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var receita = await _context.Receitas
                .FirstOrDefaultAsync(r => r.ReceitaId == id && r.UserId == userId);

            if (receita == null) return NotFound();

            return View(receita);
        }

        // GET: Receitas/Create
        public IActionResult Create()
        {
            ViewBag.Tipos = TiposReceita;
            return View();
        }

        // POST: Receitas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Descricao,Valor,Data,Tipo")] Receita receita)
        {
            var userId = _userManager.GetUserId(User);

            if (receita.Data > DateTime.Today)
            {
                ModelState.AddModelError("Data", "A data não pode ser futura.");
            }

            if (ModelState.IsValid)
            {
                receita.UserId = userId;

                _context.Add(receita);
                await _context.SaveChangesAsync();

                TempData["ToastSuccess"] = "Receita criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["ToastDanger"] = "Verifica os campos do formulário.";
            ViewBag.Tipos = TiposReceita;
            return View(receita);
        }

        // GET: Receitas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var receita = await _context.Receitas
                .FirstOrDefaultAsync(r => r.ReceitaId == id && r.UserId == userId);

            if (receita == null) return NotFound();

            ViewBag.Tipos = TiposReceita;
            return View(receita);
        }

        // POST: Receitas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReceitaId,Descricao,Valor,Data,Tipo")] Receita receita)
        {
            if (id != receita.ReceitaId) return NotFound();

            var userId = _userManager.GetUserId(User);

            var receitaDb = await _context.Receitas
                .FirstOrDefaultAsync(r => r.ReceitaId == id && r.UserId == userId);

            if (receitaDb == null) return NotFound();

            if (receita.Data > DateTime.Today)
            {
                ModelState.AddModelError("Data", "A data não pode ser futura.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    receitaDb.Descricao = receita.Descricao;
                    receitaDb.Valor = receita.Valor;
                    receitaDb.Data = receita.Data;
                    receitaDb.Tipo = receita.Tipo;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Receitas.Any(r => r.ReceitaId == id && r.UserId == userId))
                        return NotFound();
                    throw;
                }

                TempData["ToastSuccess"] = "Receita editada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Tipos = TiposReceita;
            return View(receitaDb);
        }

        // GET: Receitas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var receita = await _context.Receitas
                .FirstOrDefaultAsync(r => r.ReceitaId == id && r.UserId == userId);

            if (receita == null) return NotFound();

            return View(receita);
        }

        // POST: Receitas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var receita = await _context.Receitas
                .FirstOrDefaultAsync(r => r.ReceitaId == id && r.UserId == userId);

            if (receita == null) return NotFound();

            _context.Receitas.Remove(receita);
            await _context.SaveChangesAsync();

            TempData["ToastSuccess"] = "Receita eliminada com sucesso!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarCsv(int? ano, int? mes, string? q)
        {
            var userId = _userManager.GetUserId(User);
            var receitas = await GetReceitasFiltradas(userId!, ano, mes, q).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Descrição;Tipo;Data;Valor");

            foreach (var r in receitas)
            {
                sb.AppendLine(
                    $"{r.Descricao};" +
                    $"{r.Tipo};" +
                    $"{r.Data:dd/MM/yyyy};" +
                    $"{r.Valor.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                );
            }

            return File(
                Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                $"receitas_filtradas_{DateTime.Now:yyyyMMdd}.csv"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportarExcel(int? ano, int? mes, string? q)
        {
            var userId = _userManager.GetUserId(User);
            var receitas = await GetReceitasFiltradas(userId!, ano, mes, q).ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Receitas");

            worksheet.Cell(1, 1).Value = "Descrição";
            worksheet.Cell(1, 2).Value = "Tipo";
            worksheet.Cell(1, 3).Value = "Data";
            worksheet.Cell(1, 4).Value = "Valor (€)";

            int row = 2;
            foreach (var r in receitas)
            {
                worksheet.Cell(row, 1).Value = r.Descricao;
                worksheet.Cell(row, 2).Value = r.Tipo;
                worksheet.Cell(row, 3).Value = r.Data.ToString("dd/MM/yyyy");
                worksheet.Cell(row, 4).Value = r.Valor;
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"receitas_filtradas_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        private IQueryable<Receita> GetReceitasFiltradas(string userId, int? ano, int? mes, string? q)
        {
            var query = _context.Receitas
                .Where(r => r.UserId == userId)
                .AsQueryable();

            if (ano.HasValue)
                query = query.Where(r => r.Data.Year == ano.Value);

            if (mes.HasValue)
                query = query.Where(r => r.Data.Month == mes.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(r => r.Descricao.Contains(q));

            return query.OrderByDescending(r => r.Data);
        }
    }
}
