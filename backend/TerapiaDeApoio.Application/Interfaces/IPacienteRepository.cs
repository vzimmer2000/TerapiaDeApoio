using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Interfaces;

public interface IPacienteRepository
{
    Task<Paciente?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken);
}