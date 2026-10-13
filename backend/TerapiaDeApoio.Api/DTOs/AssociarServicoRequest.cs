using System.ComponentModel.DataAnnotations;

namespace TerapiaDeApoio.Api.DTOs;

public sealed class AssociarServicoRequest
{
    [Range(1, int.MaxValue)]
    public int ServicoId { get; set; }
}