using Microsoft.EntityFrameworkCore;
using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Domain.Entities;
using TerapiaDeApoio.Infrastructure.Data;

namespace TerapiaDeApoio.Infrastructure.Repositories;

public class ProfissionalRepository : IProfissionalRepository
{
    private readonly TerapiaDeApoioDbContext _context;

    public ProfissionalRepository(TerapiaDeApoioDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Profissional>> ListarAsync(
        CancellationToken cancellationToken)
    {
        return await _context.Profissionais
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Profissional?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _context.Profissionais
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == id,
                cancellationToken);
    }

    public async Task AdicionarAsync(
        Profissional profissional,
        CancellationToken cancellationToken)
    {
        await _context.Profissionais.AddAsync(
            profissional,
            cancellationToken);
    }

    public Task<bool> ProfissionalOfereceServicoAsync(
        int profissionalId,
        int servicoId,
        CancellationToken cancellationToken)
    {
        return _context.Profissionais
            .AnyAsync(
                p => p.Id == profissionalId
                    && p.Ativo
                    && p.ProfissionalServicos.Any(
                        ps => ps.ServicoId == servicoId),
                cancellationToken);
    }

    public Task<bool> EstaDentroDaDisponibilidadeAsync(
        int profissionalId,
        DayOfWeek diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        CancellationToken cancellationToken)
    {
        return _context.DisponibilidadesProfissionais
            .AnyAsync(
                d => d.ProfissionalId == profissionalId
                    && d.DiaSemana == diaSemana
                    && d.HoraInicio <= horaInicio
                    && d.HoraFim >= horaFim,
                cancellationToken);
    }

    public async Task AdicionarServicoAsync(
        ProfissionalServico associacao,
        CancellationToken cancellationToken)
    {
        await _context.ProfissionalServicos.AddAsync(
            associacao,
            cancellationToken);
    }

    // NOVO: lista os horários cadastrados para um profissional.
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

    // NOVO: verifica se dois intervalos se sobrepõem.
    public Task<bool> ExisteConflitoDisponibilidadeAsync(
        int profissionalId,
        DayOfWeek diaSemana,
        TimeOnly horaInicio,
        TimeOnly horaFim,
        CancellationToken cancellationToken)
    {
        return _context.DisponibilidadesProfissionais
            .AnyAsync(
                d => d.ProfissionalId == profissionalId
                    && d.DiaSemana == diaSemana
                    && d.HoraInicio < horaFim
                    && d.HoraFim > horaInicio,
                cancellationToken);
    }

    // NOVO: adiciona uma disponibilidade ao contexto.
    public async Task AdicionarDisponibilidadeAsync(
        DisponibilidadeProfissional disponibilidade,
        CancellationToken cancellationToken)
    {
        await _context.DisponibilidadesProfissionais.AddAsync(
            disponibilidade,
            cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<Profissional>> ListarPorServicoAsync(
    int servicoId,
    CancellationToken cancellationToken)
    {
        return await _context.Profissionais
            .AsNoTracking()
            .Where(p =>
                p.Ativo &&
                p.ProfissionalServicos.Any(
                    ps => ps.ServicoId == servicoId && ps.Servico.Ativo))
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }
}