using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo,Docente")]
public class CronogramaController : ControllerBase
{
    private readonly ICronogramaService _cronogramaService;

    public CronogramaController(ICronogramaService cronogramaService)
    {
        _cronogramaService = cronogramaService;
    }

    [HttpPost]
    public async Task<ActionResult<CronogramaResponse>> Criar(CronogramaRequest request)
    {
        var result = await _cronogramaService.CriarAsync(request);
        return CreatedAtAction(nameof(GetByData), new { data = result.Data.ToString("yyyy-MM-dd") }, result);
    }

    [HttpGet("data/{data}")]
    public async Task<ActionResult<IEnumerable<CronogramaResponse>>> GetByData(DateTime data)
    {
        var result = await _cronogramaService.GetByDataAsync(data);
        return Ok(result);
    }

    [HttpGet("docente/{docenteId}")]
    public async Task<ActionResult<IEnumerable<CronogramaResponse>>> GetByDocente(int docenteId, [FromQuery] DateTime inicio, [FromQuery] DateTime fim)
    {
        var result = await _cronogramaService.GetByDocenteAsync(docenteId, inicio, fim);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CronogramaResponse>> Atualizar(int id, CronogramaRequest request)
    {
        var result = await _cronogramaService.AtualizarAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Deletar(int id)
    {
        await _cronogramaService.DeletarAsync(id);
        return NoContent();
    }

    [HttpPost("conflitos")]
    public async Task<ActionResult<IEnumerable<ConflitoCronogramaResponse>>> VerificarConflitos(CronogramaRequest request, [FromQuery] int? excludeId = null)
    {
        var result = await _cronogramaService.VerificarConflitosAsync(request, excludeId);
        return Ok(result);
    }

    [HttpPost("gerar-lote")]
    public async Task<ActionResult<CronogramaGerarLoteResponse>> GerarLote(CronogramaGerarLoteRequest request)
    {
        var result = await _cronogramaService.GerarLoteAsync(request.TurmaId, request.DataInicioLote, request.DataFimLote);
        return Ok(result);
    }
}
