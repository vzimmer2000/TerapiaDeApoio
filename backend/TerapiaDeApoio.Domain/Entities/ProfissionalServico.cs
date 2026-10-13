namespace TerapiaDeApoio.Domain.Entities;

public class ProfissionalServico
{
    public int ProfissionalId { get; set; }

    public int ServicoId { get; set; }

    public Profissional Profissional { get; set; } = null!;

    public Servico Servico { get; set; } = null!;
}