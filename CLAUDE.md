# CLAUDE.md — FinSight Codebase Guide

This document helps AI assistants (Claude and others) understand the FinSight repository, its architecture, conventions, and development workflows.

---

## Project Overview

**FinSight** is a personal finance management web application for tracking expenses, managing budgets, and visualising spending trends. The UI is in **Portuguese (pt-PT)**.

- **Stack:** ASP.NET Core 8 MVC + PostgreSQL + Bootstrap 5
- **Auth:** ASP.NET Core Identity (email/password + Google OAuth + 2FA)
- **Charts:** Chart.js
- **Export:** ClosedXML (Excel), StringBuilder (CSV)

---

## Repository Layout

```
FinSight/
├── backend/
│   └── GestaoDespesas/
│       ├── GestaoDespesas/              # Main .NET 8 project
│       │   ├── Controllers/             # MVC controllers
│       │   ├── Data/                    # EF Core DbContext + seeding
│       │   ├── Migrations/              # EF Core migrations
│       │   ├── Models/                  # Domain models
│       │   ├── Views/                   # Razor views (.cshtml)
│       │   │   ├── Dashboard/
│       │   │   ├── Despesas/            # Expense views
│       │   │   ├── Categorias/          # Category views
│       │   │   ├── Orcamentos/          # Budget views
│       │   │   ├── Home/
│       │   │   └── Shared/              # _Layout.cshtml, partials
│       │   ├── Areas/
│       │   │   └── Identity/            # Auth Razor Pages (login, 2FA, settings)
│       │   ├── Properties/
│       │   │   └── launchSettings.json
│       │   ├── Program.cs               # App entry point & DI configuration
│       │   ├── GestaoDespesas.csproj    # Project file & NuGet dependencies
│       │   └── appsettings.Example.json # Config template (copy → appsettings.json)
│       ├── GestaoDespesas.slnx          # Solution file
│       └── .config/dotnet-tools.json   # dotnet-ef tool pin
├── database/
│   ├── init.sql                         # Full schema (reference only)
│   └── create_db.sql                    # DB + user creation SQL
├── instalar.bat                         # Windows: install deps, create DB, run migrations
├── iniciar.bat                          # Windows: start the app
├── parar.bat                            # Windows: stop the app
├── README.md
└── LICENSE.txt                          # MIT
```

---

## Domain Model

| Model | Table | Description |
|---|---|---|
| `Despesa` | `Despesas` | Expense transaction (value, date, category, notes, user) |
| `Categoria` | `Categorias` | Expense category (name, user) — unique per user |
| `Orcamento` | `Orcamentos` | Monthly budget limit per category (year, month, limit, user) |
| `UserProfile` | `UserProfiles` | Extends IdentityUser with salary, limit %, alert toggle |

**Key relationships:**
- `Categoria` → `Despesa` (one-to-many, **restrict delete** — cannot delete a category that has expenses)
- `Categoria` → `Orcamento` (one-to-many)
- `IdentityUser` → `UserProfile` (one-to-one)

All user-owned entities carry a `UserId` FK and all queries **must be scoped to the current user** (`User.FindFirstValue(ClaimTypes.NameIdentifier)`).

---

## Controllers & Routes

Default route: `{controller=Dashboard}/{action=Index}/{id?}`

### DashboardController
| Action | Route | Notes |
|---|---|---|
| `Index` | `GET /Dashboard` | Monthly totals, charts, budget progress, alerts |

### DespesasController
| Action | Route | Notes |
|---|---|---|
| `Index` | `GET /Despesas` | Filterable (categoriaId, ano, mes, q), sortable, paginated (10/page) |
| `Create` | `GET/POST /Despesas/Create` | Date validation: no future dates |
| `Edit` | `GET/POST /Despesas/Edit/{id}` | |
| `Delete` | `GET/POST /Despesas/Delete/{id}` | |
| `Details` | `GET /Despesas/Details/{id}` | |
| `ExportarCsv` | `POST /Despesas/ExportarCsv` | Respects active filters |
| `ExportarExcel` | `POST /Despesas/ExportarExcel` | Respects active filters, uses ClosedXML |

### CategoriasController
| Action | Route | Notes |
|---|---|---|
| `Index` | `GET /Categorias` | |
| `Create` | `GET/POST /Categorias/Create` | Unique name per user enforced |
| `Edit` | `GET/POST /Categorias/Edit/{id}` | |
| `Delete` | `GET/POST /Categorias/Delete/{id}` | Blocked if expenses exist |
| `Details` | `GET /Categorias/Details/{id}` | |
| `SeedDefault` | `POST /Categorias/SeedDefault` | Adds 10 default PT categories |

### OrcamentosController
| Action | Route | Notes |
|---|---|---|
| `Index` | `GET /Orcamentos` | |
| `Create` | `GET/POST /Orcamentos/Create` | |
| `Edit` | `GET/POST /Orcamentos/Edit/{id}` | |
| `Delete` | `GET/POST /Orcamentos/Delete/{id}` | |
| `Details` | `GET /Orcamentos/Details/{id}` | |

### Identity (Razor Pages — `/Areas/Identity/`)
- `Account/Login`, `Account/Logout`, `Account/Register`
- `Account/ExternalLogin` (Google OAuth)
- `Account/LoginWith2fa`, `Account/Manage/TwoFactorAuthentication`, `Account/Manage/EnableAuthenticator`
- `Account/Manage/FinancialSettings` — custom page for salary/limit/alert config
- `Account/Manage/Index`

---

## Data Layer

**`ApplicationDbContext`** (`Data/ApplicationDbContext.cs`):
- Inherits `IdentityDbContext<IdentityUser>`
- Registers `Despesas`, `Categorias`, `Orcamentos`, `UserProfiles` DbSets
- Configures UTC datetime conversion for all `DateTime` properties via model builder
- Enforces unique constraint on `(UserId, Nome)` in `Categorias`
- Sets delete behaviour to `Restrict` for Categoria → Despesa to protect data integrity

**`SeedTestData`** (`Data/SeedTestData.cs`):
- Creates a test user (`test@finsight.pt` / `Test123!`) on first run
- Seeds 5 categories, 5 budgets, and 40 expenses for testing

---

## Configuration

The app reads from `appsettings.json` (git-ignored). Copy the example template to get started:

```bash
cp appsettings.Example.json appsettings.json
# Then fill in real values
```

**Required sections:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DB_FinSight;Username=finsight;Password=finsight123"
  },
  "SeedAdmin": {
    "Email": "test@finsight.pt",
    "Password": "Test123!"
  },
  "Auth": {
    "Google": {
      "ClientId": "<google-client-id>",
      "ClientSecret": "<google-client-secret>"
    }
  }
}
```

**Never commit `appsettings.json` or `appsettings.Development.json`** — both are in `.gitignore`.

---

## Local Development Setup (Windows)

```bat
REM 1. Install dotnet-ef tool and create the PostgreSQL DB
instalar.bat

REM 2. Start the app (opens https://localhost:7093)
iniciar.bat

REM 3. Stop the app
parar.bat
```

### Manual steps (cross-platform)

```bash
# Restore tools
dotnet tool restore

# Create DB (requires psql)
psql -U postgres -c "CREATE USER finsight WITH PASSWORD 'finsight123';"
psql -U postgres -c "CREATE DATABASE \"DB_FinSight\" OWNER finsight;"

# Run migrations
cd backend/GestaoDespesas
dotnet ef database update --project GestaoDespesas

# Run the app
dotnet run --project GestaoDespesas
```

App runs at:
- HTTP: `http://localhost:5067`
- HTTPS: `https://localhost:7093`

---

## Database Migrations

Tool: `dotnet-ef` (pinned in `.config/dotnet-tools.json`).

```bash
# Working directory: backend/GestaoDespesas/

# Create a new migration
dotnet ef migrations add <MigrationName> --project GestaoDespesas

# Apply pending migrations
dotnet ef database update --project GestaoDespesas

# Revert last migration
dotnet ef database update <PreviousMigrationName> --project GestaoDespesas
```

Existing migrations:
- `20260226191251_TestData` — initial schema
- `20260226210840_testAcc` — account adjustments

---

## Key Conventions

### Security — always enforce user isolation

Every query that touches user-owned data must filter by `UserId`. Never skip this.

```csharp
// Correct
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
var despesas = await _context.Despesas
    .Where(d => d.UserId == userId)
    .ToListAsync();

// Wrong — exposes other users' data
var despesas = await _context.Despesas.ToListAsync();
```

### Dates — always use UTC

`ApplicationDbContext` converts all `DateTime` columns to UTC. When setting dates, use `DateTime.UtcNow` or convert with `.ToUniversalTime()`. The model validates that expense dates are not in the future.

### Language — Portuguese (pt-PT)

All UI text, validation messages, field names, and user-facing strings are in **Portuguese**. Maintain this when adding new views or validation messages.

### Delete protection

Categories with associated expenses cannot be deleted (EF `Restrict` delete behaviour). Controllers must catch `DbUpdateException` and show a user-friendly error when this constraint is violated.

### Pagination

The `DespesasController.Index` uses a `page` query parameter (default 1) with a `PageSize` constant of `10`. Maintain this pattern for any list views that could grow large.

### Exports

Both CSV and Excel exports apply the same filters as the list view (`categoriaId`, `ano`, `mes`, `q`). When adding new filter parameters to the list, apply them to the export actions too.

---

## NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.24 | Identity + EF |
| `Microsoft.AspNetCore.Identity.UI` | 8.0.24 | Scaffolded Identity Razor Pages |
| `Microsoft.AspNetCore.Authentication.Google` | 8.0.24 | Google OAuth |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.11 | PostgreSQL EF provider |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.24 | dotnet ef CLI |
| `ClosedXML` | 0.105.0 | Excel export (.xlsx) |
| `QRCoder` | 1.7.0 | 2FA QR code generation |
| `Microsoft.EntityFrameworkCore.Sqlite` | 8.0.23 | SQLite provider (dev/test use) |
| `Microsoft.EntityFrameworkCore.SqlServer` | 8.0.23 | SQL Server provider (optional) |

---

## No Test Project

There is currently no automated test project. Test coverage is provided by:
- Manual testing with the seeded test account (`test@finsight.pt` / `Test123!`)
- `SeedTestData.cs` which populates realistic sample data on first run

When adding tests in the future, create a `GestaoDespesas.Tests` project in the same solution.

---

## Common Pitfalls

1. **Missing `appsettings.json`** — the file is git-ignored; copy from `appsettings.Example.json`
2. **PostgreSQL not running** — ensure the `DB_FinSight` database exists before `dotnet run`
3. **Unapplied migrations** — run `dotnet ef database update` after pulling new migrations
4. **Future dates on expenses** — the model rejects dates in the future; use past/present dates in tests
5. **Category delete blocked** — if a category has expenses, deletion is rejected at the DB level
6. **Missing UTC conversion** — all datetimes must be UTC-stored; use `DateTime.UtcNow`
7. **User isolation** — forgetting `UserId` filter will silently leak data across user accounts
