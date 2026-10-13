using System.ComponentModel.DataAnnotations;
using TerapiaDeApoio.Application.Interfaces;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Application.Services;

public class PacienteService
{
    private readonly IPacienteRepository _repository;

    public PacienteService(IPacienteRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Paciente>> ListarAsync(
        CancellationToken cancellationToken)
    {
        return _repository.ListarAsync(cancellationToken);
    }

    public Task<Paciente?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return _repository.ObterPorIdAsync(
            id,
            cancellationToken);
    }

    public async Task<Paciente> CadastrarAsync(
        string nome,
        string telefone,
        string? email,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new ArgumentException(
                "O nome do paciente é obrigatório.");
        }

        nome = nome.Trim();

        if (nome.Length > 120)
        {
            throw new ArgumentException(
                "O nome deve ter no máximo 120 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(telefone))
        {
            throw new ArgumentException(
                "O telefone do paciente é obrigatório.");
        }

        telefone = telefone.Trim();

        if (telefone.Length > 30)
        {
            throw new ArgumentException(
                "O telefone deve ter no máximo 30 caracteres.");
        }

        email = string.IsNullOrWhiteSpace(email)
            ? null
            : email.Trim();

        if (email is not null)
        {
            if (email.Length > 254 ||
                !new EmailAddressAttribute().IsValid(email))
            {
                throw new ArgumentException(
                    "O endereço de e-mail informado é inválido.");
            }
        }

        var paciente = new Paciente
        {
            Nome = nome,
            Telefone = telefone,
            Email = email
        };

        await _repository.AdicionarAsync(
            paciente,
            cancellationToken);

        await _repository.SalvarAlteracoesAsync(
            cancellationToken);

        return paciente;
    }
}