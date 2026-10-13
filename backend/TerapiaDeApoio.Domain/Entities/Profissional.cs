namespace TerapiaDeApoio.Domain.Entities;

public class Profissional
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Especialidade { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    public ICollection<Agendamento> Agendamentos { get; set; }
        = new List<Agendamento>();

    public ICollection<DisponibilidadeProfissional> Disponibilidades
    { get; set; } = new List<DisponibilidadeProfissional>();

    public ICollection<ProfissionalServico> ProfissionalServicos
    { get; set; } = new List<ProfissionalServico>();
}