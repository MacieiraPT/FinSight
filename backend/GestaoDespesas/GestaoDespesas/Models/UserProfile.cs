using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GestaoDespesas.Models
{
    public class UserProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        [Display(Name = "Salário mensal (€)")]
        public decimal SalarioMensal { get; set; }

        [Display(Name = "Limite mensal (%)")]
        public int LimitePercentual { get; set; } = 50;

        public bool ReceberAlertas { get; set; } = true;
    }
}