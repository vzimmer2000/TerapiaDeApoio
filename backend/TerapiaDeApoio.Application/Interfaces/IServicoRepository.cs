using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Interfaces;

public interface IServicoRepository
{
    Task<Servico?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken);
}