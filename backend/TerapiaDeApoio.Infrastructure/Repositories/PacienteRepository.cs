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

    public async Task<IReadOnlyList<Paciente>> ListarAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Pacientes
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
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

    public async Task AdicionarAsync(
        Paciente paciente,
        CancellationToken cancellationToken)
    {
        await _context.Pacientes.AddAsync(
            paciente,
            cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}