using FinSightPro.Application.Services;
using FinSightPro.Domain.Entities;
using FinSightPro.Domain.Enums;
using FinSightPro.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinSightPro.Infrastructure.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<ApplicationDbContext>>();
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var config = sp.GetRequiredService<IConfiguration>();

        await db.Database.MigrateAsync();

        var seedEmail = config["SeedAdmin:Email"] ?? "demo@finsightpro.pt";
        var seedPassword = config["SeedAdmin:Password"] ?? "Demo123!";
        var seedName = config["SeedAdmin:Name"] ?? "Demo FinSight";

        var user = await userManager.FindByEmailAsync(seedEmail);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = seedEmail,
                Email = seedEmail,
                EmailConfirmed = true,
                Name = seedName,
                MonthlySalary = 1500m,
                Currency = "EUR",
                Theme = "light"
            };
            var result = await userManager.CreateAsync(user, seedPassword);
            if (!result.Succeeded)
            {
                logger.LogError("Failed to seed user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }
            logger.LogInformation("Seed user created: {Email}", seedEmail);
        }

        if (!await db.Categories.AnyAsync(c => c.UserId == user.Id))
        {
            foreach (var (name, icon, color) in CategoryService.DefaultCategories())
            {
                db.Categories.Add(new Category
                {
                    UserId = user.Id,
                    Name = name,
                    Icon = icon,
                    Color = color,
                    IsSystem = false
                });
            }
            await db.SaveChangesAsync();
        }

        var categories = await db.Categories.Where(c => c.UserId == user.Id).ToListAsync();

        if (!await db.Expenses.AnyAsync(e => e.UserId == user.Id))
        {
            var rand = new Random(42);
            var today = DateTime.UtcNow.Date;
            var start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-2);
            var samples = new[]
            {
                ("Supermercado Continente", "Alimentação", 45m, 65m),
                ("Restaurante", "Alimentação", 15m, 35m),
                ("Combustível", "Transporte", 40m, 70m),
                ("Passe Mensal", "Transporte", 30m, 30m),
                ("Farmácia", "Saúde", 8m, 25m),
                ("Cinema", "Lazer", 7m, 15m),
                ("Netflix", "Subscrições", 12.99m, 12.99m),
                ("Spotify", "Subscrições", 6.99m, 6.99m),
                ("Eletricidade", "Habitação", 45m, 80m),
                ("Internet", "Habitação", 35m, 50m),
                ("Livros", "Educação", 12m, 30m),
                ("Roupa", "Compras", 25m, 80m)
            };

            for (var d = start; d <= today; d = d.AddDays(1))
            {
                if (rand.Next(0, 100) > 70) continue;
                var s = samples[rand.Next(samples.Length)];
                var cat = categories.FirstOrDefault(c => c.Name == s.Item2);
                if (cat == null) continue;
                var amount = Math.Round((decimal)(rand.NextDouble() * (double)(s.Item4 - s.Item3) + (double)s.Item3), 2);
                db.Expenses.Add(new Expense
                {
                    UserId = user.Id,
                    CategoryId = cat.Id,
                    Description = s.Item1,
                    Amount = amount,
                    Date = DateTime.SpecifyKind(d, DateTimeKind.Utc),
                    PaymentMethod = (PaymentMethod)rand.Next(0, 5)
                });
            }
            await db.SaveChangesAsync();
        }

        if (!await db.Incomes.AnyAsync(i => i.UserId == user.Id))
        {
            var today = DateTime.UtcNow.Date;
            for (int i = 0; i < 3; i++)
            {
                var d = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
                db.Incomes.Add(new Income
                {
                    UserId = user.Id,
                    Description = "Salário",
                    Amount = 1500m,
                    Date = d,
                    IsFixed = true,
                    IsRecurring = false
                });
                if (i % 2 == 0)
                {
                    db.Incomes.Add(new Income
                    {
                        UserId = user.Id,
                        Description = "Freelance",
                        Amount = 250m,
                        Date = d.AddDays(10),
                        IsFixed = false,
                        IsRecurring = false
                    });
                }
            }
            await db.SaveChangesAsync();
        }

        if (!await db.Budgets.AnyAsync(b => b.UserId == user.Id))
        {
            var today = DateTime.UtcNow;
            var budgetTargets = new (string Cat, decimal Limit)[]
            {
                ("Alimentação", 350m),
                ("Transporte", 200m),
                ("Lazer", 100m),
                ("Habitação", 250m),
                ("Subscrições", 50m)
            };
            foreach (var (catName, limit) in budgetTargets)
            {
                var cat = categories.FirstOrDefault(c => c.Name == catName);
                if (cat == null) continue;
                db.Budgets.Add(new Budget
                {
                    UserId = user.Id,
                    CategoryId = cat.Id,
                    MonthlyLimit = limit,
                    Month = today.Month,
                    Year = today.Year
                });
            }
            await db.SaveChangesAsync();
        }
    }
}
