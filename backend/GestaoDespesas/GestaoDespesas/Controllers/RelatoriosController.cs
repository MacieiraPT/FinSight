using System;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Data;

namespace GestaoDespesas.Controllers
{
    [Authorize]
    public class RelatoriosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RelatoriosController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? dataInicio, string? dataFim)
        {
            var userId = _userManager.GetUserId(User);
            var now = DateTime.UtcNow;

            DateTime inicio;
            DateTime fim;

            if (!string.IsNullOrWhiteSpace(dataInicio) && DateTime.TryParse(dataInicio, out var di))
                inicio = DateTime.SpecifyKind(di, DateTimeKind.Utc);
            else
                inicio = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            if (!string.IsNullOrWhiteSpace(dataFim) && DateTime.TryParse(dataFim, out var df))
                fim = DateTime.SpecifyKind(df.AddDays(1), DateTimeKind.Utc);
            else
                fim = now;

            ViewBag.DataInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.DataFim = (fim.AddDays(-1)).ToString("yyyy-MM-dd");

            var despesas = await _context.Despesas
                .Include(d => d.Categoria)
                .Where(d => d.UserId == userId && d.Data >= inicio && d.Data < fim)
                .ToListAsync();

            var receitas = await _context.Receitas
                .Where(r => r.UserId == userId && r.Data >= inicio && r.Data < fim)
                .ToListAsync();

            var totalDespesas = despesas.Sum(d => d.Valor);
            var totalReceitas = receitas.Sum(r => r.Valor);
            var saldo = totalReceitas - totalDespesas;

            // Por categoria
            var porCategoria = despesas
                .Where(d => d.Categoria != null)
                .GroupBy(d => d.Categoria!.Nome)
                .Select(g => new { Categoria = g.Key, Total = g.Sum(x => x.Valor) })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Tendência mensal
            var meses = despesas
                .GroupBy(d => new { d.Data.Year, d.Data.Month })
                .Select(g => new
                {
                    Mes = $"{CultureInfo.GetCultureInfo("pt-PT").DateTimeFormat.GetAbbreviatedMonthName(g.Key.Month)} {g.Key.Year}",
                    Ano = g.Key.Year,
                    MesNum = g.Key.Month,
                    TotalDespesas = g.Sum(x => x.Valor)
                })
                .OrderBy(x => x.Ano).ThenBy(x => x.MesNum)
                .ToList();

            var mesesReceitas = receitas
                .GroupBy(r => new { r.Data.Year, r.Data.Month })
                .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month}", g => g.Sum(x => x.Valor));

            var tendencia = meses.Select(m => new
            {
                m.Mes,
                m.TotalDespesas,
                TotalReceitas = mesesReceitas.GetValueOrDefault($"{m.Ano}-{m.MesNum}", 0m)
            }).ToList();

            // Spending trends (#6) - média de 3 meses por categoria
            var mediasCategoria = despesas
                .Where(d => d.Categoria != null)
                .GroupBy(d => d.Categoria!.Nome)
                .Select(g =>
                {
                    var mesesDistintos = g.Select(x => new { x.Data.Year, x.Data.Month }).Distinct().Count();
                    var media = mesesDistintos > 0 ? g.Sum(x => x.Valor) / mesesDistintos : 0;
                    var mesAtual = g.Where(x => x.Data.Month == now.Month && x.Data.Year == now.Year).Sum(x => x.Valor);
                    var variacao = media > 0 ? ((mesAtual - media) / media) * 100 : 0;

                    return new
                    {
                        Categoria = g.Key,
                        MediaMensal = media,
                        MesAtual = mesAtual,
                        Variacao = variacao
                    };
                })
                .Where(x => x.MediaMensal > 0)
                .OrderByDescending(x => Math.Abs(x.Variacao))
                .ToList();

            // Net worth / savings tracking (#7) - cumulative over time
            var allDespesas = await _context.Despesas
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.Data)
                .ToListAsync();

            var allReceitas = await _context.Receitas
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.Data)
                .ToListAsync();

            var mesesPoupanca = allDespesas.Select(d => new { d.Data.Year, d.Data.Month })
                .Union(allReceitas.Select(r => new { r.Data.Year, r.Data.Month }))
                .Distinct()
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            decimal acumulado = 0;
            var poupancaAcumulada = mesesPoupanca.Select(m =>
            {
                var receitasMes = allReceitas.Where(r => r.Data.Year == m.Year && r.Data.Month == m.Month).Sum(r => r.Valor);
                var despesasMes = allDespesas.Where(d => d.Data.Year == m.Year && d.Data.Month == m.Month).Sum(d => d.Valor);
                acumulado += receitasMes - despesasMes;

                return new
                {
                    Mes = $"{CultureInfo.GetCultureInfo("pt-PT").DateTimeFormat.GetAbbreviatedMonthName(m.Month)} {m.Year}",
                    Acumulado = acumulado
                };
            }).ToList();

            ViewBag.TotalDespesas = totalDespesas;
            ViewBag.TotalReceitas = totalReceitas;
            ViewBag.Saldo = saldo;
            ViewBag.PorCategoria = porCategoria;
            ViewBag.Tendencia = tendencia;
            ViewBag.MediasCategoria = mediasCategoria;
            ViewBag.PoupancaAcumulada = poupancaAcumulada;

            return View();
        }
    }
}
