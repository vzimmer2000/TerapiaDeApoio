using Microsoft.EntityFrameworkCore;
using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Domain.Entities;
using TerapiaDeApoio.Domain.Enums;
using TerapiaDeApoio.Infrastructure.Data;

namespace TerapiaDeApoio.Infrastructure.Repositories;

public class AgendamentoRepository : IAgendamentoRepository
{
    private readonly TerapiaDeApoioDbContext _context;

    public AgendamentoRepository(
        TerapiaDeApoioDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExisteConflitoAsync(
        int profissionalId,
        DateTime inicio,
        DateTime fim,
        CancellationToken cancellationToken)
    {
        return _context.Agendamentos
            .AnyAsync(
                a => a.ProfissionalId == profissionalId
                    && a.Status == StatusAgendamento.Agendado
                    && a.DataHoraInicio < fim
                    && a.DataHoraFim > inicio,
                cancellationToken);
    }

    public Task<Agendamento?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        // Não usar AsNoTracking aqui:
        // precisamos alterar o status do agendamento.
        return _context.Agendamentos
            .FirstOrDefaultAsync(
                a => a.Id == id,
                cancellationToken);
    }

    public async Task AdicionarAsync(
        Agendamento agendamento,
        CancellationToken cancellationToken)
    {
        await _context.Agendamentos.AddAsync(
            agendamento,
            cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}