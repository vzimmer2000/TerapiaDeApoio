using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Services;

public class AgendamentoService
{
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IProfissionalRepository _profissionalRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IAgendamentoRepository _agendamentoRepository;

    public AgendamentoService(
        IPacienteRepository pacienteRepository,
        IProfissionalRepository profissionalRepository,
        IServicoRepository servicoRepository,
        IAgendamentoRepository agendamentoRepository)
    {
        _pacienteRepository = pacienteRepository;
        _profissionalRepository = profissionalRepository;
        _servicoRepository = servicoRepository;
        _agendamentoRepository = agendamentoRepository;
    }

    public async Task<Agendamento> AgendarAsync(
        int pacienteId,
        int profissionalId,
        int servicoId,
        DateTime dataHoraInicio,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar o horário solicitado.
        if (dataHoraInicio.Kind == DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Informe o horário local da clínica, sem conversão UTC.");
        }

        var inicio = DateTime.SpecifyKind(
            dataHoraInicio,
            DateTimeKind.Unspecified);

        if (inicio <= DateTime.Now)
        {
            throw new InvalidOperationException(
                "O agendamento deve ocorrer em uma data futura.");
        }

        // 2. Buscar e validar o paciente.
        var paciente = await _pacienteRepository.ObterPorIdAsync(
            pacienteId,
            cancellationToken);

        if (paciente is null)
        {
            throw new KeyNotFoundException(
                "Paciente não encontrado.");
        }

        // 3. Buscar e validar o serviço.
        var servico = await _servicoRepository.ObterPorIdAsync(
            servicoId,
            cancellationToken);

        if (servico is null || !servico.Ativo)
        {
            throw new InvalidOperationException(
                "O serviço não existe ou não está ativo.");
        }

        if (servico.DuracaoMinutos <= 0)
        {
            throw new InvalidOperationException(
                "A duração do serviço deve ser maior que zero.");
        }

        // 4. Buscar e validar o profissional.
        var profissional =
            await _profissionalRepository.ProfissionalOfereceServicoAsync(
                profissionalId,
                servicoId,
                cancellationToken);

        if (!profissional)
        {
            throw new InvalidOperationException(
                "O profissional não oferece esse serviço.");
        }

        // 5. Calcular o horário final.
        var fim = inicio.AddMinutes(servico.DuracaoMinutos);

        if (inicio.Date != fim.Date)
        {
            throw new InvalidOperationException(
                "O atendimento não pode ultrapassar a meia-noite.");
        }

        // 6. Verificar a disponibilidade do profissional.
        var estaDisponivel =
            await _profissionalRepository.EstaDentroDaDisponibilidadeAsync(
                profissionalId,
                inicio.DayOfWeek,
                TimeOnly.FromDateTime(inicio),
                TimeOnly.FromDateTime(fim),
                cancellationToken);

        if (!estaDisponivel)
        {
            throw new InvalidOperationException(
                "O horário está fora da disponibilidade do profissional.");
        }

        // 7. Impedir sobreposição com outro atendimento.
        var existeConflito =
            await _agendamentoRepository.ExisteConflitoAsync(
                profissionalId,
                inicio,
                fim,
                cancellationToken);

        if (existeConflito)
        {
            throw new InvalidOperationException(
                "O profissional já possui um atendimento nesse intervalo.");
        }

        // 8. Criar e persistir o agendamento.
        var agendamento = new Agendamento
        {
            PacienteId = pacienteId,
            ProfissionalId = profissionalId,
            ServicoId = servicoId,
            DataHoraInicio = inicio,
            DataHoraFim = fim
        };

        await _agendamentoRepository.AdicionarAsync(
            agendamento,
            cancellationToken);

        await _agendamentoRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return agendamento;
    }

    public async Task CancelarAsync(
        int agendamentoId,
        CancellationToken cancellationToken = default)
    {
        var agendamento = await _agendamentoRepository.ObterPorIdAsync(
            agendamentoId,
            cancellationToken);

        if (agendamento is null)
        {
            throw new KeyNotFoundException(
                "Agendamento não encontrado.");
        }

        agendamento.Cancelar();

        await _agendamentoRepository.SalvarAlteracoesAsync(
            cancellationToken);
    }
}