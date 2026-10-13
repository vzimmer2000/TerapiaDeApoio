namespace TerapiaDeApoio.Api.DTOs;

public sealed record DisponibilidadeResponse(
    int Id,
    string DiaSemana,
    TimeOnly HoraInicio,
    TimeOnly HoraFim);