using System.Globalization;
using Microsoft.AspNetCore.Localization;
using GestaoDespesas.Data;
using GestaoDespesas.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<AuditoriaService>();
builder.Services.AddScoped<DespesasRecorrentesService>();

builder.Services.AddControllersWithViews();

var culture = new CultureInfo("pt-PT");

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pt-PT");
    options.SupportedCultures = new[] { culture };
    options.SupportedUICultures = new[] { culture };
});


builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Auth:Google:ClientId"]
            ?? throw new Exception("Google ClientId em falta!");
        googleOptions.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"]
            ?? throw new Exception("Google ClientSecret em falta!");
    });

var app = builder.Build();

// Force HTTPS port if not specified
if (!app.Environment.IsProduction())
{
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
    if (string.IsNullOrEmpty(env))
    {
        app.Urls.Clear();
        app.Urls.Add("https://localhost:7093");
        app.Urls.Add("http://localhost:5067");
    }
}

var locOptions = app.Services.GetRequiredService<
    Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>().Value;

// Seed: criar utilizador admin + dados de teste
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var context = services.GetRequiredService<ApplicationDbContext>();
    var config = services.GetRequiredService<IConfiguration>();

    var adminEmail = config["SeedAdmin:Email"];
    var adminPass = config["SeedAdmin:Password"];

    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPass))
    {
        var existing = await userManager.FindByEmailAsync(adminEmail);

        if (existing == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPass);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    Console.WriteLine($"[SeedAdmin] {err.Code}: {err.Description}");
            }
        }
    }

    // 👉 AGORA criar Categorias / Despesas / Orçamentos
    await SeedTestData.SeedAsync(context, userManager, config);

    // Processar despesas recorrentes pendentes
    var recorrentesService = new DespesasRecorrentesService(context);
    await recorrentesService.ProcessarRecorrentesAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseRequestLocalization(locOptions);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
