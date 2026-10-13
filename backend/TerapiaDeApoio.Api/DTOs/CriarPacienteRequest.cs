using System.ComponentModel.DataAnnotations;

namespace TerapiaDeApoio.Api.DTOs;

public sealed class CriarPacienteRequest
{
    [Required]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Telefone { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(254)]
    public string? Email { get; set; }
}