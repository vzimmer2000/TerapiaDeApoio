using System.ComponentModel.DataAnnotations;

namespace TerapiaDeApoio.Api.DTOs;

public sealed class CriarAgendamentoRequest
{
    [Range(1, int.MaxValue)]
    public int PacienteId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProfissionalId { get; set; }

    [Range(1, int.MaxValue)]
    public int ServicoId { get; set; }

    [Required]
    public DateTime? DataHoraInicio { get; set; }
}