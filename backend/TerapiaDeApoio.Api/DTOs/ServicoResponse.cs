namespace TerapiaDeApoio.Api.DTOs;

public sealed record ServicoResponse(
    int Id,
    string Nome,
    string? Descricao,
    int DuracaoMinutos,
    bool Ativo);