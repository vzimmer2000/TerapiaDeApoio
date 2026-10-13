using TerapiaDeApoio.Domain.Enums;

namespace TerapiaDeApoio.Domain.Entities;

public class Agendamento
{
    public int Id { get; set; }

    public int PacienteId { get; set; }

    public int ProfissionalId { get; set; }

    public int ServicoId { get; set; }

    public DateTime DataHoraInicio { get; set; }

    public DateTime DataHoraFim { get; set; }

    public StatusAgendamento Status { get; private set; }
        = StatusAgendamento.Agendado;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public Paciente Paciente { get; set; } = null!;

    public Profissional Profissional { get; set; } = null!;

    public Servico Servico { get; set; } = null!;

    public void Cancelar()
    {
        if (Status != StatusAgendamento.Agendado)
        {
            throw new InvalidOperationException(
                "Somente agendamentos ativos podem ser cancelados.");
        }

        Status = StatusAgendamento.Cancelado;
    }

    public void Concluir()
    {
        if (Status != StatusAgendamento.Agendado)
        {
            throw new InvalidOperationException(
                "Somente agendamentos ativos podem ser concluídos.");
        }

        Status = StatusAgendamento.Concluido;
    }
}