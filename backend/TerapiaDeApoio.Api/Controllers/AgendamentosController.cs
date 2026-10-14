using Microsoft.AspNetCore.Mvc;
using System.Linq;

using TerapiaDeApoio.Api.DTOs;
using TerapiaDeApoio.Application.Services;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Api.Controllers;

[ApiController]
[Route("api/agendamentos")]
public class AgendamentosController : ControllerBase
{
    private readonly AgendamentoService _agendamentoService;

    public AgendamentosController(
        AgendamentoService agendamentoService)
    {
        _agendamentoService = agendamentoService;
    }

    [HttpPost]
    public async Task<ActionResult<AgendamentoResponse>> Criar(
        [FromBody] CriarAgendamentoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var agendamento =
                await _agendamentoService.AgendarAsync(
                    request.PacienteId,
                    request.ProfissionalId,
                    request.ServicoId,
                    request.DataHoraInicio!.Value,
                    cancellationToken);

            var resposta = MapearResposta(agendamento);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = agendamento.Id },
                resposta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Registro não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dados inválidos",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Não foi possível realizar o agendamento",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }
    [HttpGet("disponibilidade")]
    public async Task<ActionResult<IReadOnlyList<TimeOnly>>>
    ListarHorariosDisponiveis(
        [FromQuery] int profissionalId,
        [FromQuery] int servicoId,
        [FromQuery] DateOnly? data,
        CancellationToken cancellationToken)
    {
        if (data is null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Data obrigatória",
                Detail = "Informe a data para consultar os horários disponíveis.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            var horarios =
                await _agendamentoService.ListarHorariosDisponiveisAsync(
                    profissionalId,
                    servicoId,
                    data.Value,
                    cancellationToken);

            return Ok(horarios);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Registro não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Parâmetros inválidos",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Não foi possível consultar os horários",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AgendamentoResponse>> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var agendamento =
            await _agendamentoService.ObterPorIdAsync(
                id,
                cancellationToken);

        if (agendamento is null)
        {
            return NotFound();
        }

        return Ok(MapearResposta(agendamento));
    }

    [HttpPost("{id:int}/cancelar")]
    public async Task<IActionResult> Cancelar(
     int id,
     [FromBody] CancelarAgendamentoRequest request,
     CancellationToken cancellationToken)
    {
        try
        {
            await _agendamentoService.CancelarAsync(
                id,
                request.Telefone,
                cancellationToken);

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dados inválidos",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Agendamento não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Não foi possível cancelar",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    private static AgendamentoResponse MapearResposta(
        Agendamento agendamento)
    {
        return new AgendamentoResponse(
            agendamento.Id,
            agendamento.PacienteId,
            agendamento.ProfissionalId,
            agendamento.ServicoId,
            agendamento.DataHoraInicio,
            agendamento.DataHoraFim,
            agendamento.Status.ToString());
    }
    [HttpGet("meus")]
    public async Task<ActionResult<IReadOnlyList<MeuAgendamentoResponse>>>
    ListarMeusAgendamentos(
        [FromQuery] string? telefone,
        CancellationToken cancellationToken)
    {
        try
        {
            var agendamentos =
                await _agendamentoService.ListarPorTelefoneAsync(
                    telefone ?? string.Empty,
                    cancellationToken);

            var resposta = agendamentos
                .Select(a => new MeuAgendamentoResponse(
                    a.Id,
                    a.Servico.Nome,
                    a.Profissional.Nome,
                    a.DataHoraInicio,
                    a.DataHoraFim,
                    a.Status.ToString()))
                .ToList();

            return Ok(resposta);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Dados inválidos",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}