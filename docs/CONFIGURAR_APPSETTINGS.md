# Tutorial — Como Configurar o `appsettings.json`

Este guia explica, passo a passo e em português (pt-PT), como criar e preencher o ficheiro `appsettings.json` necessário para correr o **FinSight** no teu computador.

> ℹ️ O `appsettings.json` **não está no repositório** porque contém dados sensíveis (palavras-passe da base de dados, segredos do Google). Está incluído no `.gitignore`. Cada programador tem de criar o seu localmente a partir do ficheiro de exemplo.

---

## 📍 Onde Fica o Ficheiro?

O ficheiro tem de ser colocado em:

```
backend/GestaoDespesas/GestaoDespesas/appsettings.json
```

Ou seja, **na mesma pasta** onde está o `Program.cs` e o `appsettings.Example.json`.

---

## 🪜 Passo 1 — Copiar o Ficheiro de Exemplo

Abre um terminal na raiz do projeto e executa:

### Windows (CMD)
```bat
copy backend\GestaoDespesas\GestaoDespesas\appsettings.Example.json backend\GestaoDespesas\GestaoDespesas\appsettings.json
```

### Windows (PowerShell)
```powershell
Copy-Item backend\GestaoDespesas\GestaoDespesas\appsettings.Example.json backend\GestaoDespesas\GestaoDespesas\appsettings.json
```

### Linux / macOS
```bash
cp backend/GestaoDespesas/GestaoDespesas/appsettings.Example.json backend/GestaoDespesas/GestaoDespesas/appsettings.json
```

Agora abre o `appsettings.json` num editor de texto (Visual Studio, VS Code, Notepad++, etc.).

---

## 🧩 Passo 2 — Entender a Estrutura

O ficheiro tem **quatro secções** principais:

```json
{
  "ConnectionStrings": { ... },   // Ligação à base de dados PostgreSQL
  "SeedAdmin":          { ... },  // Conta de teste criada na primeira execução
  "Auth":               { ... },  // Login com Google (opcional)
  "Logging":            { ... },  // Já vem preenchido — não mexer
  "AllowedHosts":       "*"       // Já vem preenchido — não mexer
}
```

Vamos preencher uma de cada vez.

---

## 🐘 Passo 3 — `ConnectionStrings` (PostgreSQL)

Esta é a secção **mais importante**. Sem ela, a aplicação não arranca.

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=DB_FinSight;Username=finsight;Password=finsight123"
}
```

### Significado de cada campo

| Campo | O que significa | Valor típico |
|---|---|---|
| `Host` | Endereço do servidor PostgreSQL | `localhost` (se estiver na tua máquina) |
| `Port` | Porta do PostgreSQL | `5432` (porta por defeito) |
| `Database` | Nome da base de dados | `DB_FinSight` |
| `Username` | Utilizador PostgreSQL | `finsight` |
| `Password` | Palavra-passe desse utilizador | a que definiste ao criar o user |

### Como obter estes valores?

Se ainda não criaste a base de dados, executa estes comandos:

```bash
psql -U postgres -c "CREATE USER finsight WITH PASSWORD 'finsight123';"
psql -U postgres -c "CREATE DATABASE \"DB_FinSight\" OWNER finsight;"
```

> 💡 **No Windows**, podes simplesmente correr `instalar.bat`, que faz tudo isto automaticamente.

Se preferires usar **outro utilizador** ou **outra palavra-passe**, basta refletir essa escolha na connection string.

---

## 👤 Passo 4 — `SeedAdmin` (Conta de Teste)

Quando a aplicação arranca pela primeira vez, cria automaticamente uma conta de teste com dados de exemplo (5 categorias, 5 orçamentos, 40 despesas).

```json
"SeedAdmin": {
  "Email": "test@finsight.pt",
  "Password": "Test123!"
}
```

### O que preencher?

- **Email** — qualquer email válido. Será o email com que fazes login.
- **Password** — tem de cumprir as regras do ASP.NET Core Identity:
  - Mínimo **6 caracteres**
  - Pelo menos **1 maiúscula**
  - Pelo menos **1 minúscula**
  - Pelo menos **1 dígito**
  - Pelo menos **1 caractere não-alfanumérico** (`!`, `@`, `#`, etc.)

> 💡 Podes manter os valores por defeito (`test@finsight.pt` / `Test123!`) — só servem para a conta de demonstração.

---

## 🔐 Passo 5 — `Auth.Google` (Opcional)

Esta secção é **opcional**. Só é necessária se quiseres permitir **login com Google**. Se não te interessa, podes deixar os valores de exemplo — o login por email/password continua a funcionar.

```json
"Auth": {
  "Google": {
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET"
  }
}
```

### Como obter credenciais Google OAuth?

1. Vai a [Google Cloud Console](https://console.cloud.google.com/).
2. Cria (ou seleciona) um **projeto**.
3. Menu lateral → **APIs & Services** → **Credentials**.
4. Clica em **Create Credentials** → **OAuth client ID**.
5. Tipo de aplicação: **Web application**.
6. Em **Authorized redirect URIs** adiciona:
   ```
   https://localhost:7093/signin-google
   http://localhost:5067/signin-google
   ```
7. Clica em **Create**. Aparecem dois valores:
   - **Client ID** → copia para `ClientId`
   - **Client Secret** → copia para `ClientSecret`

> ⚠️ Estes valores são **secretos**. Nunca os partilhes nem os faças commit para o repositório.

---

## ✅ Passo 6 — Exemplo Completo Preenchido

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
      "ClientId": "123456789-abc.apps.googleusercontent.com",
      "ClientSecret": "GOCSPX-aBcDeFgHiJkLmNoPqRsT"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🧪 Passo 7 — Testar

Na pasta `backend/GestaoDespesas/`:

```bash
dotnet ef database update --project GestaoDespesas
dotnet run --project GestaoDespesas
```

Se tudo estiver bem configurado:

- Abre `https://localhost:7093` no browser.
- Faz login com `test@finsight.pt` / `Test123!`.
- Deves ver o dashboard com as despesas de exemplo.

---

## 🛟 Resolução de Problemas

| Erro | Causa provável | Solução |
|---|---|---|
| `Npgsql.PostgresException: 28P01: password authentication failed` | Password errada na connection string | Confirma o campo `Password` |
| `Npgsql.PostgresException: 3D000: database "DB_FinSight" does not exist` | A BD ainda não foi criada | Executa o passo 3 ou `instalar.bat` |
| `Connection refused` / `No connection could be made` | PostgreSQL não está a correr | Inicia o serviço PostgreSQL |
| `IdentityErrorDescriber: Passwords must have at least one non alphanumeric character` | A `SeedAdmin.Password` é fraca | Usa uma password mais forte (ver Passo 4) |
| App arranca mas login com Google falha | `ClientId` / `ClientSecret` errados ou redirect URI mal configurado | Revê o Passo 5 |
| `appsettings.json not found` | O ficheiro não está na pasta correta | Confirma o caminho (ver topo deste tutorial) |

---

## 🔒 Segurança — Boas Práticas

1. **Nunca** faças `git add appsettings.json` — já está protegido pelo `.gitignore`, mas confirma sempre com `git status` antes de cada commit.
2. **Não partilhes** o teu ficheiro `appsettings.json` por email/Slack/Discord — contém credenciais.
3. Em **produção**, prefere usar **variáveis de ambiente** ou um **secret manager** (Azure Key Vault, AWS Secrets Manager) em vez de gravar segredos em ficheiro.
4. Usa **passwords diferentes** para desenvolvimento e produção.

---

## 📚 Ver Também

- [`appsettings.Example.json`](../backend/GestaoDespesas/GestaoDespesas/appsettings.Example.json) — o template oficial
- [`CLAUDE.md`](../CLAUDE.md) — guia geral do código-fonte
- [`README.md`](../README.md) — visão geral do projeto

---

> ✨ Em caso de dúvida, abre uma [issue no GitHub](https://github.com/MacieiraPT/finsight/issues).
