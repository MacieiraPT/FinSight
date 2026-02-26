#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GestaoDespesas.Data;
using GestaoDespesas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GestaoDespesas.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {

            [Display(Name = "Salário Mensal (€)")]
            public decimal SalarioMensal { get; set; }

            [Display(Name = "Limite Mensal (%)")]
            [Range(1, 100)]
            public int LimitePercentual { get; set; }

            [Display(Name = "Receber Alertas")]
            public bool ReceberAlertas { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

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

            Username = userName;

            Input = new InputModel
            {
                SalarioMensal = profile.SalarioMensal,
                LimitePercentual = profile.LimitePercentual,
                ReceberAlertas = profile.ReceberAlertas
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // --------- FinSight Profile Logic ---------

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
            }

            profile.SalarioMensal = Input.SalarioMensal;
            profile.LimitePercentual = Input.LimitePercentual;
            profile.ReceberAlertas = Input.ReceberAlertas;

            await _context.SaveChangesAsync();

            // ------------------------------------------

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Definições financeiras atualizadas com sucesso.";
            return RedirectToPage();
        }
    }
}