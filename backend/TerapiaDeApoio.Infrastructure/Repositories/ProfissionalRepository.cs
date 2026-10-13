using Microsoft.EntityFrameworkCore;
using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Infrastructure.Data;

namespace TerapiaDeApoio.Infrastructure.Repositories;

public class ProfissionalRepository : IProfissionalRepository
{
    private readonly TerapiaDeApoioDbContext _context;

    public ProfissionalRepository(TerapiaDeApoioDbContext context)
    {
        _context = context;
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
}