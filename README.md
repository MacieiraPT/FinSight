<div align="center">

<img src="https://capsule-render.vercel.app/api?type=waving&color=gradient&customColorList=12,20,24&height=220&section=header&text=FinSight&fontSize=80&fontColor=ffffff&animation=fadeIn&fontAlignY=38&desc=Smart%20Personal%20Finance%20Management&descAlignY=58&descSize=18" width="100%" alt="FinSight banner" />

<a href="https://github.com/MacieiraPT/finsight">
  <img src="https://readme-typing-svg.herokuapp.com/?font=Fira+Code&size=22&duration=3500&pause=800&color=2EB872&center=true&vCenter=true&width=720&lines=Track+expenses+with+confidence.;Set+budgets+that+actually+work.;Visualise+spending%2C+gain+insight.;Built+with+ASP.NET+Core+8+%E2%9A%A1+PostgreSQL." alt="Typing SVG" />
</a>

<br/>

<p>
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 8" />
  <img src="https://img.shields.io/badge/ASP.NET_Core-MVC-5C2D91?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core MVC" />
  <img src="https://img.shields.io/badge/PostgreSQL-16-336791?style=for-the-badge&logo=postgresql&logoColor=white" alt="PostgreSQL" />
  <img src="https://img.shields.io/badge/Bootstrap-5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" alt="Bootstrap 5" />
  <img src="https://img.shields.io/badge/Chart.js-FF6384?style=for-the-badge&logo=chartdotjs&logoColor=white" alt="Chart.js" />
</p>

<p>
  <img src="https://img.shields.io/badge/license-MIT-2EB872?style=flat-square" alt="MIT License" />
  <img src="https://img.shields.io/badge/language-Portuguese%20(pt--PT)-006600?style=flat-square" alt="Language pt-PT" />
  <img src="https://img.shields.io/badge/status-active-brightgreen?style=flat-square" alt="Status active" />
  <img src="https://img.shields.io/badge/PRs-welcome-blueviolet?style=flat-square" alt="PRs welcome" />
  <img src="https://img.shields.io/github/last-commit/MacieiraPT/finsight?style=flat-square&color=0078D4" alt="Last commit" />
  <img src="https://img.shields.io/github/repo-size/MacieiraPT/finsight?style=flat-square&color=ff6b6b" alt="Repo size" />
</p>

</div>

---

## 🌟 About the Project

> **FinSight** is a personal finance management web application that helps you **track expenses**, **manage monthly budgets**, and **visualise spending trends** through an elegant, accessible Portuguese (pt-PT) interface.
>
> Built as part of *UFCD 5425 – Bases de Dados*, FinSight pairs a clean ASP.NET Core 8 MVC architecture with a PostgreSQL backend, secure ASP.NET Core Identity authentication (Email · Google · 2FA), and a responsive Bootstrap 5 dashboard powered by Chart.js.

<div align="center">

| 🎯 Goal | 🔐 Privacy | 📈 Insight |
|:---:|:---:|:---:|
| Help users take control of personal spending | Strict per-user data isolation everywhere | Charts, budgets, and alerts that act before you overspend |

</div>

---

## ✨ Features

<table>
  <tr>
    <td width="50%" valign="top">

### 💸 Expense Tracking
- Filterable, sortable, paginated lists
- Search by note · category · year · month
- Date validation: no future entries
- Per-user isolation enforced at every query

### 🗂️ Smart Categories
- Unique category names per user
- Restrict-delete protection (no orphaned data)
- One-click seed of 10 default PT categories

    </td>
    <td width="50%" valign="top">

### 🎯 Monthly Budgets
- Set spending limits per category, per month
- Real-time progress bars on the dashboard
- Automatic alerts when you approach the limit

### 📊 Insightful Dashboard
- Monthly totals at a glance
- Distribution by category
- 6-month spending evolution
- Salary-based personalised alerts

    </td>
  </tr>
  <tr>
    <td width="50%" valign="top">

### 🔐 Secure Authentication
- ASP.NET Core Identity (email + password)
- Google OAuth single-sign-on
- TOTP two-factor authentication (QR code)
- Per-user financial settings page

    </td>
    <td width="50%" valign="top">

### 📤 Data Export
- Export filtered expenses to **CSV**
- Export filtered expenses to **Excel (.xlsx)**
- Both honour the same active filters
- Powered by ClosedXML

    </td>
  </tr>
</table>

---

## 🧰 Tech Stack

<div align="center">

### Backend
<img src="https://img.shields.io/badge/.NET_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
<img src="https://img.shields.io/badge/ASP.NET_Core_MVC-5C2D91?style=for-the-badge&logo=dotnet&logoColor=white" />
<img src="https://img.shields.io/badge/Entity_Framework_Core-512BD4?style=for-the-badge&logo=nuget&logoColor=white" />
<img src="https://img.shields.io/badge/Identity-3E8EDE?style=for-the-badge&logo=microsoft&logoColor=white" />

### Database
<img src="https://img.shields.io/badge/PostgreSQL-336791?style=for-the-badge&logo=postgresql&logoColor=white" />
<img src="https://img.shields.io/badge/Npgsql-336791?style=for-the-badge&logo=postgresql&logoColor=white" />

### Frontend
<img src="https://img.shields.io/badge/Bootstrap_5-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white" />
<img src="https://img.shields.io/badge/Chart.js-FF6384?style=for-the-badge&logo=chartdotjs&logoColor=white" />
<img src="https://img.shields.io/badge/Razor_Views-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />

### Tooling & Libraries
<img src="https://img.shields.io/badge/ClosedXML-217346?style=for-the-badge&logo=microsoftexcel&logoColor=white" />
<img src="https://img.shields.io/badge/QRCoder-000000?style=for-the-badge&logo=qrcode&logoColor=white" />
<img src="https://img.shields.io/badge/Google_OAuth-4285F4?style=for-the-badge&logo=google&logoColor=white" />

</div>

---

## 🏗️ Architecture

```mermaid
flowchart LR
    U[👤 User] -->|HTTPS| W[🌐 ASP.NET Core 8 MVC]
    W --> A[🔐 Identity<br/>Email · Google · 2FA]
    W --> C[🎛️ Controllers<br/>Dashboard · Despesas · Categorias · Orcamentos]
    C --> E[⚙️ EF Core 8<br/>ApplicationDbContext]
    E --> D[(🐘 PostgreSQL<br/>DB_FinSight)]
    C --> X[📤 Exports<br/>CSV · Excel]
    C --> J[📊 Chart.js Dashboard]
    style U fill:#2EB872,color:#fff,stroke:#1f8d56
    style W fill:#512BD4,color:#fff,stroke:#3a1e9a
    style D fill:#336791,color:#fff,stroke:#234c69
```

### 📁 Repository Layout

```
FinSight/
├── backend/GestaoDespesas/
│   └── GestaoDespesas/
│       ├── Controllers/        # MVC controllers
│       ├── Data/               # ApplicationDbContext + SeedTestData
│       ├── Migrations/         # EF Core migrations
│       ├── Models/             # Domain: Despesa, Categoria, Orcamento, UserProfile
│       ├── Views/              # Razor views (.cshtml)
│       ├── Areas/Identity/     # Auth Razor Pages (Login, 2FA, Settings)
│       └── Program.cs          # App entry point & DI
├── database/
│   ├── init.sql                # Reference schema
│   └── create_db.sql           # DB + user creation
├── instalar.bat · iniciar.bat · parar.bat
└── README.md
```

---

## 🧱 Domain Model

<div align="center">

| Model | Table | Purpose |
|:---|:---|:---|
| 💸 `Despesa` | `Despesas` | Expense (value, date, category, notes, user) |
| 🗂️ `Categoria` | `Categorias` | Category — **unique per user** |
| 🎯 `Orcamento` | `Orcamentos` | Monthly budget limit per category |
| 👤 `UserProfile` | `UserProfiles` | Salary, limit %, alert toggle |

</div>

> **Key relationships**
> `Categoria` → `Despesa` (one-to-many, **restrict delete**) · `Categoria` → `Orcamento` (one-to-many) · `IdentityUser` → `UserProfile` (one-to-one)

---

## 🚦 Routes Overview

<details>
  <summary><b>📊 DashboardController</b></summary>

| Action | Route | Notes |
|:---|:---|:---|
| `Index` | `GET /Dashboard` | Monthly totals · charts · budget progress · alerts |

</details>

<details>
  <summary><b>💸 DespesasController (Expenses)</b></summary>

| Action | Route | Notes |
|:---|:---|:---|
| `Index` | `GET /Despesas` | Filterable · sortable · paginated (10/page) |
| `Create` | `GET/POST /Despesas/Create` | No future dates allowed |
| `Edit` | `GET/POST /Despesas/Edit/{id}` | |
| `Delete` | `GET/POST /Despesas/Delete/{id}` | |
| `Details` | `GET /Despesas/Details/{id}` | |
| `ExportarCsv` | `POST /Despesas/ExportarCsv` | Honours active filters |
| `ExportarExcel` | `POST /Despesas/ExportarExcel` | ClosedXML, honours filters |

</details>

<details>
  <summary><b>🗂️ CategoriasController (Categories)</b></summary>

| Action | Route | Notes |
|:---|:---|:---|
| `Index` | `GET /Categorias` | |
| `Create` | `GET/POST /Categorias/Create` | Unique name per user |
| `Edit` | `GET/POST /Categorias/Edit/{id}` | |
| `Delete` | `GET/POST /Categorias/Delete/{id}` | Blocked if expenses exist |
| `Details` | `GET /Categorias/Details/{id}` | |
| `SeedDefault` | `POST /Categorias/SeedDefault` | Adds 10 default PT categories |

</details>

<details>
  <summary><b>🎯 OrcamentosController (Budgets)</b></summary>

| Action | Route | Notes |
|:---|:---|:---|
| `Index` | `GET /Orcamentos` | |
| `Create` | `GET/POST /Orcamentos/Create` | |
| `Edit` | `GET/POST /Orcamentos/Edit/{id}` | |
| `Delete` | `GET/POST /Orcamentos/Delete/{id}` | |
| `Details` | `GET /Orcamentos/Details/{id}` | |

</details>

<details>
  <summary><b>🔐 Identity (Razor Pages)</b></summary>

- `Account/Login` · `Account/Logout` · `Account/Register`
- `Account/ExternalLogin` (Google OAuth)
- `Account/LoginWith2fa` · `Account/Manage/EnableAuthenticator`
- `Account/Manage/TwoFactorAuthentication`
- `Account/Manage/FinancialSettings` *(custom)*

</details>

---

## 🚀 Getting Started

### ⚙️ Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)

### 🪟 Windows — One-click scripts

```bat
:: 1️⃣ Install deps, create DB, apply migrations
instalar.bat

:: 2️⃣ Start the app  →  https://localhost:7093
iniciar.bat

:: 3️⃣ Stop the app
parar.bat
```

### 🐧 Cross-platform (manual)

```bash
# Restore the dotnet-ef tool
dotnet tool restore

# Create the PostgreSQL user & database
psql -U postgres -c "CREATE USER finsight WITH PASSWORD 'finsight123';"
psql -U postgres -c "CREATE DATABASE \"DB_FinSight\" OWNER finsight;"

# Apply migrations
cd backend/GestaoDespesas
dotnet ef database update --project GestaoDespesas

# Run
dotnet run --project GestaoDespesas
```

<div align="center">

| Endpoint | URL |
|:---:|:---:|
| 🔒 HTTPS | `https://localhost:7093` |
| 🌐 HTTP | `http://localhost:5067` |

</div>

---

## ⚙️ Configuration

The app reads from `appsettings.json` (git-ignored). Copy the example template:

```bash
cp appsettings.Example.json appsettings.json
```

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

> ⚠️ **Never commit** `appsettings.json` or `appsettings.Development.json` — both are in `.gitignore`.

---

## 🧪 Test Account

After first run, a seeded test user is automatically available with realistic sample data (5 categories, 5 budgets, 40 expenses):

<div align="center">

| 📧 Email | 🔑 Password |
|:---:|:---:|
| `test@finsight.pt` | `Test123!` |

</div>

---

## 🧬 Database Migrations

```bash
# Working directory: backend/GestaoDespesas/

# New migration
dotnet ef migrations add <MigrationName> --project GestaoDespesas

# Apply pending
dotnet ef database update --project GestaoDespesas

# Revert
dotnet ef database update <PreviousMigrationName> --project GestaoDespesas
```

---

## 🛡️ Conventions & Best Practices

| Convention | Rule |
|:---|:---|
| 🔒 **User isolation** | Every query filters by `UserId` from `ClaimTypes.NameIdentifier` |
| 🌍 **UTC dates** | All `DateTime` columns are converted to UTC by `ApplicationDbContext` |
| 🇵🇹 **Language** | UI, validation, and field names are in Portuguese (pt-PT) |
| 🚫 **Restrict delete** | Categories with expenses cannot be deleted |
| 📄 **Pagination** | List views default to 10 items per page |
| 📤 **Exports** | CSV & Excel honour the same filters as list views |

---

## 📦 Key NuGet Packages

| Package | Version | Purpose |
|:---|:---:|:---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 8.0.24 | Identity + EF |
| `Microsoft.AspNetCore.Identity.UI` | 8.0.24 | Scaffolded Razor Pages |
| `Microsoft.AspNetCore.Authentication.Google` | 8.0.24 | Google OAuth |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.0.11 | PostgreSQL EF provider |
| `Microsoft.EntityFrameworkCore.Tools` | 8.0.24 | `dotnet ef` CLI |
| `ClosedXML` | 0.105.0 | Excel `.xlsx` export |
| `QRCoder` | 1.7.0 | 2FA QR code generation |

---

## 📈 Repository Stats

<div align="center">

<img src="https://github-readme-stats.vercel.app/api/pin/?username=MacieiraPT&repo=finsight&theme=tokyonight&hide_border=true" alt="FinSight repo card" />

<br/><br/>

<img src="https://github-readme-stats.vercel.app/api/top-langs/?username=MacieiraPT&layout=compact&theme=tokyonight&hide_border=true&repo=finsight" alt="Top languages" />

</div>

---

## 🤝 Contributing

Contributions, issues, and feature requests are welcome! Feel free to open an [issue](https://github.com/MacieiraPT/finsight/issues) or submit a pull request.

1. 🍴 Fork the project
2. 🌿 Create your feature branch (`git checkout -b feature/amazing-feature`)
3. 💾 Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. 📤 Push to the branch (`git push origin feature/amazing-feature`)
5. 🔀 Open a Pull Request

---

## 📄 License

Distributed under the **MIT License**. See [`LICENSE.txt`](./LICENSE.txt) for details.

<div align="center">

<img src="https://img.shields.io/badge/Made_with-❤️_in_Portugal-006600?style=for-the-badge" alt="Made with love in Portugal" />
<img src="https://img.shields.io/badge/UFCD_5425-Bases_de_Dados-512BD4?style=for-the-badge" alt="UFCD 5425" />

<br/><br/>

<img src="https://capsule-render.vercel.app/api?type=waving&color=gradient&customColorList=12,20,24&height=120&section=footer" width="100%" alt="footer" />

<sub>⭐ If you find FinSight useful, consider starring the repo — it really helps!</sub>

</div>
