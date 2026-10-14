namespace TerapiaDeApoio.Api.DTOs;

public sealed record AgendaClinicaResponse(
    int Id,
    string Paciente,
    string Telefone,
    string Profissional,
    string Servico,
    DateTime Inicio,
    DateTime Fim,
    string Status);