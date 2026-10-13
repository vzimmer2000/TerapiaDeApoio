using Microsoft.AspNetCore.Mvc;
using TerapiaDeApoio.Api.DTOs;
using TerapiaDeApoio.Application.Services;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Api.Controllers;

[ApiController]
[Route("api/pacientes")]
public class PacientesController : ControllerBase
{
    private readonly PacienteService _pacienteService;

    public PacientesController(PacienteService pacienteService)
    {
        _pacienteService = pacienteService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PacienteResponse>>> Listar(
        CancellationToken cancellationToken)
    {
        var pacientes = await _pacienteService.ListarAsync(
            cancellationToken);

        return Ok(pacientes.Select(MapearResposta).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PacienteResponse>> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var paciente = await _pacienteService.ObterPorIdAsync(
            id,
            cancellationToken);

        if (paciente is null)
        {
            return NotFound();
        }

        return Ok(MapearResposta(paciente));
    }

    [HttpPost]
    public async Task<ActionResult<PacienteResponse>> Criar(
        [FromBody] CriarPacienteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var paciente = await _pacienteService.CadastrarAsync(
                request.Nome,
                request.Telefone,
                request.Email,
                cancellationToken);

            var resposta = MapearResposta(paciente);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = paciente.Id },
                resposta);
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

    private static PacienteResponse MapearResposta(Paciente paciente)
    {
        return new PacienteResponse(
            paciente.Id,
            paciente.Nome,
            paciente.Telefone,
            paciente.Email);
    }
}