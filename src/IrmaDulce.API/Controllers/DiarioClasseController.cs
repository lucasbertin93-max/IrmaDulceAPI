using System.Security.Claims;
using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo,Docente")]
public class DiarioClasseController : ControllerBase
{
    private readonly IDiarioClasseService _diarioService;

    public DiarioClasseController(IDiarioClasseService diarioService)
    {
        _diarioService = diarioService;
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarAula([FromBody] DiarioClasseRequest request)
    {
        var diarioId = await _diarioService.RegistrarAulaAsync(request);
        return Created("", new { id = diarioId });
    }

    [HttpPost("{diarioId}/presencas")]
    public async Task<IActionResult> RegistrarPresencas(int diarioId, [FromBody] List<PresencaRequest> presencas)
    {
        await _diarioService.RegistrarPresencasAsync(diarioId, presencas);
        return Ok(new { message = "Presenças registradas." });
    }

    [HttpPost("notas")]
    public async Task<IActionResult> LancarNota([FromBody] NotaRequest request)
    {
        await _diarioService.LancarNotaAsync(request);
        return Ok(new { message = "Nota lançada." });
    }

    [HttpGet("notas/{alunoId}")]
    [Authorize] // Aluno pode ver suas próprias notas
    public async Task<ActionResult<IEnumerable<NotaAlunoResumo>>> GetNotas(
        int alunoId, [FromQuery] int turmaId, [FromQuery] int disciplinaId)
    {
        var notas = await _diarioService.GetNotasByAlunoAsync(alunoId, turmaId, disciplinaId);
        return Ok(notas);
    }

    [HttpGet("media/{alunoId}")]
    [Authorize]
    public async Task<ActionResult<object>> GetMedia(
        int alunoId, [FromQuery] int turmaId, [FromQuery] int disciplinaId)
    {
        var media = await _diarioService.CalcularMediaAsync(alunoId, turmaId, disciplinaId);
        return Ok(new { alunoId, turmaId, disciplinaId, media });
    }

    [HttpGet("frequencia/{alunoId}")]
    [Authorize]
    public async Task<ActionResult<object>> GetFrequencia(
        int alunoId, [FromQuery] int turmaId, [FromQuery] int disciplinaId)
    {
        var frequencia = await _diarioService.CalcularFrequenciaAsync(alunoId, turmaId, disciplinaId);
        return Ok(new { alunoId, turmaId, disciplinaId, frequencia });
    }

    [HttpGet("aprovado/{alunoId}")]
    [Authorize]
    public async Task<ActionResult<object>> VerificarAprovacao(
        int alunoId, [FromQuery] int turmaId, [FromQuery] int disciplinaId)
    {
        var media = await _diarioService.CalcularMediaAsync(alunoId, turmaId, disciplinaId);
        var frequencia = await _diarioService.CalcularFrequenciaAsync(alunoId, turmaId, disciplinaId);
        var aprovado = await _diarioService.AlunoAprovadoAsync(alunoId, turmaId, disciplinaId);
        return Ok(new { alunoId, turmaId, disciplinaId, media, frequencia, aprovado });
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo,Docente")]
public class AvaliacoesController : ControllerBase
{
    private readonly IAvaliacaoService _avaliacaoService;

    public AvaliacoesController(IAvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvaliacaoResponse>>> GetByTurmaDisciplina(
        [FromQuery] int turmaId, [FromQuery] int disciplinaId)
    {
        var avaliacoes = await _avaliacaoService.GetByTurmaAndDisciplinaAsync(turmaId, disciplinaId);
        return Ok(avaliacoes);
    }

    [HttpPost]
    public async Task<ActionResult<AvaliacaoResponse>> Criar([FromBody] AvaliacaoRequest request)
    {
        var avaliacao = await _avaliacaoService.CriarAsync(request);
        return Created("", avaliacao);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AvaliacaoResponse>> Atualizar(int id, [FromBody] AvaliacaoRequest request)
    {
        try
        {
            var avaliacao = await _avaliacaoService.AtualizarAsync(id, request);
            return Ok(avaliacao);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        try
        {
            await _avaliacaoService.DeletarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
