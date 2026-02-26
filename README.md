FinSight — Gestão de Despesas Pessoais

Projeto desenvolvido no âmbito do módulo UFCD 5425.

  --------------------------------------
  REQUISITOS
  --------------------------------------
  - .NET 8 SDK - PostgreSQL (instalado
  localmente) - Windows (para execução
  dos scripts .bat)
  --------------------------------------
  UTILIZADOR DE TESTE
  --------------------------------------
  Email: test@finsight.pt 
  Password: Test123!

  --------------------------------------

BASE DE DADOS

Nome da BD: DB_FinSight 
Motor: PostgreSQL (local)

A base de dados será criada automaticamente ao executar o instalar.bat.

  --------------------------------------
  PORTA DA APLICAÇÃO
  --------------------------------------
  https://localhost:7093

  --------------------------------------

EXECUÇÃO

1) Instalação

Executar: instalar.bat

Este script: - Instala as dependências do projeto - Cria a base de
dados - Aplica as migrations - Insere dados de teste (categorias,
despesas e orçamentos)

2) Iniciar aplicação

Executar: iniciar.bat

Este script: - Inicia o servidor backend - Abre automaticamente o
browser em: https://localhost:7093

3) Parar aplicação

Executar: parar.bat

Este script: - Encerra todos os processos do backend

  --------------------------------------
  FUNCIONALIDADES
  --------------------------------------
  - Registo e Login - Login com Google
  (OAuth 2.0) - Gestão de Categorias -
  Registo de Despesas - Gestão de
  Orçamentos Mensais - Dashboard com
  gráficos (Chart.js) - Alertas de
  limite mensal - Exportação de
  Despesas: - CSV - Excel (.xlsx) - Tema
  claro/escuro

  --------------------------------------

NOTAS

Caso a base de dados já exista e seja necessário recriar: - Apagar a BD
DB_FinSight no pgAdmin - Executar novamente o instalar.bat

------------------------------------------------------------------------
