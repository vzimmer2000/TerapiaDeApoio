using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Interfaces;

public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Servico>> ListarAtivosAsync(
        CancellationToken cancellationToken);

    Task AdicionarAsync(
        Servico servico,
        CancellationToken cancellationToken);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken);
}
