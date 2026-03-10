using GestaoDespesas.Data;
using GestaoDespesas.Models;

namespace GestaoDespesas.Services;

public class AuditoriaService
{
    private readonly ApplicationDbContext _context;

    public AuditoriaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RegistarAsync(string userId, string entidade, int entidadeId, string acao, string? detalhes = null)
    {
        _context.RegistosAuditoria.Add(new RegistoAuditoria
        {
            UserId = userId,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Acao = acao,
            Detalhes = detalhes,
            DataHora = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
