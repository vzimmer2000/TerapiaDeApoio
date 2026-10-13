using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Interfaces;

public interface IPacienteRepository
{
    Task<IReadOnlyList<Paciente>> ListarAsync(
        CancellationToken cancellationToken);

    Task<Paciente?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task AdicionarAsync(
        Paciente paciente,
        CancellationToken cancellationToken);

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken);
}