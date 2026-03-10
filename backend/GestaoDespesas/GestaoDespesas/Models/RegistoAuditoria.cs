using System;
using System.ComponentModel.DataAnnotations;

namespace GestaoDespesas.Models;

public class RegistoAuditoria
{
    public int RegistoAuditoriaId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Entidade { get; set; } = string.Empty;

    public int EntidadeId { get; set; }

    [Required, StringLength(20)]
    public string Acao { get; set; } = string.Empty; // Criar, Editar, Eliminar

    public string? Detalhes { get; set; }

    public DateTime DataHora { get; set; } = DateTime.UtcNow;
}
