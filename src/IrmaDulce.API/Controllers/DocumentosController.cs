using System.Security.Claims;
using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoService _documentoService;

    public DocumentosController(IDocumentoService documentoService)
    {
        _documentoService = documentoService;
    }

    [HttpPost("emitir")]
    public async Task<IActionResult> EmitirDocumento([FromBody] EmitirDocumentoRequest request)
    {
        try
        {
            var operadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var bytes = await _documentoService.EmitirDocumentoAsync(request, operadorId);
            return File(bytes, "application/octet-stream", $"{request.TipoDocumento}_{request.AlunoId}.txt");
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex)
        {
            // Bloqueio financeiro
            if (ex.Message.StartsWith("BLOQUEIO_FINANCEIRO"))
                return StatusCode(403, new { bloqueioFinanceiro = true, message = ex.Message.Replace("BLOQUEIO_FINANCEIRO: ", "") });
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class CronogramaController : ControllerBase
{
    private readonly ICronogramaService _cronogramaService;

    public CronogramaController(ICronogramaService cronogramaService)
    {
        _cronogramaService = cronogramaService;
    }

    [HttpGet]
    [Authorize] // Todos podem ver
    public async Task<ActionResult<IEnumerable<CronogramaResponse>>> GetByData([FromQuery] DateTime data)
    {
        var cronogramas = await _cronogramaService.GetByDataAsync(data);
        return Ok(cronogramas);
    }

    [HttpGet("docente/{docenteId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CronogramaResponse>>> GetByDocente(
        int docenteId, [FromQuery] DateTime inicio, [FromQuery] DateTime fim)
    {
        var cronogramas = await _cronogramaService.GetByDocenteAsync(docenteId, inicio, fim);
        return Ok(cronogramas);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Criar([FromBody] CronogramaRequest request)
    {
        // Verifica conflitos antes de criar (regra 9.2 — alerta, não bloqueia)
        var conflitos = await _cronogramaService.VerificarConflitosAsync(request);
        var cronograma = await _cronogramaService.CriarAsync(request);

        return Created("", new
        {
            cronograma,
            conflitos,
            temConflitos = conflitos.Any()
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> Atualizar(int id, [FromBody] CronogramaRequest request)
    {
        try
        {
            var conflitos = await _cronogramaService.VerificarConflitosAsync(request, id);
            var cronograma = await _cronogramaService.AtualizarAsync(id, request);

            return Ok(new
            {
                cronograma,
                conflitos,
                temConflitos = conflitos.Any()
            });
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(int id)
    {
        try
        {
            await _cronogramaService.DeletarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("verificar-conflitos")]
    public async Task<ActionResult<IEnumerable<ConflitoCronogramaResponse>>> VerificarConflitos(
        [FromBody] CronogramaRequest request)
    {
        var conflitos = await _cronogramaService.VerificarConflitosAsync(request);
        return Ok(conflitos);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master")]
public class ConfiguracoesController : ControllerBase
{
    private readonly IConfiguracaoService _configuracaoService;

    public ConfiguracoesController(IConfiguracaoService configuracaoService)
    {
        _configuracaoService = configuracaoService;
    }

    [HttpGet]
    public async Task<ActionResult<ConfiguracaoResponse>> Get()
    {
        var config = await _configuracaoService.GetAsync();
        return Ok(config);
    }

    [HttpPut]
    public async Task<ActionResult<ConfiguracaoResponse>> Atualizar([FromBody] ConfiguracaoRequest request)
    {
        var config = await _configuracaoService.AtualizarAsync(request);
        return Ok(config);
    }
}
