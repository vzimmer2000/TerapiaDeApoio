namespace TerapiaDeApoio.Api.DTOs;

public sealed record PacienteResponse(
    int Id,
    string Nome,
    string Telefone,
    string? Email);