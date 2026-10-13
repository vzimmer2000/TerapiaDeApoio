namespace TerapiaDeApoio.Api.DTOs;

public sealed record ProfissionalResponse(
    int Id,
    string Nome,
    string Especialidade,
    bool Ativo);