namespace TerapiaDeApoio.Api.DTOs;

public sealed record MeuAgendamentoResponse(
    int Id,
    string Servico,
    string Profissional,
    DateTime DataHoraInicio,
    DateTime DataHoraFim,
    string Status);