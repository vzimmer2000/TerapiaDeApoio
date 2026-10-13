using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Services;

public class ServicoService
{
    private readonly IServicoRepository _repository;

    public ServicoService(IServicoRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Servico>> ListarAtivosAsync(
        CancellationToken cancellationToken)
    {
        return _repository.ListarAtivosAsync(cancellationToken);
    }

    public async Task<Servico?> ObterAtivoPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var servico = await _repository.ObterPorIdAsync(
            id,
            cancellationToken);

        return servico is { Ativo: true } ? servico : null;
    }

    public async Task<Servico> CadastrarAsync(
        string nome,
        string? descricao,
        int duracaoMinutos,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException(
                "O nome do serviço é obrigatório.");
        }

        if (duracaoMinutos <= 0)
        {
            throw new ArgumentException(
                "A duração deve ser maior que zero.");
        }

        var servico = new Servico
        {
            Nome = nome.Trim(),
            Descricao = string.IsNullOrWhiteSpace(descricao)
                ? null
                : descricao.Trim(),
            DuracaoMinutos = duracaoMinutos,
            Ativo = true
        };

        await _repository.AdicionarAsync(
            servico,
            cancellationToken);

        await _repository.SalvarAlteracoesAsync(
            cancellationToken);

        return servico;
    }
}