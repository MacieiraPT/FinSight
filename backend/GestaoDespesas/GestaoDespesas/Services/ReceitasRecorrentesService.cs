using GestaoDespesas.Data;
using GestaoDespesas.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaoDespesas.Services;

public class ReceitasRecorrentesService
{
    private readonly ApplicationDbContext _context;

    public ReceitasRecorrentesService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task ProcessarRecorrentesAsync()
    {
        var hoje = DateTime.UtcNow.Date;

        var recorrentes = await _context.ReceitasRecorrentes
            .Where(r => r.Ativa && r.DataInicio <= hoje && (r.DataFim == null || r.DataFim >= hoje))
            .ToListAsync();

        foreach (var rec in recorrentes)
        {
            var datasParaGerar = CalcularDatasEmFalta(rec, hoje);

            foreach (var data in datasParaGerar)
            {
                _context.Receitas.Add(new Receita
                {
                    Descricao = rec.Descricao,
                    Valor = rec.Valor,
                    Data = data,
                    Tipo = rec.Tipo,
                    UserId = rec.UserId
                });
            }

            if (datasParaGerar.Any())
            {
                rec.UltimaGeracao = datasParaGerar.Max();
            }
        }

        await _context.SaveChangesAsync();
    }

    private List<DateTime> CalcularDatasEmFalta(ReceitaRecorrente rec, DateTime hoje)
    {
        var datas = new List<DateTime>();
        var inicio = rec.UltimaGeracao.HasValue
            ? ProximaData(rec.UltimaGeracao.Value, rec.Frequencia)
            : rec.DataInicio;

        var atual = inicio;

        while (atual <= hoje)
        {
            if (rec.DataFim.HasValue && atual > rec.DataFim.Value)
                break;

            datas.Add(DateTime.SpecifyKind(atual, DateTimeKind.Utc));
            atual = ProximaData(atual, rec.Frequencia);
        }

        return datas;
    }

    private DateTime ProximaData(DateTime data, string frequencia)
    {
        return frequencia switch
        {
            "Semanal" => data.AddDays(7),
            "Mensal" => data.AddMonths(1),
            "Anual" => data.AddYears(1),
            _ => data.AddMonths(1)
        };
    }
}
