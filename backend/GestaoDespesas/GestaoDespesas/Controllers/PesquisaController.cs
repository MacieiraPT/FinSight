using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using GestaoDespesas.Data;

namespace GestaoDespesas.Controllers
{
    [Authorize]
    public class PesquisaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PesquisaController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? q)
        {
            ViewBag.Q = q;

            if (string.IsNullOrWhiteSpace(q))
            {
                return View();
            }

            var userId = _userManager.GetUserId(User);

            var despesas = await _context.Despesas
                .Include(d => d.Categoria)
                .Where(d => d.UserId == userId && d.Descricao.Contains(q))
                .OrderByDescending(d => d.Data)
                .Take(20)
                .ToListAsync();

            var receitas = await _context.Receitas
                .Where(r => r.UserId == userId && r.Descricao.Contains(q))
                .OrderByDescending(r => r.Data)
                .Take(20)
                .ToListAsync();

            var categorias = await _context.Categorias
                .Where(c => c.UserId == userId && c.Nome.Contains(q))
                .OrderBy(c => c.Nome)
                .Take(20)
                .ToListAsync();

            ViewBag.Despesas = despesas;
            ViewBag.Receitas = receitas;
            ViewBag.Categorias = categorias;
            ViewBag.TotalResultados = despesas.Count + receitas.Count + categorias.Count;

            return View();
        }
    }
}
