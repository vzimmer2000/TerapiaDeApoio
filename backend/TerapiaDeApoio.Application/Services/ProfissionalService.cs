using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Services;

public class ProfissionalService
{
    private readonly IProfissionalRepository _profissionalRepository;
    private readonly IServicoRepository _servicoRepository;

    public ProfissionalService(
        IProfissionalRepository profissionalRepository,
        IServicoRepository servicoRepository)
    {
        _profissionalRepository = profissionalRepository;
        _servicoRepository = servicoRepository;
    }

    public Task<IReadOnlyList<Profissional>> ListarAsync(
        CancellationToken cancellationToken)
    {
        return _profissionalRepository.ListarAsync(
            cancellationToken);
    }

    public Task<Profissional?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _profissionalRepository.ObterPorIdAsync(
            id,
            cancellationToken);
    }

    public async Task<Profissional> CadastrarAsync(
        string nome,
        string especialidade,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException(
                "O nome do profissional é obrigatório.");
        }

        if (nome.Trim().Length > 120)
        {
            throw new ArgumentException(
                "O nome deve ter no máximo 120 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(especialidade))
        {
            throw new ArgumentException(
                "A especialidade é obrigatória.");
        }

        if (especialidade.Trim().Length > 100)
        {
            throw new ArgumentException(
                "A especialidade deve ter no máximo 100 caracteres.");
        }

        var profissional = new Profissional
        {
            Nome = nome.Trim(),
            Especialidade = especialidade.Trim(),
            Ativo = true
        };

        await _profissionalRepository.AdicionarAsync(
            profissional,
            cancellationToken);

        await _profissionalRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return profissional;
    }

    public async Task AssociarServicoAsync(
        int profissionalId,
        int servicoId,
        CancellationToken cancellationToken)
    {
        var profissional =
            await _profissionalRepository.ObterPorIdAsync(
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
                "Não é possível associar serviços a um profissional inativo.");
        }

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
                "Não é possível associar um serviço inativo.");
        }

        var jaAssociado =
            await _profissionalRepository.ProfissionalOfereceServicoAsync(
                profissionalId,
                servicoId,
                cancellationToken);

        if (jaAssociado)
        {
            throw new InvalidOperationException(
                "Esse profissional já oferece o serviço informado.");
        }

        var associacao = new ProfissionalServico
        {
            ProfissionalId = profissionalId,
            ServicoId = servicoId
        };

        await _profissionalRepository.AdicionarServicoAsync(
            associacao,
            cancellationToken);

        await _profissionalRepository.SalvarAlteracoesAsync(
            cancellationToken);
    }
    public async Task<IReadOnlyList<DisponibilidadeProfissional>>
    ListarDisponibilidadesAsync(
        int profissionalId,
        CancellationToken cancellationToken)
    {
        var profissional = await _profissionalRepository.ObterPorIdAsync(
            profissionalId,
            cancellationToken);

        if (profissional is null)
        {
            throw new KeyNotFoundException(
                "Profissional não encontrado.");
        }

        return await _profissionalRepository.ListarDisponibilidadesAsync(
            profissionalId,
            cancellationToken);
    }

    public async Task<DisponibilidadeProfissional>
        CadastrarDisponibilidadeAsync(
            int profissionalId,
            DayOfWeek diaSemana,
            TimeOnly horaInicio,
            TimeOnly horaFim,
            CancellationToken cancellationToken)
    {
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
                "Não é possível cadastrar horários para um profissional inativo.");
        }

        if (!Enum.IsDefined(typeof(DayOfWeek), diaSemana))
        {
            throw new ArgumentException(
                "O dia da semana informado é inválido.");
        }

        if (horaInicio >= horaFim)
        {
            throw new ArgumentException(
                "O horário inicial deve ser anterior ao horário final.");
        }

        var existeConflito =
            await _profissionalRepository.ExisteConflitoDisponibilidadeAsync(
                profissionalId,
                diaSemana,
                horaInicio,
                horaFim,
                cancellationToken);

        if (existeConflito)
        {
            throw new InvalidOperationException(
                "O intervalo informado se sobrepõe a uma disponibilidade existente.");
        }

        var disponibilidade = new DisponibilidadeProfissional
        {
            ProfissionalId = profissionalId,
            DiaSemana = diaSemana,
            HoraInicio = horaInicio,
            HoraFim = horaFim
        };

        await _profissionalRepository.AdicionarDisponibilidadeAsync(
            disponibilidade,
            cancellationToken);

        await _profissionalRepository.SalvarAlteracoesAsync(
            cancellationToken);

        return disponibilidade;
    }
    public async Task<IReadOnlyList<Profissional>> ListarPorServicoAsync(
    int servicoId,
    CancellationToken cancellationToken)
    {
        if (servicoId <= 0)
        {
            throw new ArgumentException(
                "Informe um serviço válido.");
        }

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

        return await _profissionalRepository.ListarPorServicoAsync(
            servicoId,
            cancellationToken);
    }
}