using TerapiaDeApoio.Domain.Enums;

namespace TerapiaDeApoio.Domain.Entities;

public class Paciente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Email { get; set; }

    public ICollection<Agendamento> Agendamentos { get; set; }
        = new List<Agendamento>();

}