# 💰 FinSight – Sistema de Gestão de Despesas

Projeto desenvolvido no âmbito do módulo **UFCD 5425 – Bases de Dados**.

A aplicação FinSight permite ao utilizador registar, organizar e analisar as suas despesas mensais, definir orçamentos por categoria e receber alertas quando se aproxima dos limites definidos com base no seu salário.

---

## 🚀 Funcionalidades

- Registo e autenticação de utilizadores (Email / Google)
- Gestão de despesas pessoais
- Gestão de categorias
- Definição de orçamentos mensais
- Dashboard com:
  - Total mensal gasto
  - Alertas financeiros
  - Distribuição por categoria
  - Evolução dos últimos 6 meses
- Exportação de dados (CSV e Excel)
- Alertas automáticos baseados no salário
- Interface com modo claro / escuro

---

## 🧰 Tecnologias Utilizadas

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- PostgreSQL
- Bootstrap 5
- Chart.js
- ClosedXML

---

## ⚙️ Requisitos

Antes de executar a aplicação, é necessário ter instalado:

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- PostgreSQL

---

## ▶️ Instalação e Execução

### 1️⃣ Instalar dependências e criar base de dados

Executar: instalar.bat

Este script:

- Instala as dependências do projeto
- Cria a base de dados local
- Aplica as migrações necessárias

---

### 2️⃣ Iniciar a aplicação

Executar: iniciar.bat

Este script:

- Inicia o servidor de base de dados
- Inicia o backend da aplicação
- Abre automaticamente a aplicação no browser

---

### 3️⃣ Parar a aplicação

Executar: parar.bat

Este script:

- Encerra todos os processos relacionados com o projeto

---

## 👤 Utilizador de Teste

Após a instalação, será criado automaticamente o seguinte utilizador:

- Email: test@finsight.pt
- Password: Test123!

Este utilizador contém dados de exemplo para:

- Despesas
- Categorias
- Orçamentos

---

## 📊 Exportação de Dados

A aplicação permite exportar as despesas filtradas para:

- CSV
- Excel (.xlsx)

---

## 📌 Notas

- A aplicação utiliza PostgreSQL localmente.
- A porta utilizada por defeito é: https://localhost:7093
