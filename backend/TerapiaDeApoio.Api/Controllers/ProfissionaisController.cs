using System;
using System.Linq;
using TerapiaDeApoio.Api.DTOs;
using TerapiaDeApoio.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using TerapiaDeApoio.Application.Services;


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
        [FromQuery] int? servicoId,
        CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<Profissional> profissionais;

            if (servicoId.HasValue)
            {
                profissionais =
                    await _profissionalService.ListarPorServicoAsync(
                        servicoId.Value,
                        cancellationToken);
            }
            else
            {
                profissionais =
                    await _profissionalService.ListarAsync(
                        cancellationToken);
            }

            return Ok(profissionais.Select(MapearResposta).ToList());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Serviço não encontrado",
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
                Title = "Serviço indisponível",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
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
    [HttpGet("{id:int}/disponibilidades")]
    public async Task<
     ActionResult<IReadOnlyList<DisponibilidadeResponse>>>
     ListarDisponibilidades(
         int id,
         CancellationToken cancellationToken)
    {
        try
        {
            var disponibilidades =
                await _profissionalService.ListarDisponibilidadesAsync(
                    id,
                    cancellationToken);

            var resposta = disponibilidades
                .Select(MapearDisponibilidade)
                .ToList();

            return Ok(resposta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Profissional não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

    [HttpPost("{id:int}/disponibilidades")]
    public async Task<IActionResult> CadastrarDisponibilidade(
        int id,
        [FromBody] CriarDisponibilidadeRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var disponibilidade =
                await _profissionalService.CadastrarDisponibilidadeAsync(
                    id,
                    (DayOfWeek)request.DiaSemana!.Value,
                    request.HoraInicio!.Value,
                    request.HoraFim!.Value,
                    cancellationToken);

            var resposta = MapearDisponibilidade(disponibilidade);

            return CreatedAtAction(
                nameof(ListarDisponibilidades),
                new { id },
                resposta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Profissional não encontrado",
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
                Title = "Conflito de disponibilidade",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    private static DisponibilidadeResponse MapearDisponibilidade(
        DisponibilidadeProfissional disponibilidade)
    {
        var dia = disponibilidade.DiaSemana switch
        {
            DayOfWeek.Sunday => "Domingo",
            DayOfWeek.Monday => "Segunda-feira",
            DayOfWeek.Tuesday => "Terça-feira",
            DayOfWeek.Wednesday => "Quarta-feira",
            DayOfWeek.Thursday => "Quinta-feira",
            DayOfWeek.Friday => "Sexta-feira",
            DayOfWeek.Saturday => "Sábado",
            _ => "Desconhecido"
        };

        return new DisponibilidadeResponse(
            disponibilidade.Id,
            dia,
            disponibilidade.HoraInicio,
            disponibilidade.HoraFim);
    }
    [HttpGet("{id:int}/servicos")]
    public async Task<ActionResult<IReadOnlyList<ServicoResponse>>>
    ListarServicos(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var servicos = await _profissionalService.ListarServicosAsync(
                id,
                cancellationToken);

            var resposta = servicos
                .Select(s => new ServicoResponse(
                    s.Id,
                    s.Nome,
                    s.Descricao,
                    s.DuracaoMinutos,
                    s.Ativo))
                .ToList();

            return Ok(resposta);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Profissional não encontrado",
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound
            });
        }
    }

}