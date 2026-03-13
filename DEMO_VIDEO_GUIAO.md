# FinSight — Guião de Vídeo de Demonstração

**Duração estimada:** 5–8 minutos
**Data:** Março de 2026
**Idioma:** Português (pt-PT)

---

## Introdução (0:00 – 0:30)

**[Ecrã: Landing page — `/`]**

> "Olá! Hoje vou mostrar o progresso atual do **FinSight**, uma aplicação web de gestão de finanças pessoais desenvolvida com ASP.NET Core 8, PostgreSQL e Bootstrap 5."
>
> "O objetivo do FinSight é simples: ajudar qualquer pessoa a controlar as suas despesas, gerir os seus orçamentos e visualizar os seus hábitos financeiros de forma clara e intuitiva — tudo em Português."

---

## 1. Autenticação (0:30 – 1:15)

**[Ecrã: Página de Login — `/Identity/Account/Login`]**

> "Começamos pelo sistema de autenticação. O utilizador pode entrar com **e-mail e palavra-passe**, ou através do **Google OAuth** com um clique."

- Mostrar o formulário de login
- Clicar em "Entrar com Google" (demonstrar o botão)

> "Existe também suporte completo para **Autenticação de Dois Fatores (2FA)** com aplicações autenticadoras como o Google Authenticator."

**[Ecrã: Entrar com a conta de teste `test@finsight.pt` / `Test123!`]**

> "Vou entrar com a conta de demonstração que já tem dados de teste carregados."

---

## 2. Dashboard Principal (1:15 – 2:15)

**[Ecrã: Dashboard — `/Dashboard`]**

> "Após o login, chegamos ao **painel principal**. Aqui temos uma visão geral das finanças do mês atual."

Destacar, por ordem:

1. **Cartões de resumo** — Receitas totais, Despesas totais e Saldo do mês
2. **Alertas de orçamento** — Aviso a laranja quando o orçamento está acima de 80%, vermelho quando ultrapassa 100%
3. **Gráfico de tendências** — Linha dos últimos 6 meses a comparar receitas vs. despesas (Chart.js)
4. **Gráfico de distribuição** — Doughnut com as categorias de despesa do mês
5. **Progresso dos orçamentos** — Barras de progresso por categoria

> "Todos os dados são calculados em tempo real e estão isolados por utilizador — não existe forma de ver os dados de outra pessoa."

---

## 3. Gestão de Despesas (2:15 – 3:15)

**[Ecrã: Lista de Despesas — `/Despesas`]**

> "Na secção de **Despesas**, podemos ver todas as transações registadas com múltiplas opções de pesquisa e ordenação."

- Demonstrar o **filtro por categoria, ano e mês**
- Mostrar a **pesquisa por descrição**
- Mostrar a **ordenação** por data, valor ou categoria

**[Criar uma nova despesa — `/Despesas/Create`]**

> "Criar uma despesa é rápido: definimos a descrição, o valor, a categoria e a data. Datas futuras são automaticamente rejeitadas pelo sistema."

- Preencher o formulário e submeter

**[Funcionalidades avançadas]**

> "Existem também funcionalidades mais avançadas:"

- Abrir **Duplicados** — `/Despesas/Duplicados` — *"Deteção automática de despesas duplicadas"*
- Mostrar checkboxes de **seleção múltipla** para eliminação em lote
- Clicar em **Exportar CSV** e **Exportar Excel** — *"Exportação com os filtros ativos aplicados"*

---

## 4. Gestão de Receitas (3:15 – 3:45)

**[Ecrã: Lista de Receitas — `/Receitas`]**

> "Do lado das **Receitas**, o funcionamento é semelhante. Temos 6 tipos pré-definidos: Salário, Freelance, Bónus, Vendas, Investimentos e Outros."

- Mostrar a lista de receitas com filtros
- Abrir criar receita para mostrar os tipos disponíveis
- Mencionar exportação CSV/Excel

---

## 5. Categorias e Orçamentos (3:45 – 4:30)

**[Ecrã: Categorias — `/Categorias`]**

> "As **Categorias** são totalmente personalizáveis por utilizador. Existe um atalho para adicionar 10 categorias padrão em Português de uma só vez."

- Mostrar a lista de categorias
- Clicar em **"Adicionar Categorias Padrão"**

**[Ecrã: Orçamentos — `/Orcamentos`]**

> "Os **Orçamentos** definem um limite mensal para cada categoria. Podemos criar orçamentos para o mês atual ou **copiar automaticamente os orçamentos do mês anterior** — uma funcionalidade muito prática."

- Mostrar a lista de orçamentos com o filtro de ano/mês
- Demonstrar o botão **"Copiar Mês Anterior"**

---

## 6. Despesas Recorrentes (4:30 – 5:00)

**[Ecrã: Despesas Recorrentes — `/DespesasRecorrentes`]**

> "Para despesas que se repetem, existe a secção de **Despesas Recorrentes**. Podemos configurar a frequência — semanal, mensal ou anual — e o sistema processa-as automaticamente quando a aplicação arranca."

- Mostrar uma despesa recorrente existente (ex: renda ou subscrição)
- Destacar os campos de data de início, data de fim e estado ativo

---

## 7. Relatórios e Pesquisa Global (5:00 – 5:45)

**[Ecrã: Relatórios — `/Relatorios`]**

> "A secção de **Relatórios** oferece análises mais aprofundadas. Podemos filtrar por intervalo de datas e ver:"

- **Distribuição por categoria** com valores e percentagens
- **Tendências mensais** de despesa
- **Média dos últimos 3 meses** por categoria com variação
- **Poupança acumulada** ao longo do tempo

**[Ecrã: Pesquisa Global — `/Pesquisa`]**

> "A **Pesquisa Global** permite encontrar qualquer transação ou categoria em simultâneo — despesas, receitas e categorias aparecem nos resultados de uma só vez."

---

## 8. Auditoria e Definições (5:45 – 6:15)

**[Ecrã: Auditoria — `/Auditoria`]**

> "Todas as ações do utilizador ficam registadas no **Registo de Auditoria**: criação, edição e eliminação de qualquer entidade, com data e hora exatas."

**[Ecrã: Definições Financeiras — `/Identity/Account/Manage/FinancialSettings`]**

> "Nas **Definições Financeiras**, o utilizador configura o seu salário mensal, a percentagem máxima a gastar em despesas e se quer receber alertas quando os limites são atingidos. Estes valores alimentam os alertas no dashboard."

---

## 9. Segurança e Qualidade Técnica (6:15 – 6:45)

> "Do ponto de vista técnico, o FinSight foi construído com várias camadas de segurança:"

- **Isolamento de dados por utilizador** — todas as queries filtram por `UserId`
- **Headers de segurança HTTP** — X-Frame-Options, X-XSS-Protection, Content-Type-Options, Permissions-Policy
- **Validação de datas** — datas futuras são rejeitadas
- **Proteção contra eliminação** — categorias com despesas associadas não podem ser apagadas
- **Conversão UTC** — todas as datas são guardadas em UTC
- **Exportações seguras** — CSV com sanitização para prevenir injeção de fórmulas

---

## Conclusão (6:45 – 7:15)

> "O **FinSight** já cobre as funcionalidades essenciais de uma aplicação de finanças pessoais moderna:"

| Funcionalidade | Estado |
|---|---|
| Autenticação (e-mail, Google, 2FA) | ✅ Concluído |
| Dashboard com gráficos interativos | ✅ Concluído |
| Gestão de Despesas (CRUD + filtros + exportação) | ✅ Concluído |
| Gestão de Receitas (CRUD + filtros + exportação) | ✅ Concluído |
| Categorias personalizadas + padrão | ✅ Concluído |
| Orçamentos mensais por categoria | ✅ Concluído |
| Despesas Recorrentes | ✅ Concluído |
| Relatórios e tendências | ✅ Concluído |
| Pesquisa global | ✅ Concluído |
| Registo de auditoria | ✅ Concluído |
| Definições financeiras do utilizador | ✅ Concluído |
| Exportação CSV e Excel | ✅ Concluído |
| Deteção de duplicados | ✅ Concluído |

> "Obrigado por acompanhar esta demonstração do FinSight!"

---

## Notas de Produção

- **Conta de demonstração:** `test@finsight.pt` / `Test123!`
- **URL local:** `https://localhost:7093`
- **Resolução recomendada:** 1920×1080 ou 1280×720
- **Navegador recomendado:** Chrome ou Edge (para melhor renderização dos gráficos)
- Fechar notificações do sistema antes de gravar
- Limpar histórico de pesquisa do browser para não aparecerem sugestões indesejadas
