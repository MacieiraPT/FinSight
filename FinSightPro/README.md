# FinSight Pro

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791.svg)](https://www.postgresql.org/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3.svg)](https://getbootstrap.com/)
[![Chart.js](https://img.shields.io/badge/Chart.js-4.4-FF6384.svg)](https://www.chartjs.org/)

> Aplicação web profissional de gestão de finanças pessoais — uma evolução completa do projeto **FinSight**, com arquitetura limpa em camadas, dashboard analítico, despesas recorrentes, anexos, orçamentos inteligentes e relatórios exportáveis.

---

## ✨ Visão Geral

O **FinSight Pro** ajuda-te a controlar receitas, despesas e orçamentos com clareza:

- 📊 **Dashboard rico** com saldo do mês, comparação MoM, taxa de poupança, projeção e top categorias.
- 💸 **Gestão de despesas** com filtros avançados, anexos (PDF/imagem) e despesas recorrentes (diário, semanal, mensal, anual).
- 💰 **Receitas** fixas e variáveis, recorrentes ou pontuais.
- 🧾 **Orçamentos** mensais por categoria com alertas a 75 % / 90 % / 100 %.
- 🏷️ **Categorias** personalizáveis com cor e ícone, com sub-categorias.
- 📈 **Relatórios** mensais comparativos com exportação em CSV e Excel formatado.
- 🌗 **Modo Claro/Escuro** com persistência em localStorage.
- 🔐 **Autenticação completa**: Email + Password, Google OAuth, 2FA, recuperação de password.
- 🇵🇹 Interface em **Português (pt-PT)**.

---

## 🖼️ Screenshots

> _Coloca capturas de ecrã da aplicação em execução aqui:_

```
docs/
├── dashboard.png
├── despesas.png
├── orcamentos.png
└── relatorios.png
```

---

## 🧱 Stack Tecnológico

| Camada | Tecnologia |
|---|---|
| Backend | ASP.NET Core MVC (.NET 8) |
| Arquitetura | Layered (Domain · Application · Infrastructure · Web · Tests) |
| ORM | Entity Framework Core 8 + Migrations |
| Base de Dados | PostgreSQL 14+ |
| Autenticação | ASP.NET Core Identity + Google OAuth |
| Frontend | Bootstrap 5.3 + Chart.js 4.4 + CSS variables |
| Validação | DataAnnotations + FluentValidation |
| Logs | Serilog (Console + File) |
| Exportação | ClosedXML (Excel) + CsvHelper (CSV) |
| Testes | xUnit + Moq + EF InMemory |

---

## 📁 Estrutura do Projeto

```
FinSightPro/
├── FinSightPro.sln
├── FinSightPro.Domain/          # Entidades e enums (sem dependências)
│   ├── Entities/                # ApplicationUser, Expense, Income, Category, Budget
│   └── Enums/                   # RecurrenceType, PaymentMethod
├── FinSightPro.Application/     # Lógica de negócio e contratos
│   ├── Interfaces/              # IExpenseService, ICategoryService, IRepository...
│   ├── Services/                # CategoryService, ExpenseService, DashboardService...
│   ├── DTOs/                    # ExpenseDto, BudgetDto, DashboardDto...
│   ├── Validators/              # FluentValidation rules
│   └── Common/                  # Result<T>, PagedResult<T>
├── FinSightPro.Infrastructure/  # EF Core + Repositórios + Seeder
│   ├── Data/                    # ApplicationDbContext, UnitOfWork
│   ├── Repositories/            # ExpenseRepository, CategoryRepository...
│   ├── Migrations/              # EF migrations (geradas)
│   └── Seed/                    # DatabaseSeeder
├── FinSightPro.Web/             # MVC + Razor Pages (Identity)
│   ├── Controllers/             # Dashboard, Despesas, Receitas, Categorias, Orçamentos, Relatórios
│   ├── ViewModels/              # Form/list view-models
│   ├── Views/                   # .cshtml organizados por controller
│   ├── Middleware/              # GlobalExceptionMiddleware
│   ├── Areas/Identity/          # Pages de autenticação (gerados pelo AddDefaultUI)
│   └── wwwroot/                 # CSS, JS, uploads
├── FinSightPro.Tests/           # xUnit + Moq
│   └── Services/                # Testes unitários por serviço
└── instalar.sh / .bat           # Scripts de setup
    iniciar.sh / .bat
    parar.sh / .bat
```

---

## 🚀 Instalação

### Pré-requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/) com utilizador `postgres` e ferramenta `psql` no PATH
- (Opcional) Credenciais Google OAuth para login social

### Passos rápidos

#### Windows

```bat
cd FinSightPro
copy FinSightPro.Web\appsettings.Example.json FinSightPro.Web\appsettings.json
instalar.bat
iniciar.bat
```

#### Linux/macOS

```bash
cd FinSightPro
cp FinSightPro.Web/appsettings.Example.json FinSightPro.Web/appsettings.json
./instalar.sh
./iniciar.sh
```

A aplicação fica disponível em **https://localhost:7080**.

### Setup manual (alternativa)

```bash
# 1. Restaurar dotnet-ef e pacotes
dotnet tool restore
dotnet restore FinSightPro.sln

# 2. Criar base de dados
psql -U postgres -c "CREATE USER finsightpro WITH PASSWORD 'finsightpro123';"
psql -U postgres -c "CREATE DATABASE \"DB_FinSightPro\" OWNER finsightpro;"

# 3. Configurar
cp FinSightPro.Web/appsettings.Example.json FinSightPro.Web/appsettings.json
# editar a connection string e (opcional) credenciais Google

# 4. Aplicar migrações + arrancar
dotnet ef database update --project FinSightPro.Infrastructure --startup-project FinSightPro.Web
dotnet run --project FinSightPro.Web
```

### Migrações (EF Core)

```bash
# Criar nova migração
dotnet ef migrations add NomeDaMigracao --project FinSightPro.Infrastructure --startup-project FinSightPro.Web

# Aplicar
dotnet ef database update --project FinSightPro.Infrastructure --startup-project FinSightPro.Web
```

---

## 👤 Conta de Teste

Após o primeiro arranque, o seeder cria automaticamente:

- **Email:** `demo@finsightpro.pt`
- **Password:** `Demo123!`
- 10 categorias pré-definidas com ícones e cores
- Despesas e receitas dos últimos 3 meses
- 5 orçamentos para o mês corrente

---

## 🧪 Testes

```bash
dotnet test FinSightPro.Tests
```

Os testes cobrem:

- Validação de regras de negócio (`CategoryService`, `ExpenseService`)
- Cálculos de DTOs (`BudgetDto.PercentUsed`, `Remaining`)
- Lógica de despesas recorrentes (`RecurringTransactionService`)

---

## 🔐 Configuração

`appsettings.json` é ignorado pelo Git. Copia o template:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=DB_FinSightPro;Username=finsightpro;Password=finsightpro123"
  },
  "SeedAdmin": {
    "Email": "demo@finsightpro.pt",
    "Password": "Demo123!",
    "Name": "Demo FinSight"
  },
  "Auth": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    }
  }
}
```

---

## 🧠 Convenções e Boas Práticas

- ✅ **Arquitetura em camadas** — `Domain` não depende de nada; `Application` depende só de `Domain`; `Infrastructure` implementa contratos de `Application`.
- ✅ **Lógica nos Services** — Controllers só orquestram entradas/saídas.
- ✅ **ViewModels separados** das entidades de domínio.
- ✅ **Isolamento por utilizador** — Toda a query filtra por `UserId`.
- ✅ **Datas em UTC** — Conversor automático no `ApplicationDbContext`.
- ✅ **Idioma pt-PT** — UI, validações e mensagens em português.
- ✅ **Result<T>** para devolver sucesso/erro sem lançar exceções.
- ✅ **Logs estruturados** com Serilog (consola e ficheiro `logs/finsightpro-.log`).
- ✅ **Tratamento de erros global** via `GlobalExceptionMiddleware` + página `/Home/Error`.
- ✅ **Anexos** — Validados por extensão e tamanho (5 MB máx) e guardados em `wwwroot/uploads/{userId}/`.

---

## 📦 Funcionalidades

| Área | Funcionalidade |
|---|---|
| Auth | Email/Password, Google OAuth, 2FA, recuperação de password, perfil editável |
| Despesas | CRUD, filtros (data, categoria, método, valor, texto), recorrentes, anexos, exportação |
| Receitas | CRUD, fixas vs variáveis, recorrentes |
| Categorias | CRUD, sub-categorias, ícone + cor, defaults seedáveis |
| Orçamentos | CRUD por categoria/mês, alertas 75%/90%/100%, histórico mensal |
| Dashboard | Saldo, MoM%, Top 5, gráficos barras/donut/linha, projeção, alertas inteligentes |
| Relatórios | Mensal detalhado, comparação entre meses, projeção, exportação CSV/Excel formatado |
| UI | Modo claro/escuro com `localStorage`, design responsivo mobile-first, toasts, breadcrumbs, skeleton loaders |

---

## 🤝 Como Contribuir

1. Faz fork do repositório
2. Cria uma branch: `git checkout -b feat/minha-feature`
3. Commits descritivos: `git commit -m "feat: adiciona X"`
4. Abre Pull Request descrevendo o que mudou e porquê
5. Adiciona testes para novas funcionalidades sempre que possível

### Estilo

- Segue as convenções do .NET
- Mantém a UI em **pt-PT**
- Adiciona testes unitários no `FinSightPro.Tests`

---

## 📜 Licença

MIT — vê [LICENSE.txt](../LICENSE.txt) para detalhes.

---

## 🙏 Créditos

Inspirado no projeto [FinSight](https://github.com/MacieiraPT/FinSight). Construído com ❤️ em Portugal.
