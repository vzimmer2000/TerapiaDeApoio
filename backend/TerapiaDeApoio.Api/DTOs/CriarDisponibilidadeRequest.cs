using System.ComponentModel.DataAnnotations;

namespace TerapiaDeApoio.Api.DTOs;

public sealed class CriarDisponibilidadeRequest
{
    [Required]
    [Range(0, 6)]
    public int? DiaSemana { get; set; }

    [Required]
    public TimeOnly? HoraInicio { get; set; }

    [Required]
    public TimeOnly? HoraFim { get; set; }
}