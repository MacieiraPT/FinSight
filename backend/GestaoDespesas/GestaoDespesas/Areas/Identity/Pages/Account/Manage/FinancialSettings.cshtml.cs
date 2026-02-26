#nullable disable
using GestaoDespesas.Data;
using GestaoDespesas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestaoDespesas.Areas.Identity.Pages.Account.Manage
{
    public class FinancialSettingsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public FinancialSettingsModel(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public decimal SalarioMensal { get; set; }
            public int LimitePercentual { get; set; }
            public bool ReceberAlertas { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = user.Id,
                    SalarioMensal = 0,
                    LimitePercentual = 50,
                    ReceberAlertas = true
                };

                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            Input = new InputModel
            {
                SalarioMensal = profile.SalarioMensal,
                LimitePercentual = profile.LimitePercentual,
                ReceberAlertas = profile.ReceberAlertas
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
                return NotFound();

            profile.SalarioMensal = Input.SalarioMensal;
            profile.LimitePercentual = Input.LimitePercentual;
            profile.ReceberAlertas = Input.ReceberAlertas;

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}