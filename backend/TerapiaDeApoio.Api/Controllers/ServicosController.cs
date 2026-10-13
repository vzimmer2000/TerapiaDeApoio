using Microsoft.AspNetCore.Mvc;
using TerapiaDeApoio.Api.DTOs;
using TerapiaDeApoio.Application.Services;
using TerapiaDeApoio.Domain.Entities;

namespace TerapiaDeApoio.Api.Controllers;

[ApiController]
[Route("api/servicos")]
public class ServicosController : ControllerBase
{
    private readonly ServicoService _servicoService;

    public ServicosController(ServicoService servicoService)
    {
        _servicoService = servicoService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServicoResponse>>> Listar(
        CancellationToken cancellationToken)
    {
        var servicos = await _servicoService.ListarAtivosAsync(
            cancellationToken);

        return Ok(servicos.Select(MapearResposta).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServicoResponse>> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var servico = await _servicoService.ObterAtivoPorIdAsync(
            id,
            cancellationToken);

        if (servico is null)
        {
            return NotFound();
        }

        return Ok(MapearResposta(servico));
    }

    [HttpPost]
    public async Task<ActionResult<ServicoResponse>> Criar(
        [FromBody] CriarServicoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var servico = await _servicoService.CadastrarAsync(
                request.Nome,
                request.Descricao,
                request.DuracaoMinutos,
                cancellationToken);

            var resposta = MapearResposta(servico);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = servico.Id },
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

    private static ServicoResponse MapearResposta(Servico servico)
    {
        return new ServicoResponse(
            servico.Id,
            servico.Nome,
            servico.Descricao,
            servico.DuracaoMinutos,
            servico.Ativo);
    }
}