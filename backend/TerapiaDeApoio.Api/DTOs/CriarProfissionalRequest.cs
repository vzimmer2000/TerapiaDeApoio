using System.ComponentModel.DataAnnotations;

namespace TerapiaDeApoio.Api.DTOs;

public sealed class CriarProfissionalRequest
{
    [Required]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Especialidade { get; set; } = string.Empty;
}