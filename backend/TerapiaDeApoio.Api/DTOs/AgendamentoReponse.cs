namespace TerapiaDeApoio.Api.DTOs;

public sealed record AgendamentoResponse(
    int Id,
    int PacienteId,
    int ProfissionalId,
    int ServicoId,
    DateTime DataHoraInicio,
    DateTime DataHoraFim,
    string Status);