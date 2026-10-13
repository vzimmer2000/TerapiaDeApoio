using Microsoft.AspNetCore.Mvc;
using TerapiaDeApoio.Api.DTOs;
using TerapiaDeApoio.Application.Services;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Api.Controllers;

[ApiController]
[Route("api/profissionais")]
public class ProfissionaisController : ControllerBase
{
    private readonly ProfissionalService _profissionalService;

    public ProfissionaisController(
        ProfissionalService profissionalService)
    {
        _profissionalService = profissionalService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProfissionalResponse>>> Listar(
        CancellationToken cancellationToken)
    {
        var profissionais = await _profissionalService.ListarAsync(
            cancellationToken);

        return Ok(profissionais.Select(MapearResposta).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProfissionalResponse>> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var profissional = await _profissionalService.ObterPorIdAsync(
            id,
            cancellationToken);

        if (profissional is null)
        {
            return NotFound();
        }

        return Ok(MapearResposta(profissional));
    }

    [HttpPost]
    public async Task<ActionResult<ProfissionalResponse>> Criar(
        [FromBody] CriarProfissionalRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var profissional =
                await _profissionalService.CadastrarAsync(
                    request.Nome,
                    request.Especialidade,
                    cancellationToken);

            var resposta = MapearResposta(profissional);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = profissional.Id },
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

    [HttpPost("{id:int}/servicos")]
    public async Task<IActionResult> AssociarServico(
        int id,
        [FromBody] AssociarServicoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _profissionalService.AssociarServicoAsync(
                id,
                request.ServicoId,
                cancellationToken);

            return NoContent();
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Não foi possível associar o serviço",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    private static ProfissionalResponse MapearResposta(
        Profissional profissional)
    {
        return new ProfissionalResponse(
            profissional.Id,
            profissional.Nome,
            profissional.Especialidade,
            profissional.Ativo);
    }
}