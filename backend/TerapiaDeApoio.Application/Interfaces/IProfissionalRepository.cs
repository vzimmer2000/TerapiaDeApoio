using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Interfaces;

public interface IProfissionalRepository
{
    Task<IReadOnlyList<Profissional>> ListarAsync(
        CancellationToken cancellationToken);

    Task<Profissional?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task AdicionarAsync(
        Profissional profissional,
        CancellationToken cancellationToken);

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

    Task AdicionarServicoAsync(
        ProfissionalServico associacao,
        CancellationToken cancellationToken);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken);
}