using System.ComponentModel.DataAnnotations;

namespace TerapiaDeApoio.Api.DTOs;

public sealed class CancelarAgendamentoRequest
{
    [Required]
    [StringLength(30)]
    public string Telefone { get; set; } = string.Empty;
}