using GestaoDespesas.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
                    Data = DateTime.UtcNow.AddDays(-rnd.Next(0, 120)),
                    CategoriaId = cat.CategoriaId,
                    UserId = userId
                });
            }

            context.Despesas.AddRange(despesas);

            // ORÇAMENTOS (este mês)
            var orcamentos = categorias.Select(c => new Orcamento
            {
                Ano = DateTime.UtcNow.Year,
                Mes = DateTime.UtcNow.Month,
                CategoriaId = c.CategoriaId,
                Limite = 300,
                UserId = userId
            });

            context.Orcamentos.AddRange(orcamentos);

            // RECEITAS
            var tiposReceita = new[] { "Salário", "Freelance", "Investimentos", "Reembolso", "Outros" };

            var receitas = new List<Receita>();

            for (int i = 0; i < 15; i++)
            {
                receitas.Add(new Receita
                {
                    Descricao = $"Receita teste {i}",
                    Valor = rnd.Next(50, 2000),
                    Data = DateTime.UtcNow.AddDays(-rnd.Next(0, 120)),
                    Tipo = tiposReceita[rnd.Next(tiposReceita.Length)],
                    UserId = userId
                });
            }

            context.Receitas.AddRange(receitas);

            // DESPESAS RECORRENTES
            var frequencias = new[] { "Semanal", "Mensal", "Anual" };

            var recorrentes = new List<DespesaRecorrente>
            {
                new()
                {
                    Descricao = "Renda mensal",
                    Valor = 550,
                    CategoriaId = categorias[3].CategoriaId, // Habitação
                    Frequencia = "Mensal",
                    DataInicio = DateTime.UtcNow.AddMonths(-6),
                    Ativa = true,
                    UltimaGeracao = DateTime.UtcNow.AddDays(-5),
                    UserId = userId
                },
                new()
                {
                    Descricao = "Passe de transportes",
                    Valor = 40,
                    CategoriaId = categorias[1].CategoriaId, // Transportes
                    Frequencia = "Mensal",
                    DataInicio = DateTime.UtcNow.AddMonths(-3),
                    Ativa = true,
                    UltimaGeracao = DateTime.UtcNow.AddDays(-10),
                    UserId = userId
                },
                new()
                {
                    Descricao = "Ginásio",
                    Valor = 30,
                    CategoriaId = categorias[2].CategoriaId, // Lazer
                    Frequencia = "Mensal",
                    DataInicio = DateTime.UtcNow.AddMonths(-4),
                    Ativa = true,
                    UserId = userId
                },
                new()
                {
                    Descricao = "Seguro de saúde anual",
                    Valor = 480,
                    CategoriaId = categorias[4].CategoriaId, // Saúde
                    Frequencia = "Anual",
                    DataInicio = DateTime.UtcNow.AddYears(-1),
                    Ativa = true,
                    UltimaGeracao = DateTime.UtcNow.AddMonths(-1),
                    UserId = userId
                },
                new()
                {
                    Descricao = "Compras semanais supermercado",
                    Valor = 60,
                    CategoriaId = categorias[0].CategoriaId, // Alimentação
                    Frequencia = "Semanal",
                    DataInicio = DateTime.UtcNow.AddMonths(-2),
                    DataFim = DateTime.UtcNow.AddMonths(1),
                    Ativa = true,
                    UserId = userId
                }
            };

            context.DespesasRecorrentes.AddRange(recorrentes);

            // REGISTOS DE AUDITORIA
            var registos = new List<RegistoAuditoria>();

            for (int i = 0; i < 10; i++)
            {
                var cat = categorias[rnd.Next(categorias.Count)];
                var acoes = new[] { "Criar", "Editar", "Eliminar" };

                registos.Add(new RegistoAuditoria
                {
                    UserId = userId,
                    Entidade = "Despesa",
                    EntidadeId = i + 1,
                    Acao = acoes[rnd.Next(acoes.Length)],
                    Detalhes = $"Operação de teste {i} na despesa",
                    DataHora = DateTime.UtcNow.AddDays(-rnd.Next(0, 60))
                });
            }

            context.RegistosAuditoria.AddRange(registos);

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