namespace TerapiaDeApoio.Application.Interfaces;

public interface IProfissionalRepository
{
    Task<bool> ProfissionalOfereceServicoAsync(
        int profissionalId,
        int servicoId,
        CancellationToken cancellationToken);

    Task<bool> EstaDentroDaDisponibilidadeAsync(
        int profissionalId,
        DayOfWeek diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        CancellationToken cancellationToken);
}