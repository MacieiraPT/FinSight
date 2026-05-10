"""
Gera o "Relatorio_FinSight.pdf" com a estrutura exigida pela UFCD 5425.

Formato:
- Times New Roman, 12pt
- Espaçamento 1,5 linhas
- Margens A4
- Capa, Índice, Secções obrigatórias, Anexos
"""

from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import cm, mm
from reportlab.lib.enums import TA_JUSTIFY, TA_CENTER, TA_LEFT
from reportlab.lib import colors
from reportlab.platypus import (
    BaseDocTemplate, PageTemplate, Frame, Paragraph, Spacer,
    PageBreak, Table, TableStyle, Image, KeepTogether
)
from reportlab.platypus.tableofcontents import TableOfContents
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
import os

# ---------------------------------------------------------------------------
# Fontes — usar Times-Roman (PDF base font, equivalente a Times New Roman)
# ---------------------------------------------------------------------------
BASE_FONT = "Times-Roman"
BASE_FONT_BOLD = "Times-Bold"
BASE_FONT_ITALIC = "Times-Italic"

FONT_SIZE = 12
LEADING = FONT_SIZE * 1.5  # espaçamento 1,5 linhas

# ---------------------------------------------------------------------------
# Estilos
# ---------------------------------------------------------------------------
def make_styles():
    body = ParagraphStyle(
        name="Body",
        fontName=BASE_FONT,
        fontSize=FONT_SIZE,
        leading=LEADING,
        alignment=TA_JUSTIFY,
        spaceAfter=6,
        firstLineIndent=0,
    )
    h1 = ParagraphStyle(
        name="H1",
        fontName=BASE_FONT_BOLD,
        fontSize=18,
        leading=22,
        spaceBefore=18,
        spaceAfter=12,
        textColor=colors.HexColor("#0a3d62"),
    )
    h2 = ParagraphStyle(
        name="H2",
        fontName=BASE_FONT_BOLD,
        fontSize=14,
        leading=18,
        spaceBefore=12,
        spaceAfter=8,
        textColor=colors.HexColor("#1e3799"),
    )
    h3 = ParagraphStyle(
        name="H3",
        fontName=BASE_FONT_BOLD,
        fontSize=12,
        leading=16,
        spaceBefore=8,
        spaceAfter=4,
        textColor=colors.HexColor("#34495e"),
    )
    cover_title = ParagraphStyle(
        name="CoverTitle",
        fontName=BASE_FONT_BOLD,
        fontSize=36,
        leading=42,
        alignment=TA_CENTER,
        textColor=colors.HexColor("#0a3d62"),
        spaceAfter=18,
    )
    cover_subtitle = ParagraphStyle(
        name="CoverSubtitle",
        fontName=BASE_FONT_ITALIC,
        fontSize=18,
        leading=24,
        alignment=TA_CENTER,
        textColor=colors.HexColor("#34495e"),
        spaceAfter=24,
    )
    cover_meta = ParagraphStyle(
        name="CoverMeta",
        fontName=BASE_FONT,
        fontSize=14,
        leading=20,
        alignment=TA_CENTER,
        spaceAfter=6,
    )
    bullet = ParagraphStyle(
        name="Bullet",
        parent=body,
        leftIndent=18,
        bulletIndent=4,
        spaceAfter=2,
    )
    code = ParagraphStyle(
        name="Code",
        fontName="Courier",
        fontSize=9,
        leading=12,
        leftIndent=12,
        rightIndent=12,
        spaceBefore=4,
        spaceAfter=8,
        backColor=colors.HexColor("#f4f6f8"),
        borderPadding=6,
    )
    caption = ParagraphStyle(
        name="Caption",
        fontName=BASE_FONT_ITALIC,
        fontSize=10,
        leading=12,
        alignment=TA_CENTER,
        textColor=colors.HexColor("#555555"),
        spaceAfter=10,
    )
    return {
        "body": body, "h1": h1, "h2": h2, "h3": h3,
        "cover_title": cover_title, "cover_subtitle": cover_subtitle,
        "cover_meta": cover_meta, "bullet": bullet, "code": code,
        "caption": caption,
    }


# ---------------------------------------------------------------------------
# Doc template com numeração de página e cabeçalhos para o índice
# ---------------------------------------------------------------------------
class FinSightDocTemplate(BaseDocTemplate):
    def __init__(self, filename, **kw):
        BaseDocTemplate.__init__(self, filename, **kw)

    def afterFlowable(self, flowable):
        """Regista entradas no Índice para títulos H1/H2."""
        if isinstance(flowable, Paragraph):
            style_name = flowable.style.name
            text = flowable.getPlainText()
            if style_name == "H1":
                self.notify("TOCEntry", (0, text, self.page))
            elif style_name == "H2":
                self.notify("TOCEntry", (1, text, self.page))


def header_footer(canvas_obj, doc):
    """Rodapé com número de página (omite na capa)."""
    canvas_obj.saveState()
    if doc.page > 1:
        canvas_obj.setFont(BASE_FONT, 9)
        canvas_obj.setFillColor(colors.HexColor("#777777"))
        canvas_obj.drawString(2 * cm, 1.2 * cm, "FinSight — Relatório de Projeto · UFCD 5425")
        canvas_obj.drawRightString(A4[0] - 2 * cm, 1.2 * cm, f"Página {doc.page - 1}")
        canvas_obj.setStrokeColor(colors.HexColor("#cccccc"))
        canvas_obj.setLineWidth(0.5)
        canvas_obj.line(2 * cm, 1.5 * cm, A4[0] - 2 * cm, 1.5 * cm)
    canvas_obj.restoreState()


def cover_page(canvas_obj, doc):
    """Decoração da capa: barra colorida no topo."""
    canvas_obj.saveState()
    canvas_obj.setFillColor(colors.HexColor("#0a3d62"))
    canvas_obj.rect(0, A4[1] - 1.5 * cm, A4[0], 1.5 * cm, stroke=0, fill=1)
    canvas_obj.setFillColor(colors.HexColor("#2eb872"))
    canvas_obj.rect(0, 0, A4[0], 1.0 * cm, stroke=0, fill=1)
    canvas_obj.restoreState()


# ---------------------------------------------------------------------------
# Helpers de conteúdo
# ---------------------------------------------------------------------------
def P(text, style):
    return Paragraph(text, style)


def bullets(items, styles):
    return [Paragraph(f"• {it}", styles["bullet"]) for it in items]


# ---------------------------------------------------------------------------
# Construção do documento
# ---------------------------------------------------------------------------
def build(output_path):
    doc = FinSightDocTemplate(
        output_path,
        pagesize=A4,
        leftMargin=2.5 * cm,
        rightMargin=2.5 * cm,
        topMargin=2.5 * cm,
        bottomMargin=2.5 * cm,
        title="FinSight — Relatório de Elaboração do Projeto",
        author="Aluno do curso (UFCD 5425)",
    )

    frame_cover = Frame(
        0, 0, A4[0], A4[1], leftPadding=2.5 * cm, rightPadding=2.5 * cm,
        topPadding=3.5 * cm, bottomPadding=2.5 * cm, id="cover"
    )
    frame_normal = Frame(
        doc.leftMargin, doc.bottomMargin,
        doc.width, doc.height, id="normal"
    )

    doc.addPageTemplates([
        PageTemplate(id="Cover", frames=[frame_cover], onPage=cover_page),
        PageTemplate(id="Normal", frames=[frame_normal], onPage=header_footer),
    ])

    s = make_styles()
    story = []

    # ====================================================================
    # CAPA
    # ====================================================================
    story.append(Spacer(1, 4 * cm))
    story.append(P("FinSight", s["cover_title"]))
    story.append(P("Gestão Inteligente de Finanças Pessoais", s["cover_subtitle"]))
    story.append(Spacer(1, 2 * cm))

    capa_box = Table(
        [
            ["Relatório de Elaboração do Projeto"],
            ["UFCD 5425 — Bases de Dados"],
            [""],
            ["Aluno: ____________________________________"],
            [""],
            ["Data: 10 de maio de 2026"],
        ],
        colWidths=[12 * cm],
    )
    capa_box.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (-1, -1), BASE_FONT),
        ("FONTSIZE", (0, 0), (-1, -1), 14),
        ("ALIGN", (0, 0), (-1, -1), "CENTER"),
        ("TEXTCOLOR", (0, 0), (-1, -1), colors.HexColor("#34495e")),
        ("FONTNAME", (0, 0), (0, 0), BASE_FONT_BOLD),
        ("FONTSIZE", (0, 0), (0, 0), 16),
        ("FONTNAME", (0, 1), (0, 1), BASE_FONT_BOLD),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 8),
    ]))
    story.append(capa_box)

    story.append(Spacer(1, 4 * cm))
    story.append(P(
        "Aplicação web para registo de despesas e receitas, gestão de orçamentos "
        "mensais e visualização de tendências de consumo.",
        s["cover_meta"]
    ))

    story.append(PageBreak())
    # A partir daqui usa template Normal
    story.append(NextPageTemplate := __import__("reportlab.platypus", fromlist=["NextPageTemplate"]).NextPageTemplate("Normal"))

    # ====================================================================
    # ÍNDICE
    # ====================================================================
    story.append(P("Índice", s["h1"]))
    toc = TableOfContents()
    toc.levelStyles = [
        ParagraphStyle(
            name="TOC1", fontName=BASE_FONT_BOLD, fontSize=12, leading=18,
            leftIndent=0, firstLineIndent=0, spaceBefore=4,
        ),
        ParagraphStyle(
            name="TOC2", fontName=BASE_FONT, fontSize=11, leading=15,
            leftIndent=18, firstLineIndent=0,
        ),
    ]
    story.append(toc)
    story.append(PageBreak())

    # ====================================================================
    # 1. INTRODUÇÃO
    # ====================================================================
    story.append(P("1. Introdução", s["h1"]))

    story.append(P("1.1 Contextualização do projeto", s["h2"]))
    story.append(P(
        "O <b>FinSight</b> é uma aplicação web de gestão de finanças pessoais, "
        "desenvolvida no âmbito da UFCD 5425 — Bases de Dados. O projeto resulta da "
        "necessidade de oferecer ao utilizador comum uma ferramenta simples, segura "
        "e em português europeu, capaz de centralizar o registo das suas despesas, "
        "permitir a definição de orçamentos mensais e produzir indicadores visuais "
        "que ajudem à tomada de decisão financeira.",
        s["body"]
    ))
    story.append(P(
        "A aplicação foi construída com <b>ASP.NET Core 8 MVC</b> sobre uma base de "
        "dados <b>PostgreSQL</b>, recorrendo ao Entity Framework Core para a "
        "persistência e ao ASP.NET Core Identity para a autenticação. A interface, "
        "totalmente em pt-PT, adota o Bootstrap 5 e o Chart.js para garantir uma "
        "experiência responsiva e um conjunto rico de visualizações.",
        s["body"]
    ))

    story.append(P("1.2 Objetivos", s["h2"]))
    story.extend(bullets([
        "Permitir o registo, edição, consulta e remoção de despesas e receitas.",
        "Organizar transações por categorias e subcategorias personalizáveis pelo utilizador.",
        "Definir limites mensais de orçamento por categoria e gerar alertas automáticos.",
        "Disponibilizar um dashboard com totais mensais, distribuição por categoria e evolução semestral.",
        "Implementar autenticação segura com email/palavra-passe, Google OAuth e 2FA.",
        "Garantir o isolamento estrito dos dados — cada utilizador só vê os seus.",
        "Permitir a exportação de despesas filtradas para CSV e Excel.",
        "Aplicar boas práticas de modelação relacional, integridade referencial e migrações versionadas.",
    ], s))

    story.append(P("1.3 Motivação para a escolha do tema", s["h2"]))
    story.append(P(
        "A literacia financeira é, em Portugal, um problema reconhecido. As "
        "ferramentas disponíveis no mercado são, em grande parte, estrangeiras, "
        "comerciais e pouco adequadas ao quotidiano nacional (categorias, moeda, "
        "linguagem). Este projeto procura demonstrar que, com tecnologias abertas e "
        "uma modelação cuidada, é possível construir uma alternativa <i>open source</i>, "
        "em pt-PT, focada em privacidade — não há partilha de dados com terceiros — e "
        "extensível academicamente para servir de caso de estudo da UFCD 5425.",
        s["body"]
    ))
    story.append(P(
        "Adicionalmente, o tema permite exercitar a totalidade das competências "
        "previstas no referencial: modelação E-R, normalização, definição e execução "
        "de scripts SQL, integração com uma linguagem de programação e desenho de "
        "uma camada de acesso a dados consistente.",
        s["body"]
    ))

    # ====================================================================
    # 2. DESCRIÇÃO DO SISTEMA
    # ====================================================================
    story.append(P("2. Descrição do Sistema", s["h1"]))

    story.append(P("2.1 Funcionalidades implementadas", s["h2"]))
    story.extend(bullets([
        "<b>Autenticação e gestão de conta:</b> registo, login, recuperação de palavra-passe, "
        "Google OAuth, 2FA via TOTP (QR Code com QRCoder) e gestão de definições financeiras "
        "(salário mensal, percentagem-limite, ativação de alertas).",
        "<b>Despesas:</b> CRUD completo com listagem filtrável (categoria, ano, mês, texto livre), "
        "ordenação por colunas e paginação (10 por página). Validação que rejeita datas futuras.",
        "<b>Receitas:</b> CRUD completo com tipologia configurável (salário, prémio, juros, etc.).",
        "<b>Despesas e receitas recorrentes:</b> registo de movimentos automáticos com frequência "
        "semanal, mensal ou anual e gestão da última geração.",
        "<b>Categorias:</b> CRUD com validação de unicidade por utilizador, suporte a categoria-pai "
        "(hierarquia), ícone e cor personalizáveis, e ação <i>Seed</i> que popula 10 categorias por defeito.",
        "<b>Orçamentos:</b> definição de limites mensais por categoria, com barras de progresso e "
        "alertas no dashboard.",
        "<b>Dashboard:</b> totais do mês, distribuição por categoria (gráfico circular), evolução "
        "dos últimos 6 meses (gráfico de linhas/barras), comparação com o salário e indicadores de "
        "estado dos orçamentos.",
        "<b>Pesquisa global e relatórios:</b> pesquisa por descrição em despesas e relatórios "
        "comparativos por intervalo temporal e mês homólogo.",
        "<b>Auditoria:</b> registo das ações de criação, edição e eliminação por utilizador, com "
        "consulta paginada.",
        "<b>Exportações:</b> despesas filtradas exportadas para CSV (StringBuilder) e Excel "
        "(.xlsx via ClosedXML), respeitando os filtros ativos.",
    ], s))

    story.append(P("2.2 Público-alvo", s["h2"]))
    story.append(P(
        "O FinSight destina-se a particulares com literacia digital básica que "
        "pretendam controlar o orçamento doméstico. O âmbito não inclui a gestão "
        "empresarial ou contabilística — não foram modeladas faturas, IVA ou "
        "centros de custo. Em contexto académico, o sistema serve como caso prático "
        "para a disciplina de Bases de Dados.",
        s["body"]
    ))

    story.append(P("2.3 Requisitos do sistema", s["h2"]))

    requisitos = Table(
        [
            ["Componente", "Versão / Detalhe"],
            [".NET SDK", "8.0 ou superior"],
            ["PostgreSQL", "14 ou superior"],
            ["Sistema operativo", "Windows 10/11, Linux ou macOS"],
            ["Memória RAM", "2 GB livres (mínimo recomendado)"],
            ["Espaço em disco", "500 MB para o projeto e ferramentas EF"],
            ["Browser", "Chrome, Edge, Firefox ou Safari (versão recente)"],
            ["Resolução", "1280 × 720 ou superior"],
        ],
        colWidths=[6 * cm, 9 * cm],
    )
    requisitos.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (-1, 0), BASE_FONT_BOLD),
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#0a3d62")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTNAME", (0, 1), (-1, -1), BASE_FONT),
        ("FONTSIZE", (0, 0), (-1, -1), 10),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#cccccc")),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#f4f6f8")]),
        ("ALIGN", (0, 0), (-1, -1), "LEFT"),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
        ("TOPPADDING", (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
    ]))
    story.append(requisitos)
    story.append(P("Tabela 1 — Requisitos mínimos do sistema.", s["caption"]))

    # ====================================================================
    # 3. ARQUITETURA E TECNOLOGIAS
    # ====================================================================
    story.append(P("3. Arquitetura e Tecnologias", s["h1"]))

    story.append(P("3.1 Descrição da arquitetura", s["h2"]))
    story.append(P(
        "A aplicação segue uma arquitetura <b>MVC em três camadas</b>: "
        "apresentação (Razor Views + Bootstrap 5 + Chart.js), aplicação "
        "(Controllers ASP.NET Core 8 e serviços de domínio) e persistência "
        "(Entity Framework Core 8 sobre PostgreSQL).",
        s["body"]
    ))
    story.extend(bullets([
        "<b>Frontend:</b> Razor Pages e Views <i>.cshtml</i>, layout partilhado "
        "<i>_Layout.cshtml</i>, componentes Bootstrap 5 e gráficos Chart.js. Toda "
        "a interface é renderizada no servidor — não existe SPA — o que simplifica "
        "o desenvolvimento académico e reduz a superfície de ataque.",
        "<b>Backend:</b> ASP.NET Core 8 MVC com <i>Controllers</i> dedicados por "
        "agregado (Despesas, Receitas, Categorias, Orçamentos, Dashboard, "
        "Relatórios, Auditoria, Pesquisa). A autenticação é garantida pelo "
        "ASP.NET Core Identity e o Google OAuth é registado via "
        "<i>AddAuthentication().AddGoogle()</i>.",
        "<b>Base de dados:</b> PostgreSQL 14+, acedida pelo provedor Npgsql do EF "
        "Core. As migrações são versionadas em <i>Migrations/</i> e geradas/aplicadas "
        "pela ferramenta <i>dotnet-ef</i> fixada em <i>.config/dotnet-tools.json</i>.",
        "<b>Segurança transversal:</b> isolamento por <i>UserId</i> em todas as "
        "queries, conversão automática para UTC, restrição de eliminação em "
        "categorias com despesas associadas (ON DELETE RESTRICT) e cookies HTTPS.",
    ], s))

    story.append(P("3.2 Justificação das tecnologias escolhidas", s["h2"]))

    just = Table(
        [
            ["Tecnologia", "Justificação"],
            ["ASP.NET Core 8", "Framework <i>cross-platform</i>, com forte tipagem, performance superior\nà do .NET Framework e excelente integração com EF Core."],
            ["PostgreSQL", "SGBD relacional <i>open source</i>, robusto em integridade referencial,\ncom suporte nativo a <i>timestamp with time zone</i> e amplamente usado em\nproduções reais — ideal para a UFCD."],
            ["Entity Framework Core", "ORM oficial da Microsoft, permite migrações versionadas, queries\ntipadas (LINQ) e mantém o foco no modelo de domínio."],
            ["ASP.NET Core Identity", "Pilha de autenticação oficial: gestão de utilizadores,\npalavras-passe, recuperação, 2FA e <i>external login</i>."],
            ["Bootstrap 5 + Chart.js", "Componentes visuais consistentes, responsivos e gráficos\ninterativos sem dependências pesadas."],
            ["ClosedXML", "Geração de ficheiros .xlsx em puro .NET, sem necessidade de\nMicrosoft Office instalado no servidor."],
            ["QRCoder", "Geração de códigos QR para a configuração do TOTP no 2FA."],
        ],
        colWidths=[4.5 * cm, 11 * cm],
    )
    just.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (-1, 0), BASE_FONT_BOLD),
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#0a3d62")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTNAME", (0, 1), (-1, -1), BASE_FONT),
        ("FONTSIZE", (0, 0), (-1, -1), 10),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#cccccc")),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#f4f6f8")]),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
        ("TOPPADDING", (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
    ]))
    story.append(just)
    story.append(P("Tabela 2 — Tecnologias adotadas e respetiva justificação.", s["caption"]))

    story.append(P("3.3 Diagrama de arquitetura", s["h2"]))
    story.append(P(
        "A figura seguinte apresenta uma vista de blocos da arquitetura:",
        s["body"]
    ))

    arq = Table(
        [
            ["Utilizador (Browser)"],
            ["↓ HTTPS"],
            ["Camada de Apresentação\nRazor Views + Bootstrap 5 + Chart.js"],
            ["↓"],
            ["Camada de Aplicação\nASP.NET Core 8 MVC · Identity · Controllers · Filtros"],
            ["↓ EF Core 8 (LINQ)"],
            ["Camada de Persistência\nNpgsql · Migrações"],
            ["↓"],
            ["Base de Dados PostgreSQL\nDB_FinSight"],
        ],
        colWidths=[12 * cm],
    )
    arq.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (-1, -1), BASE_FONT),
        ("FONTSIZE", (0, 0), (-1, -1), 10),
        ("ALIGN", (0, 0), (-1, -1), "CENTER"),
        ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
        ("BACKGROUND", (0, 0), (0, 0), colors.HexColor("#2eb872")),
        ("TEXTCOLOR", (0, 0), (0, 0), colors.white),
        ("FONTNAME", (0, 0), (0, 0), BASE_FONT_BOLD),
        ("BACKGROUND", (0, 2), (0, 2), colors.HexColor("#3498db")),
        ("TEXTCOLOR", (0, 2), (0, 2), colors.white),
        ("FONTNAME", (0, 2), (0, 2), BASE_FONT_BOLD),
        ("BACKGROUND", (0, 4), (0, 4), colors.HexColor("#9b59b6")),
        ("TEXTCOLOR", (0, 4), (0, 4), colors.white),
        ("FONTNAME", (0, 4), (0, 4), BASE_FONT_BOLD),
        ("BACKGROUND", (0, 6), (0, 6), colors.HexColor("#e67e22")),
        ("TEXTCOLOR", (0, 6), (0, 6), colors.white),
        ("FONTNAME", (0, 6), (0, 6), BASE_FONT_BOLD),
        ("BACKGROUND", (0, 8), (0, 8), colors.HexColor("#0a3d62")),
        ("TEXTCOLOR", (0, 8), (0, 8), colors.white),
        ("FONTNAME", (0, 8), (0, 8), BASE_FONT_BOLD),
        ("ROWBACKGROUNDS", (0, 1), (0, 1), [colors.white]),
        ("TOPPADDING", (0, 0), (-1, -1), 6),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 6),
    ]))
    story.append(arq)
    story.append(P("Figura 1 — Diagrama em camadas da arquitetura do FinSight.", s["caption"]))

    # ====================================================================
    # 4. BASE DE DADOS
    # ====================================================================
    story.append(P("4. Base de Dados", s["h1"]))

    story.append(P("4.1 Modelo Entidade-Relacionamento", s["h2"]))
    story.append(P(
        "O modelo relacional do FinSight assenta em quatro entidades de domínio "
        "(<i>Categoria</i>, <i>Despesa</i>, <i>Receita</i>, <i>Orcamento</i>), "
        "duas variantes recorrentes (<i>DespesaRecorrente</i>, <i>ReceitaRecorrente</i>), "
        "uma tabela de auditoria (<i>RegistoAuditoria</i>) e uma extensão do utilizador "
        "(<i>UserProfile</i>), agregadas em torno do <i>IdentityUser</i> da plataforma "
        "ASP.NET Core Identity.",
        s["body"]
    ))

    er = Table(
        [
            ["IdentityUser (1) — (1) UserProfile",
             "Cada conta tem um perfil financeiro com salário, limite e alertas."],
            ["IdentityUser (1) — (N) Categoria",
             "Cada utilizador define as suas categorias; o nome é único por utilizador."],
            ["Categoria (1) — (N) Despesa",
             "Uma categoria agrupa várias despesas. Eliminação restrita (RESTRICT)."],
            ["Categoria (1) — (N) Orcamento",
             "Cada orçamento aplica-se a uma categoria, ano e mês."],
            ["Categoria (0..1) — (N) Categoria",
             "Auto-relação: hierarquia mãe/filha (CategoriaPaiId)."],
            ["IdentityUser (1) — (N) Despesa / Receita",
             "Todas as movimentações financeiras pertencem a um utilizador."],
            ["IdentityUser (1) — (N) DespesaRecorrente / ReceitaRecorrente",
             "Movimentos automáticos com frequência e datas de geração."],
            ["IdentityUser (1) — (N) RegistoAuditoria",
             "Histórico de criação/edição/eliminação por entidade."],
        ],
        colWidths=[6.5 * cm, 9 * cm],
    )
    er.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (-1, -1), BASE_FONT),
        ("FONTSIZE", (0, 0), (-1, -1), 10),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#cccccc")),
        ("ROWBACKGROUNDS", (0, 0), (-1, -1), [colors.HexColor("#f4f6f8"), colors.white]),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("FONTNAME", (0, 0), (0, -1), BASE_FONT_BOLD),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
        ("TOPPADDING", (0, 0), (-1, -1), 4),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 4),
    ]))
    story.append(er)
    story.append(P("Tabela 3 — Cardinalidades principais do modelo E-R.", s["caption"]))

    story.append(P("4.2 Descrição das tabelas", s["h2"]))

    tabelas = Table(
        [
            ["Tabela", "Descrição", "Colunas-chave"],
            ["AspNetUsers", "Utilizadores (Identity)", "Id (PK), Email, PasswordHash, TwoFactorEnabled"],
            ["UserProfiles", "Perfil financeiro do utilizador", "Id (PK), UserId (FK→AspNetUsers, único), SalarioMensal, LimitePercentual, ReceberAlertas"],
            ["Categorias", "Categorias por utilizador", "CategoriaId (PK), Nome, Icone, Cor, CategoriaPaiId (FK), UserId (FK)\nUnique (UserId, Nome)"],
            ["Despesas", "Movimentos de despesa", "DespesaId (PK), Descricao, Valor, Data (UTC), CategoriaId (FK RESTRICT), Observacoes, UserId (FK)"],
            ["Receitas", "Movimentos de receita", "ReceitaId (PK), Descricao, Valor, Data (UTC), Tipo, UserId (FK)"],
            ["DespesasRecorrentes", "Modelos de despesa recorrente", "Id (PK), Frequencia, DataInicio, DataFim, UltimaGeracao, Ativa, CategoriaId (FK)"],
            ["ReceitasRecorrentes", "Modelos de receita recorrente", "Id (PK), Frequencia, DataInicio, DataFim, UltimaGeracao, Ativa"],
            ["Orcamentos", "Limite mensal por categoria", "OrcamentoId (PK), Ano, Mes, CategoriaId (FK), Limite, UserId (FK)"],
            ["RegistosAuditoria", "Pista de auditoria", "Id (PK), UserId, Entidade, EntidadeId, Acao, Detalhes, DataHora"],
        ],
        colWidths=[3.4 * cm, 3.6 * cm, 8.5 * cm],
    )
    tabelas.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (-1, 0), BASE_FONT_BOLD),
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#0a3d62")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTNAME", (0, 1), (-1, -1), BASE_FONT),
        ("FONTSIZE", (0, 0), (-1, -1), 9),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#cccccc")),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#f4f6f8")]),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 4),
        ("RIGHTPADDING", (0, 0), (-1, -1), 4),
        ("TOPPADDING", (0, 0), (-1, -1), 3),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 3),
    ]))
    story.append(tabelas)
    story.append(P("Tabela 4 — Síntese das tabelas de domínio (excertos das colunas).", s["caption"]))

    story.append(P("4.3 Excertos SQL relevantes", s["h2"]))
    story.append(P(
        "Excertos do <i>script</i> de inicialização gerado pelas migrações EF Core "
        "(<i>database/init.sql</i>):",
        s["body"]
    ))
    story.append(P(
        "<font face='Courier' size='9'>"
        "CREATE TABLE \"Categorias\" (<br/>"
        "&nbsp;&nbsp;\"CategoriaId\" integer GENERATED BY DEFAULT AS IDENTITY,<br/>"
        "&nbsp;&nbsp;\"Nome\" character varying(60) NOT NULL,<br/>"
        "&nbsp;&nbsp;\"UserId\" text NOT NULL,<br/>"
        "&nbsp;&nbsp;CONSTRAINT \"PK_Categorias\" PRIMARY KEY (\"CategoriaId\")<br/>"
        ");<br/>"
        "CREATE UNIQUE INDEX \"IX_Categorias_UserId_Nome\" ON \"Categorias\" (\"UserId\", \"Nome\");<br/><br/>"
        "ALTER TABLE \"Despesas\" ADD CONSTRAINT \"FK_Despesas_Categorias_CategoriaId\"<br/>"
        "&nbsp;&nbsp;FOREIGN KEY (\"CategoriaId\") REFERENCES \"Categorias\" (\"CategoriaId\")<br/>"
        "&nbsp;&nbsp;ON DELETE RESTRICT;"
        "</font>",
        s["body"]
    ))
    story.append(P(
        "O <i>script</i> completo encontra-se em anexo (Anexo A) e em "
        "<i>database/init.sql</i>.",
        s["body"]
    ))

    # ====================================================================
    # 5. IMPLEMENTAÇÃO
    # ====================================================================
    story.append(P("5. Implementação", s["h1"]))

    story.append(P("5.1 Processo de desenvolvimento", s["h2"]))
    story.append(P(
        "O desenvolvimento seguiu uma abordagem <b>iterativa e incremental</b>, "
        "organizada em quatro fases principais:",
        s["body"]
    ))
    story.extend(bullets([
        "<b>Fase 1 — Modelação:</b> levantamento de requisitos, desenho do modelo "
        "E-R em papel, normalização até à 3FN e tradução para classes do domínio.",
        "<b>Fase 2 — Andaime (<i>scaffolding</i>):</b> criação do projeto ASP.NET "
        "Core 8 MVC, configuração de Identity, primeira migração e CRUD inicial "
        "para Categorias e Despesas.",
        "<b>Fase 3 — Funcionalidades de valor:</b> orçamentos, dashboard com "
        "Chart.js, autenticação Google e 2FA, exportação para CSV/Excel, pesquisa "
        "global, relatórios e auditoria.",
        "<b>Fase 4 — Robustecimento:</b> validações, paginação, conversão UTC "
        "transversal, testes manuais com dados semeados e <i>scripts</i> "
        "<i>instalar.bat / iniciar.bat / parar.bat</i> para facilitar a entrega.",
    ], s))
    story.append(P(
        "O versionamento foi feito em Git, com <i>branches</i> de funcionalidade e "
        "<i>commits</i> atómicos. As migrações foram criadas pelo CLI <i>dotnet ef "
        "migrations add</i> e aplicadas com <i>dotnet ef database update</i>, "
        "garantindo que cada alteração de modelo corresponde a um <i>script</i> "
        "rastreável.",
        s["body"]
    ))

    story.append(P("5.2 Decisões de implementação relevantes", s["h2"]))
    story.extend(bullets([
        "<b>Conversão UTC global:</b> o <i>ApplicationDbContext</i> regista um "
        "<i>ValueConverter</i> para todas as colunas <i>DateTime</i>, evitando o "
        "erro frequente de armazenar datas locais em colunas <i>timestamp with "
        "time zone</i>.",
        "<b>Filtro por utilizador em todos os <i>controllers</i>:</b> o <i>UserId</i> "
        "é obtido via <i>User.FindFirstValue(ClaimTypes.NameIdentifier)</i> e aplicado "
        "como cláusula <i>WHERE</i>. Esta política impede a fuga de dados entre contas.",
        "<b>RESTRICT em Categoria → Despesa:</b> opção deliberada para proteger o "
        "histórico financeiro. A interface apresenta uma mensagem amigável quando o "
        "utilizador tenta apagar uma categoria com despesas.",
        "<b>Paginação no servidor:</b> a listagem de despesas usa <i>Skip/Take</i> "
        "com <i>page size</i> de 10 — solução escalável e simples de testar.",
        "<b>Exportações reaproveitam o <i>query</i>:</b> os mesmos filtros (categoria, "
        "ano, mês, texto) aplicam-se aos <i>endpoints</i> de exportação, garantindo "
        "consistência entre o que o utilizador vê e o que descarrega.",
        "<b>Configuração externa via <i>appsettings.json</i>:</b> credenciais e "
        "<i>connection string</i> ficam fora do controlo de versões; o repositório "
        "publica apenas <i>appsettings.Example.json</i>.",
        "<b><i>SeedTestData</i>:</b> no primeiro arranque é criado um utilizador "
        "de teste (<i>test@finsight.pt</i> / <i>Test123!</i>) com 5 categorias, 5 "
        "orçamentos e 40 despesas — facilita a avaliação sem ser preciso introduzir "
        "dados manualmente.",
    ], s))

    story.append(P("5.3 Funcionalidades implementadas (mapa de <i>controllers</i>)", s["h2"]))

    ctrls = Table(
        [
            ["Controller", "Rotas principais"],
            ["DashboardController", "GET /Dashboard — totais, gráficos, alertas"],
            ["DespesasController", "GET/POST /Despesas/{Index,Create,Edit,Delete,Details,ExportarCsv,ExportarExcel}"],
            ["CategoriasController", "GET/POST /Categorias/{Index,Create,Edit,Delete,Details,SeedDefault}"],
            ["OrcamentosController", "GET/POST /Orcamentos/{Index,Create,Edit,Delete,Details}"],
            ["ReceitasController", "GET/POST /Receitas/{Index,Create,Edit,Delete,Details}"],
            ["DespesasRecorrentesController", "Gestão de modelos recorrentes de despesa"],
            ["ReceitasRecorrentesController", "Gestão de modelos recorrentes de receita"],
            ["RelatoriosController", "GET /Relatorios — relatórios por intervalo e mês homólogo"],
            ["PesquisaController", "GET /Pesquisa — pesquisa global por descrição"],
            ["AuditoriaController", "GET /Auditoria — histórico paginado de ações"],
            ["Areas/Identity (Razor Pages)", "Login, Registo, Google OAuth, 2FA, FinancialSettings"],
        ],
        colWidths=[5 * cm, 10.5 * cm],
    )
    ctrls.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (-1, 0), BASE_FONT_BOLD),
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#0a3d62")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTNAME", (0, 1), (-1, -1), BASE_FONT),
        ("FONTSIZE", (0, 0), (-1, -1), 9.5),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#cccccc")),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#f4f6f8")]),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("LEFTPADDING", (0, 0), (-1, -1), 6),
        ("RIGHTPADDING", (0, 0), (-1, -1), 6),
        ("TOPPADDING", (0, 0), (-1, -1), 3),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 3),
    ]))
    story.append(ctrls)
    story.append(P("Tabela 5 — <i>Controllers</i> e responsabilidades.", s["caption"]))

    # ====================================================================
    # 6. TESTES E VALIDAÇÃO
    # ====================================================================
    story.append(P("6. Testes e Validação", s["h1"]))

    story.append(P("6.1 Metodologia de testes", s["h2"]))
    story.append(P(
        "Não foi adotado um <i>framework</i> de testes automáticos no escopo da "
        "UFCD; em alternativa, foi seguida uma metodologia de <b>testes manuais "
        "estruturados</b> sobre uma base semeada (<i>SeedTestData</i>) e uma "
        "<b>matriz de cenários</b> que cobre os requisitos funcionais e não "
        "funcionais. Os testes foram repetidos sempre que uma migração era "
        "aplicada e antes de cada entrega.",
        s["body"]
    ))

    story.append(P("6.2 Principais testes realizados", s["h2"]))

    testes = Table(
        [
            ["Cenário", "Resultado esperado", "Estado"],
            ["Registo + Login com email/palavra-passe", "Sessão autenticada e redireccionamento ao Dashboard", "OK"],
            ["Login com Google OAuth", "Conta criada e sessão autenticada", "OK"],
            ["Ativação 2FA via TOTP (QR Code)", "Pedido de código no <i>login</i> seguinte", "OK"],
            ["Criar despesa com data futura", "Validação rejeita o formulário", "OK"],
            ["Criar duas categorias com o mesmo nome", "Segunda criação rejeitada (índice único)", "OK"],
            ["Eliminar categoria com despesas", "Operação bloqueada com mensagem amigável", "OK"],
            ["Filtrar despesas por mês/ano/categoria/texto", "Lista resultante respeita os filtros", "OK"],
            ["Paginação da lista de despesas", "10 itens por página, navegação consistente", "OK"],
            ["Exportar CSV/Excel com filtros ativos", "Ficheiro contém apenas as linhas filtradas", "OK"],
            ["Definir orçamento e ultrapassá-lo no mês", "Barra de progresso a 100%+ e alerta no dashboard", "OK"],
            ["Aceder a despesa de outro utilizador (URL forjado)", "Devolve 404 / acesso negado", "OK"],
            ["Migração da BD em ambiente limpo", "Esquema criado sem erros", "OK"],
            ["<i>Seed</i> automático na primeira execução", "Utilizador de teste com 40 despesas", "OK"],
            ["Auditoria de criar/editar/eliminar", "Registo correspondente persistido", "OK"],
        ],
        colWidths=[7.5 * cm, 6.5 * cm, 1.5 * cm],
    )
    testes.setStyle(TableStyle([
        ("FONTNAME", (0, 0), (-1, 0), BASE_FONT_BOLD),
        ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#0a3d62")),
        ("TEXTCOLOR", (0, 0), (-1, 0), colors.white),
        ("FONTNAME", (0, 1), (-1, -1), BASE_FONT),
        ("FONTSIZE", (0, 0), (-1, -1), 9.5),
        ("GRID", (0, 0), (-1, -1), 0.5, colors.HexColor("#cccccc")),
        ("ROWBACKGROUNDS", (0, 1), (-1, -1), [colors.white, colors.HexColor("#f4f6f8")]),
        ("VALIGN", (0, 0), (-1, -1), "TOP"),
        ("ALIGN", (2, 1), (2, -1), "CENTER"),
        ("FONTNAME", (2, 1), (2, -1), BASE_FONT_BOLD),
        ("TEXTCOLOR", (2, 1), (2, -1), colors.HexColor("#1e8449")),
        ("LEFTPADDING", (0, 0), (-1, -1), 5),
        ("RIGHTPADDING", (0, 0), (-1, -1), 5),
        ("TOPPADDING", (0, 0), (-1, -1), 3),
        ("BOTTOMPADDING", (0, 0), (-1, -1), 3),
    ]))
    story.append(testes)
    story.append(P("Tabela 6 — Cenários de teste e respetivo estado.", s["caption"]))

    story.append(P("6.3 Resultados obtidos", s["h2"]))
    story.append(P(
        "Todos os cenários listados foram concluídos com sucesso na <i>build</i> "
        "final. Em cada teste foi confirmado: (i) o comportamento funcional "
        "esperado, (ii) a integridade dos dados na BD via <i>queries</i> SQL "
        "diretas no PostgreSQL e (iii) a inexistência de fugas de dados entre "
        "utilizadores (validação manual com duas contas distintas).",
        s["body"]
    ))

    # ====================================================================
    # 7. GUIA DE INSTALAÇÃO E UTILIZAÇÃO
    # ====================================================================
    story.append(P("7. Guia de Instalação e Utilização", s["h1"]))

    story.append(P("7.1 Pré-requisitos", s["h2"]))
    story.extend(bullets([
        "<b>.NET 8 SDK</b> — <i>https://dotnet.microsoft.com/download/dotnet/8.0</i>",
        "<b>PostgreSQL 14+</b> com o utilitário <i>psql</i> disponível no PATH.",
        "<b>Sistema operativo:</b> Windows 10/11 (recomendado para os <i>scripts</i> .bat) ou Linux/macOS.",
        "<b>Browser moderno</b> (Chrome, Edge, Firefox ou Safari).",
        "<b>Acesso à Internet</b> (para o login Google e <i>restore</i> de pacotes NuGet).",
    ], s))

    story.append(P("7.2 Instalação (Windows — instalar.bat)", s["h2"]))
    story.append(P(
        "O <i>script</i> <b>instalar.bat</b>, na raiz do projeto, automatiza toda "
        "a preparação do ambiente:",
        s["body"]
    ))
    story.extend(bullets([
        "Verifica a presença do <i>dotnet</i> e do <i>psql</i> no PATH.",
        "Cria <i>appsettings.json</i> a partir de <i>appsettings.Example.json</i> "
        "se ainda não existir.",
        "Restaura a ferramenta <i>dotnet-ef</i> (<i>dotnet tool restore</i>).",
        "Cria, no PostgreSQL, o utilizador <i>finsight</i> e a base de dados "
        "<i>DB_FinSight</i> (ignorados se já existirem).",
        "Aplica todas as <i>migrations</i> com <i>dotnet ef database update</i>.",
    ], s))
    story.append(P(
        "Após executar <b>instalar.bat</b>, o ambiente fica pronto para arranque.",
        s["body"]
    ))

    story.append(P("7.3 Iniciar e parar a aplicação", s["h2"]))
    story.extend(bullets([
        "<b>iniciar.bat</b> — arranca o serviço PostgreSQL (se necessário), "
        "executa <i>dotnet run --launch-profile https</i>, aguarda que a porta "
        "7093 esteja a escutar e abre <i>https://localhost:7093</i> no browser "
        "predefinido.",
        "<b>parar.bat</b> — termina os processos do <i>backend</i> que estejam a "
        "escutar nas portas 7093/5067 e desliga os serviços do PostgreSQL "
        "associados.",
    ], s))

    story.append(P("7.4 Alternativa multi-plataforma (manual)", s["h2"]))
    story.append(P(
        "Em Linux/macOS, ou quando se prefere o controlo manual em Windows, os "
        "passos são:",
        s["body"]
    ))
    story.append(P(
        "<font face='Courier' size='9'>"
        "dotnet tool restore<br/>"
        "psql -U postgres -c \"CREATE USER finsight WITH PASSWORD 'finsight123';\"<br/>"
        "psql -U postgres -c \"CREATE DATABASE \\\"DB_FinSight\\\" OWNER finsight;\"<br/>"
        "cd backend/GestaoDespesas<br/>"
        "dotnet ef database update --project GestaoDespesas<br/>"
        "dotnet run --project GestaoDespesas"
        "</font>",
        s["body"]
    ))

    story.append(P("7.5 Guia básico de utilização", s["h2"]))
    story.extend(bullets([
        "Aceder a <i>https://localhost:7093</i> e autenticar-se com a conta de "
        "demonstração (<i>test@finsight.pt</i> / <i>Test123!</i>) ou criar uma nova.",
        "Em <b>Categorias</b>, clicar em <i>Semear categorias por defeito</i> "
        "para popular as 10 categorias iniciais.",
        "Em <b>Despesas → Nova</b>, registar movimentos. Os filtros no topo "
        "permitem cruzar categoria, ano, mês e texto livre.",
        "Em <b>Orçamentos → Novo</b>, definir limites mensais. As barras de "
        "progresso aparecem automaticamente no Dashboard.",
        "Em <b>Definições → Configurações financeiras</b>, indicar o salário "
        "mensal e a percentagem-limite para personalizar os alertas.",
        "Em <b>Despesas</b>, usar <i>Exportar CSV</i> ou <i>Exportar Excel</i> "
        "para descarregar a vista filtrada.",
    ], s))

    # ====================================================================
    # 8. CONCLUSÕES
    # ====================================================================
    story.append(P("8. Conclusões", s["h1"]))

    story.append(P("8.1 Reflexão sobre o trabalho desenvolvido", s["h2"]))
    story.append(P(
        "O FinSight cumpriu o que se propôs: demonstrar, num caso real e "
        "completo, a aplicação dos conceitos da UFCD 5425 — modelação E-R, "
        "normalização, integridade referencial, consultas e migrações — "
        "integrados numa aplicação web funcional. A escolha do <i>stack</i> "
        ".NET 8 + PostgreSQL revelou-se acertada: o EF Core acelerou o "
        "desenvolvimento sem esconder o SQL gerado, e o PostgreSQL respondeu "
        "sem incidentes a todos os cenários testados.",
        s["body"]
    ))

    story.append(P("8.2 Objetivos alcançados", s["h2"]))
    story.extend(bullets([
        "Modelo de dados normalizado, com restrições de unicidade, FK e UTC.",
        "CRUD completo para todas as entidades de domínio.",
        "Autenticação robusta (Identity + Google OAuth + 2FA).",
        "Dashboard com Chart.js e relatórios temporais.",
        "Exportação CSV/Excel respeitando filtros.",
        "Auditoria de operações sensíveis.",
        "<i>Scripts</i> de instalação/arranque/paragem para Windows.",
        "Documentação técnica viva (<i>CLAUDE.md</i>, <i>README.md</i>, este relatório).",
    ], s))

    story.append(P("8.3 Limitações identificadas", s["h2"]))
    story.extend(bullets([
        "Ausência de testes unitários e de integração automatizados.",
        "A geração de movimentos recorrentes ainda depende de um <i>trigger</i> "
        "manual; não existe um <i>background service</i> agendado.",
        "Não existe API pública (REST/GraphQL) para integração com aplicações "
        "móveis ou terceiros.",
        "A internacionalização está limitada ao pt-PT.",
        "Não foi implementada a partilha de orçamentos entre utilizadores "
        "(p. ex. casais ou famílias).",
    ], s))

    story.append(P("8.4 Possíveis melhorias futuras", s["h2"]))
    story.extend(bullets([
        "Introduzir um projeto <i>GestaoDespesas.Tests</i> com xUnit + bUnit "
        "para cobrir <i>controllers</i>, validadores e regras de negócio.",
        "Implementar <i>Hosted Services</i> que executem diariamente a geração "
        "automática de movimentos recorrentes.",
        "Expor uma API REST autenticada por <i>JWT</i> e desenhar uma versão "
        "móvel (MAUI) que consuma essa API.",
        "Adicionar suporte multi-idioma com <i>resource files</i> .resx (en-US, es-ES).",
        "Modelar <i>Households</i> para permitir orçamentos partilhados.",
        "Integrar com <i>Open Banking</i> para importação automática de "
        "movimentos bancários.",
        "Acrescentar deteção de outliers de gasto com modelos estatísticos simples.",
    ], s))

    # ====================================================================
    # ANEXOS
    # ====================================================================
    story.append(PageBreak())
    story.append(P("Anexos", s["h1"]))

    story.append(P("Anexo A — Excerto do <i>script</i> SQL completo", s["h2"]))
    story.append(P(
        "O ficheiro <i>database/init.sql</i> contém a sequência completa de "
        "instruções DDL geradas pelas <i>migrations</i> EF Core. A título "
        "ilustrativo, segue um excerto da criação das tabelas centrais:",
        s["body"]
    ))

    story.append(P(
        "<font face='Courier' size='8'>"
        "CREATE TABLE \"AspNetUsers\" (...);<br/>"
        "CREATE TABLE \"UserProfiles\" (<br/>"
        "&nbsp;&nbsp;\"Id\" integer GENERATED BY DEFAULT AS IDENTITY,<br/>"
        "&nbsp;&nbsp;\"UserId\" text NOT NULL,<br/>"
        "&nbsp;&nbsp;\"SalarioMensal\" numeric(18,2) NOT NULL,<br/>"
        "&nbsp;&nbsp;\"LimitePercentual\" integer NOT NULL,<br/>"
        "&nbsp;&nbsp;\"ReceberAlertas\" boolean NOT NULL,<br/>"
        "&nbsp;&nbsp;CONSTRAINT \"PK_UserProfiles\" PRIMARY KEY (\"Id\"),<br/>"
        "&nbsp;&nbsp;CONSTRAINT \"FK_UserProfiles_AspNetUsers_UserId\"<br/>"
        "&nbsp;&nbsp;&nbsp;&nbsp;FOREIGN KEY (\"UserId\") REFERENCES \"AspNetUsers\" (\"Id\") ON DELETE CASCADE<br/>"
        ");<br/><br/>"
        "CREATE TABLE \"Despesas\" (<br/>"
        "&nbsp;&nbsp;\"DespesaId\" integer GENERATED BY DEFAULT AS IDENTITY,<br/>"
        "&nbsp;&nbsp;\"Descricao\" character varying(120) NOT NULL,<br/>"
        "&nbsp;&nbsp;\"Valor\" numeric NOT NULL,<br/>"
        "&nbsp;&nbsp;\"Data\" timestamp with time zone NOT NULL,<br/>"
        "&nbsp;&nbsp;\"CategoriaId\" integer NOT NULL,<br/>"
        "&nbsp;&nbsp;\"Observacoes\" text,<br/>"
        "&nbsp;&nbsp;\"UserId\" text NOT NULL,<br/>"
        "&nbsp;&nbsp;CONSTRAINT \"PK_Despesas\" PRIMARY KEY (\"DespesaId\"),<br/>"
        "&nbsp;&nbsp;CONSTRAINT \"FK_Despesas_Categorias_CategoriaId\"<br/>"
        "&nbsp;&nbsp;&nbsp;&nbsp;FOREIGN KEY (\"CategoriaId\") REFERENCES \"Categorias\" (\"CategoriaId\") ON DELETE RESTRICT<br/>"
        ");<br/><br/>"
        "CREATE TABLE \"Orcamentos\" (<br/>"
        "&nbsp;&nbsp;\"OrcamentoId\" integer GENERATED BY DEFAULT AS IDENTITY,<br/>"
        "&nbsp;&nbsp;\"Ano\" integer NOT NULL,<br/>"
        "&nbsp;&nbsp;\"Mes\" integer NOT NULL,<br/>"
        "&nbsp;&nbsp;\"CategoriaId\" integer NOT NULL,<br/>"
        "&nbsp;&nbsp;\"Limite\" numeric NOT NULL,<br/>"
        "&nbsp;&nbsp;\"UserId\" text NOT NULL,<br/>"
        "&nbsp;&nbsp;CONSTRAINT \"PK_Orcamentos\" PRIMARY KEY (\"OrcamentoId\"),<br/>"
        "&nbsp;&nbsp;CONSTRAINT \"FK_Orcamentos_Categorias_CategoriaId\"<br/>"
        "&nbsp;&nbsp;&nbsp;&nbsp;FOREIGN KEY (\"CategoriaId\") REFERENCES \"Categorias\" (\"CategoriaId\") ON DELETE CASCADE<br/>"
        ");"
        "</font>",
        s["body"]
    ))

    story.append(P("Anexo B — Lista de <i>migrations</i> EF Core", s["h2"]))
    story.extend(bullets([
        "<i>20260225190740_InitialPostgres</i> — esquema inicial.",
        "<i>20260225205308_AppDbUpdate</i> — ajustes a Categorias/Despesas e índice único.",
        "<i>20260226191251_TestData</i> — refinamento e dados de teste.",
        "<i>20260226210840_testAcc</i> — ajustes à conta de demonstração.",
        "<i>20260310120000_AddReceitasTable</i> — introdução da tabela Receitas.",
    ], s))

    story.append(P("Anexo C — Estrutura de pastas relevante", s["h2"]))
    story.append(P(
        "<font face='Courier' size='9'>"
        "FinSight/<br/>"
        "├── backend/GestaoDespesas/GestaoDespesas/<br/>"
        "│&nbsp;&nbsp;&nbsp;├── Controllers/<br/>"
        "│&nbsp;&nbsp;&nbsp;├── Data/  (ApplicationDbContext, SeedTestData)<br/>"
        "│&nbsp;&nbsp;&nbsp;├── Migrations/<br/>"
        "│&nbsp;&nbsp;&nbsp;├── Models/<br/>"
        "│&nbsp;&nbsp;&nbsp;├── Views/  (Razor)<br/>"
        "│&nbsp;&nbsp;&nbsp;├── Areas/Identity/  (Login, 2FA, FinancialSettings)<br/>"
        "│&nbsp;&nbsp;&nbsp;└── Program.cs<br/>"
        "├── database/  (init.sql, create_db.sql)<br/>"
        "├── docs/  (este relatório)<br/>"
        "├── instalar.bat · iniciar.bat · parar.bat<br/>"
        "└── README.md / CLAUDE.md / LICENSE.txt"
        "</font>",
        s["body"]
    ))

    # ---------------------------------------------------------------------
    # Construir com índice de duas passagens
    # ---------------------------------------------------------------------
    doc.multiBuild(story)


if __name__ == "__main__":
    out = os.path.join(os.path.dirname(__file__), "Relatorio_FinSight.pdf")
    build(out)
    print(f"PDF gerado: {out}")
