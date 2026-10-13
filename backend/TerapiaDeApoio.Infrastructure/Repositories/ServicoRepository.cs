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
    public async Task<IReadOnlyList<Servico>> ListarAtivosAsync(
    CancellationToken cancellationToken)
    {
        return await _context.Servicos
            .AsNoTracking()
            .Where(s => s.Ativo)
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(
        Servico servico,
        CancellationToken cancellationToken)
    {
        await _context.Servicos.AddAsync(
            servico,
            cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<DisponibilidadeProfissional>>
    ListarDisponibilidadesAsync(
        int profissionalId,
        CancellationToken cancellationToken)
    {
        return await _context.DisponibilidadesProfissionais
            .AsNoTracking()
            .Where(d => d.ProfissionalId == profissionalId)
            .OrderBy(d => d.DiaSemana)
            .ThenBy(d => d.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExisteConflitoDisponibilidadeAsync(
        int profissionalId,
        DayOfWeek diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        CancellationToken cancellationToken)
    {
        return _context.DisponibilidadesProfissionais.AnyAsync(
            d => d.ProfissionalId == profissionalId
                && d.DiaSemana == diaSemana
                && d.HoraInicio < horaFim
                && d.HoraFim > horaInicio,
            cancellationToken);
    }

    public async Task AdicionarDisponibilidadeAsync(
        DisponibilidadeProfissional disponibilidade,
        CancellationToken cancellationToken)
    {
        await _context.DisponibilidadesProfissionais.AddAsync(
            disponibilidade,
            cancellationToken);
    }
}