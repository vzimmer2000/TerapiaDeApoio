namespace TerapiaDeApoio.Domain.Entities;

public class Servico
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public int DuracaoMinutos { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<Agendamento> Agendamentos { get; set; }
        = new List<Agendamento>();

    public ICollection<ProfissionalServico> ProfissionalServicos
    { get; set; } = new List<ProfissionalServico>();
}