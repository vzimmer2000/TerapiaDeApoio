using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task<bool> PossuiCapacidadeAsync(
        int profissionalId,
        DateTime inicio,
        DateTime fim,
        int capacidadeMaxima,
        CancellationToken cancellationToken)
    {
        ValidarIntervalo(inicio, fim, capacidadeMaxima);

        var intervalos = await ConsultarIntervalosSobrepostosAsync(
            profissionalId,
            inicio,
            fim,
            cancellationToken);

        return AvaliarCapacidade(
            inicio,
            fim,
            capacidadeMaxima,
            intervalos);
    }

    public async Task<bool> AdicionarSeHouverCapacidadeAsync(
        Agendamento agendamento,
        int capacidadeMaxima,
        CancellationToken cancellationToken)
    {
        ValidarIntervalo(
            agendamento.DataHoraInicio,
            agendamento.DataHoraFim,
            capacidadeMaxima);

        // A transação mantém o bloqueio ativo até confirmar a gravação.
        await using var transacao =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        // Serializa tentativas de agendamento para o mesmo profissional.
        // A chave do bloqueio é o ID do profissional.
        var conexao = _context.Database.GetDbConnection();

        await using (var comando = conexao.CreateCommand())
        {
            comando.Transaction = transacao.GetDbTransaction();

            comando.CommandText =
                "SELECT pg_advisory_xact_lock(CAST(@profissionalId AS bigint));";

            var parametro = comando.CreateParameter();
            parametro.ParameterName = "profissionalId";
            parametro.Value = agendamento.ProfissionalId;

            comando.Parameters.Add(parametro);

            await comando.ExecuteScalarAsync(cancellationToken);
        }

        // Verifica novamente a capacidade depois de obter o bloqueio.
        var intervalos = await ConsultarIntervalosSobrepostosAsync(
            agendamento.ProfissionalId,
            agendamento.DataHoraInicio,
            agendamento.DataHoraFim,
            cancellationToken);

        var possuiCapacidade = AvaliarCapacidade(
            agendamento.DataHoraInicio,
            agendamento.DataHoraFim,
            capacidadeMaxima,
            intervalos);

        if (!possuiCapacidade)
        {
            await transacao.RollbackAsync(cancellationToken);
            return false;
        }

        await _context.Agendamentos.AddAsync(
            agendamento,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        await transacao.CommitAsync(cancellationToken);

        return true;
    }

    public Task<Agendamento?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _context.Agendamentos
            .FirstOrDefaultAsync(
                a => a.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Agendamento>> ListarPorTelefoneAsync(
        string telefoneNormalizado,
        CancellationToken cancellationToken)
    {
        return await _context.Agendamentos
            .AsNoTracking()
            .Include(a => a.Servico)
            .Include(a => a.Profissional)
            .Where(a =>
                a.Paciente.Telefone
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("-", "")
                    .Replace(" ", "")
                    .Replace("+", "")
                    == telefoneNormalizado)
            .OrderByDescending(a => a.DataHoraInicio)
            .ToListAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<IntervaloAgendamento>>
        ConsultarIntervalosSobrepostosAsync(
            int profissionalId,
            DateTime inicio,
            DateTime fim,
            CancellationToken cancellationToken)
    {
        return await _context.Agendamentos
            .AsNoTracking()
            .Where(a =>
                a.ProfissionalId == profissionalId
                && a.Status == StatusAgendamento.Agendado
                && a.DataHoraInicio < fim
                && a.DataHoraFim > inicio)
            .Select(a => new IntervaloAgendamento
            {
                Inicio = a.DataHoraInicio,
                Fim = a.DataHoraFim
            })
            .ToListAsync(cancellationToken);
    }

    private static bool AvaliarCapacidade(
        DateTime inicio,
        DateTime fim,
        int capacidadeMaxima,
        IReadOnlyCollection<IntervaloAgendamento> intervalos)
    {
        // A capacidade é avaliada no início do novo atendimento
        // e em cada início de atendimento que ocorra dentro dele.
        var momentosParaVerificar = intervalos
            .Where(i => i.Inicio > inicio && i.Inicio < fim)
            .Select(i => i.Inicio)
            .Append(inicio)
            .Distinct()
            .OrderBy(momento => momento);

        foreach (var momento in momentosParaVerificar)
        {
            var atendimentosSimultaneos = intervalos.Count(
                i => i.Inicio <= momento && i.Fim > momento);

            // O novo agendamento ocupará mais uma vaga.
            if (atendimentosSimultaneos >= capacidadeMaxima)
            {
                return false;
            }
        }

        return true;
    }

    private static void ValidarIntervalo(
        DateTime inicio,
        DateTime fim,
        int capacidadeMaxima)
    {
        if (inicio >= fim)
        {
            throw new ArgumentException(
                "O término do atendimento deve ser posterior ao início.");
        }

        if (capacidadeMaxima <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacidadeMaxima),
                "A capacidade máxima deve ser maior que zero.");
        }
    }

    private sealed class IntervaloAgendamento
    {
        public DateTime Inicio { get; set; }

        public DateTime Fim { get; set; }
    }
}