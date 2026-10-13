using Microsoft.EntityFrameworkCore;
using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Domain.Entities;
using TerapiaDeApoio.Infrastructure.Data;

namespace TerapiaDeApoio.Infrastructure.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly TerapiaDeApoioDbContext _context;

    public PacienteRepository(TerapiaDeApoioDbContext context)
    {
        _context = context;
    }

    public Task<Paciente?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _context.Pacientes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }
}