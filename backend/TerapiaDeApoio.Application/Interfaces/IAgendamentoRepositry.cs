using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Interfaces;

public interface IAgendamentoRepository
{
    Task<bool> PossuiCapacidadeAsync(
        int profissionalId,
        DateTime inicio,
        DateTime fim,
        int capacidadeMaxima,
        CancellationToken cancellationToken);

    Task<bool> AdicionarSeHouverCapacidadeAsync(
        Agendamento agendamento,
        int capacidadeMaxima,
        CancellationToken cancellationToken);

    Task<Agendamento?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Agendamento>> ListarPorTelefoneAsync(
        string telefoneNormalizado,
        CancellationToken cancellationToken);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Agendamento>> ListarAgendaAsync(
DateOnly data,
int? profissionalId,
CancellationToken cancellationToken);
}