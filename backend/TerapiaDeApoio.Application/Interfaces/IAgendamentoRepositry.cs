using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Interfaces;

public interface IAgendamentoRepository
{
    Task<bool> ExisteConflitoAsync(
        int profissionalId,
        DateTime inicio,
        DateTime fim,
        CancellationToken cancellationToken);

    Task<Agendamento?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task AdicionarAsync(
        Agendamento agendamento,
        CancellationToken cancellationToken);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Agendamento>> ListarPorTelefoneAsync(
string telefoneNormalizado,
CancellationToken cancellationToken);
}