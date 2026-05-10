using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Data;
using GestaoDespesas.Models;
using GestaoDespesas.Services;

namespace GestaoDespesas.Controllers
{
    [Authorize]
    public class ReceitasRecorrentesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuditoriaService _auditoria;

        private static readonly string[] Frequencias = new[] { "Semanal", "Mensal", "Anual" };

        private static readonly string[] TiposReceita = new[]
        {
            "Salário", "Freelance", "Bónus", "Vendas", "Investimentos", "Outros"
        };

        public ReceitasRecorrentesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, AuditoriaService auditoria)
        {
            _context = context;
            _userManager = userManager;
            _auditoria = auditoria;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var recorrentes = await _context.ReceitasRecorrentes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.Ativa)
                .ThenByDescending(r => r.DataInicio)
                .ToListAsync();

            return View(recorrentes);
        }

        public IActionResult Create()
        {
            ViewBag.Frequencias = Frequencias;
            ViewBag.Tipos = TiposReceita;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Descricao,Valor,Tipo,Observacoes,Frequencia,DataInicio,DataFim,Ativa")] ReceitaRecorrente receita)
        {
            var userId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                receita.UserId = userId!;
                _context.Add(receita);
                await _context.SaveChangesAsync();

                await _auditoria.RegistarAsync(userId!, "ReceitaRecorrente", receita.ReceitaRecorrenteId, "Criar", $"Criada receita recorrente: {receita.Descricao}");

                TempData["ToastSuccess"] = "Receita recorrente criada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Frequencias = Frequencias;
            ViewBag.Tipos = TiposReceita;
            return View(receita);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var receita = await _context.ReceitasRecorrentes
                .FirstOrDefaultAsync(r => r.ReceitaRecorrenteId == id && r.UserId == userId);

            if (receita == null) return NotFound();

            ViewBag.Frequencias = Frequencias;
            ViewBag.Tipos = TiposReceita;
            return View(receita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReceitaRecorrenteId,Descricao,Valor,Tipo,Observacoes,Frequencia,DataInicio,DataFim,Ativa")] ReceitaRecorrente receita)
        {
            if (id != receita.ReceitaRecorrenteId) return NotFound();

            var userId = _userManager.GetUserId(User);
            var receitaDb = await _context.ReceitasRecorrentes
                .FirstOrDefaultAsync(r => r.ReceitaRecorrenteId == id && r.UserId == userId);

            if (receitaDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                receitaDb.Descricao = receita.Descricao;
                receitaDb.Valor = receita.Valor;
                receitaDb.Tipo = receita.Tipo;
                receitaDb.Observacoes = receita.Observacoes;
                receitaDb.Frequencia = receita.Frequencia;
                receitaDb.DataInicio = receita.DataInicio;
                receitaDb.DataFim = receita.DataFim;
                receitaDb.Ativa = receita.Ativa;

                await _context.SaveChangesAsync();
                await _auditoria.RegistarAsync(userId!, "ReceitaRecorrente", id, "Editar", $"Editada: {receita.Descricao}");

                TempData["ToastSuccess"] = "Receita recorrente editada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Frequencias = Frequencias;
            ViewBag.Tipos = TiposReceita;
            return View(receitaDb);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var receita = await _context.ReceitasRecorrentes
                .FirstOrDefaultAsync(r => r.ReceitaRecorrenteId == id && r.UserId == userId);

            if (receita == null) return NotFound();

            return View(receita);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var receita = await _context.ReceitasRecorrentes
                .FirstOrDefaultAsync(r => r.ReceitaRecorrenteId == id && r.UserId == userId);

            if (receita == null) return NotFound();

            _context.ReceitasRecorrentes.Remove(receita);
            await _context.SaveChangesAsync();

            await _auditoria.RegistarAsync(userId!, "ReceitaRecorrente", id, "Eliminar", $"Eliminada: {receita.Descricao}");

            TempData["ToastSuccess"] = "Receita recorrente eliminada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}
