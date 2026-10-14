using System.Linq;

using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Services;

public class AgendamentoService

{
    private const int CapacidadeMaximaSimultanea = 3;
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
        var agendamento = new Agendamento
        {
            PacienteId = pacienteId,
            ProfissionalId = profissionalId,
            ServicoId = servicoId,
            DataHoraInicio = inicio,
            DataHoraFim = fim
        };

        var foiAdicionado =
            await _agendamentoRepository.AdicionarSeHouverCapacidadeAsync(
                agendamento,
                CapacidadeMaximaSimultanea,
                cancellationToken);

        if (!foiAdicionado)
        {
            throw new InvalidOperationException(
                "O profissional já atingiu a capacidade de três pacientes simultâneos nesse intervalo.");
        }

        return agendamento;

    }

    public async Task CancelarAsync(
      int agendamentoId,
      string telefone,
      CancellationToken cancellationToken = default)
    {
        var telefoneNormalizado = NormalizarTelefone(telefone);

        if (telefoneNormalizado.Length < 8)
        {
            throw new ArgumentException(
                "Informe um telefone válido.");
        }

        var agendamento = await _agendamentoRepository.ObterPorIdAsync(
            agendamentoId,
            cancellationToken);

        if (agendamento is null)
        {
            throw new KeyNotFoundException(
                "Agendamento não encontrado para os dados informados.");
        }

        var paciente = await _pacienteRepository.ObterPorIdAsync(
            agendamento.PacienteId,
            cancellationToken);

        if (paciente is null ||
            NormalizarTelefone(paciente.Telefone) != telefoneNormalizado)
        {
            throw new KeyNotFoundException(
                "Agendamento não encontrado para os dados informados.");
        }

        agendamento.Cancelar();

        await _agendamentoRepository.SalvarAlteracoesAsync(
            cancellationToken);
    }

    public Task<Agendamento?> ObterPorIdAsync(
    int id,
    CancellationToken cancellationToken)
    {
        return _agendamentoRepository.ObterPorIdAsync(
            id,
            cancellationToken);
    }

    public async Task<IReadOnlyList<TimeOnly>> ListarHorariosDisponiveisAsync(
    int profissionalId,
    int servicoId,
    DateOnly data,
    CancellationToken cancellationToken = default)
    {
        if (profissionalId <= 0 || servicoId <= 0)
        {
            throw new ArgumentException(
                "Informe um profissional e um serviço válidos.");
        }

        var hoje = DateOnly.FromDateTime(DateTime.Now);

        if (data < hoje)
        {
            throw new ArgumentException(
                "Não é possível consultar horários de uma data passada.");
        }

        // 1. Verificar o profissional.
        var profissional = await _profissionalRepository.ObterPorIdAsync(
            profissionalId,
            cancellationToken);

        if (profissional is null)
        {
            throw new KeyNotFoundException(
                "Profissional não encontrado.");
        }

        if (!profissional.Ativo)
        {
            throw new InvalidOperationException(
                "O profissional está inativo.");
        }

        // 2. Verificar o serviço.
        var servico = await _servicoRepository.ObterPorIdAsync(
            servicoId,
            cancellationToken);

        if (servico is null)
        {
            throw new KeyNotFoundException(
                "Serviço não encontrado.");
        }

        if (!servico.Ativo)
        {
            throw new InvalidOperationException(
                "O serviço está inativo.");
        }

        if (servico.DuracaoMinutos <= 0)
        {
            throw new InvalidOperationException(
                "A duração do serviço é inválida.");
        }

        // 3. Verificar se o profissional oferece o serviço.
        var ofereceServico =
            await _profissionalRepository.ProfissionalOfereceServicoAsync(
                profissionalId,
                servicoId,
                cancellationToken);

        if (!ofereceServico)
        {
            throw new InvalidOperationException(
                "O profissional não oferece esse serviço.");
        }

        // 4. Buscar os intervalos de disponibilidade.
        var disponibilidades =
            await _profissionalRepository.ListarDisponibilidadesAsync(
                profissionalId,
                cancellationToken);

        var horariosDisponiveis = new List<TimeOnly>();

        // Os horários iniciais serão sugeridos em intervalos de 30 minutos.
        const int intervaloMinutos = 30;

        foreach (var disponibilidade in disponibilidades
                     .Where(d => d.DiaSemana == data.DayOfWeek)
                     .OrderBy(d => d.HoraInicio))
        {
            var inicioIntervalo = disponibilidade.HoraInicio.ToTimeSpan();
            var fimIntervalo = disponibilidade.HoraFim.ToTimeSpan();

            var candidato = inicioIntervalo;

            // Garantir que o atendimento termine dentro da disponibilidade.
            while (candidato + TimeSpan.FromMinutes(servico.DuracaoMinutos)
                   <= fimIntervalo)
            {
                var horarioInicio = TimeOnly.FromTimeSpan(candidato);

                var horarioFim = TimeOnly.FromTimeSpan(
                    candidato.Add(TimeSpan.FromMinutes(servico.DuracaoMinutos)));

                var inicio = data.ToDateTime(
                    horarioInicio,
                    DateTimeKind.Unspecified);

                var fim = data.ToDateTime(
                    horarioFim,
                    DateTimeKind.Unspecified);

                // Não oferecer horários que já passaram.
                if (inicio > DateTime.Now)
                {
                    var possuiCapacidade =
    await _agendamentoRepository.PossuiCapacidadeAsync(
        profissionalId,
        inicio,
        fim,
        CapacidadeMaximaSimultanea,
        cancellationToken);

                    if (possuiCapacidade)
                    {
                        horariosDisponiveis.Add(horarioInicio);
                    }
                }

                candidato = candidato.Add(
                    TimeSpan.FromMinutes(intervaloMinutos));
            }
        }

        return horariosDisponiveis
            .Distinct()
            .OrderBy(h => h)
            .ToList();
    }
    public async Task<IReadOnlyList<Agendamento>> ListarPorTelefoneAsync(
    string telefone,
    CancellationToken cancellationToken = default)
    {
        var telefoneNormalizado = NormalizarTelefone(telefone);

        if (telefoneNormalizado.Length < 8)
        {
            throw new ArgumentException(
                "Informe um telefone válido para consultar os agendamentos.");
        }

        return await _agendamentoRepository.ListarPorTelefoneAsync(
            telefoneNormalizado,
            cancellationToken);
    }

    private static string NormalizarTelefone(string telefone)
    {
        return new string(
            telefone.Where(char.IsDigit).ToArray());
    }
    public async Task<IReadOnlyList<Agendamento>> ListarAgendaAsync(
    DateOnly data,
    int? profissionalId,
    CancellationToken cancellationToken = default)
    {
        if (profissionalId.HasValue && profissionalId.Value <= 0)
        {
            throw new ArgumentException(
                "O ID do profissional deve ser válido.");
        }

        if (profissionalId.HasValue)
        {
            var profissional = await _profissionalRepository.ObterPorIdAsync(
                profissionalId.Value,
                cancellationToken);

            if (profissional is null)
            {
                throw new KeyNotFoundException(
                    "Profissional não encontrado.");
            }
        }

        return await _agendamentoRepository.ListarAgendaAsync(
            data,
            profissionalId,
            cancellationToken);
    }
}