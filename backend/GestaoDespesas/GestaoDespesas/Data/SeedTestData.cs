using GestaoDespesas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace GestaoDespesas.Data
{
    public static class SeedTestData
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IConfiguration config)
        {
            var adminEmail = config["SeedAdmin:Email"];
            if (string.IsNullOrWhiteSpace(adminEmail)) return;

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null) return;

            var userId = admin.Id;

            if (context.Despesas.Any(d => d.UserId == userId))
                return;

            // CATEGORIAS
            var categorias = new List<Categoria>
            {
                new() { Nome = "Alimentação", UserId = userId },
                new() { Nome = "Transportes", UserId = userId },
                new() { Nome = "Lazer", UserId = userId },
                new() { Nome = "Habitação", UserId = userId },
                new() { Nome = "Saúde", UserId = userId }
            };

            context.Categorias.AddRange(categorias);
            await context.SaveChangesAsync();

            // DESPESAS
            var rnd = new Random();

            var despesas = new List<Despesa>();

            for (int i = 0; i < 40; i++)
            {
                var cat = categorias[rnd.Next(categorias.Count)];

                despesas.Add(new Despesa
                {
                    Descricao = $"Despesa teste {i}",
                    Valor = rnd.Next(5, 120),
                    Data = DateTime.Now.AddDays(-rnd.Next(0, 120)),
                    CategoriaId = cat.CategoriaId,
                    UserId = userId
                });
            }

            context.Despesas.AddRange(despesas);

            // ORÇAMENTOS (este mês)
            var orcamentos = categorias.Select(c => new Orcamento
            {
                Ano = DateTime.Now.Year,
                Mes = DateTime.Now.Month,
                CategoriaId = c.CategoriaId,
                Limite = 300,
                UserId = userId
            });

            context.Orcamentos.AddRange(orcamentos);

            // PERFIL
            context.UserProfiles.Add(new UserProfile
            {
                UserId = userId,
                SalarioMensal = 1200,
                LimitePercentual = 50
            });

            await context.SaveChangesAsync();
        }
    }
}