using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Data;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var now = DateTime.UtcNow;

        var inicioMes = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var fimMes = inicioMes.AddMonths(1);

        var despesasMes = await _context.Despesas
            .Where(d => d.UserId == userId &&
                        d.Data >= inicioMes &&
                        d.Data < fimMes)
            .ToListAsync();

        var totalMes = despesasMes.Sum(d => d.Valor);

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        decimal limitePermitido = 0;
        decimal percentagemUsada = 0;
        bool alerta = false;
        bool alertaGrave = false;

        if (profile != null && profile.SalarioMensal > 0)
        {
            limitePermitido = profile.SalarioMensal * profile.LimitePercentual / 100m;
            percentagemUsada = limitePermitido > 0
                ? (totalMes / limitePermitido) * 100
                : 0;

            if (percentagemUsada > 100)
                alertaGrave = true;
            else if (percentagemUsada > 80)
                alerta = true;
        }

        // Distribuição por categoria
        var porCategoria = await _context.Despesas
            .Where(d => d.UserId == userId &&
                        d.Data.Month == now.Month &&
                        d.Data.Year == now.Year &&
                        d.Categoria != null)
            .GroupBy(d => d.Categoria!.Nome)
            .Select(g => new
            {
                Categoria = g.Key,
                Total = g.Sum(x => x.Valor)
            })
            .ToListAsync();

        // Últimos 6 meses
        var seisMeses = Enumerable.Range(0, 6)
            .Select(i => new DateTime(now.Year, now.Month, 1).AddMonths(-i))
            .OrderBy(d => d)
            .ToList();

        var despesas6Meses = await _context.Despesas
            .Where(d => d.UserId == userId &&
                        d.Data >= seisMeses.First())
            .ToListAsync();

        var dados6Meses = seisMeses.Select(m => new
        {
            Mes = m.ToString("MMM yyyy"),
            Total = despesas6Meses
                .Where(d => d.Data.Month == m.Month && d.Data.Year == m.Year)
                .Sum(d => d.Valor)
        }).ToList();

        // Orçamentos do mês
        // Buscar orçamentos com categorias
        var orcamentos = await _context.Orcamentos
            .Include(o => o.Categoria)
            .Where(o => o.UserId == userId &&
                        o.Mes == now.Month &&
                        o.Ano == now.Year)
            .ToListAsync();

        // Agora calcular em memória
        var progressoOrcamentos = orcamentos
            .Where(o => o.Categoria != null)
            .Select(o => new
            {
                Categoria = o.Categoria!.Nome,
                Limite = o.Limite,
                TotalGasto = despesasMes
                    .Where(d => d.CategoriaId == o.CategoriaId)
                    .Sum(d => d.Valor)
            })
            .ToList();

        ViewBag.TotalMes = totalMes;
        ViewBag.Categorias = porCategoria;
        ViewBag.SeisMeses = dados6Meses;
        ViewBag.Progresso = progressoOrcamentos;
        ViewBag.LimitePermitido = limitePermitido;
        ViewBag.PercentagemUsada = percentagemUsada;
        ViewBag.Alerta = alerta;
        ViewBag.AlertaGrave = alertaGrave;

        return View();
    }
}
