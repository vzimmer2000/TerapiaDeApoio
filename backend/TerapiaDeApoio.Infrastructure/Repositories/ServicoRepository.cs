using Microsoft.EntityFrameworkCore;
using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Domain.Entities;
using TerapiaDeApoio.Infrastructure.Data;

namespace TerapiaDeApoio.Infrastructure.Repositories;

public class ServicoRepository : IServicoRepository
{
    private readonly TerapiaDeApoioDbContext _context;

    public ServicoRepository(TerapiaDeApoioDbContext context)
    {
        _context = context;
    }

    public Task<Servico?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _context.Servicos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.Id == id,
                cancellationToken);
    }
}