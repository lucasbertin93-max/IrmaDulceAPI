using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class TemplatesController : ControllerBase
{
    private readonly ITemplateService _templateService;
    private readonly IWebHostEnvironment _env;

    public TemplatesController(ITemplateService templateService, IWebHostEnvironment env)
    {
        _templateService = templateService;
        _env = env;
    }

    [HttpGet("{tipo}")]
    public async Task<ActionResult<TemplateDocumentoResponse>> GetByTipo(TipoDocumento tipo)
    {
        var template = await _templateService.GetTemplateByTipoAsync(tipo);
        if (template == null)
            return NotFound(new { message = "Template não encontrado para este tipo." });

        return Ok(template);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<TemplateDocumentoResponse>> Upload([FromForm] TipoDocumento tipoDocumento, IFormFile arquivo)
    {
        try
        {
            if (arquivo == null || arquivo.Length == 0)
                throw new ArgumentException("Arquivo não enviado.");

            using var ms = new MemoryStream();
            await arquivo.CopyToAsync(ms);

            var req = new UploadTemplateRequest
            {
                TipoDocumento = tipoDocumento,
                ArquivoBytes = ms.ToArray(),
                NomeArquivo = arquivo.FileName
            };

            var result = await _templateService.UploadTemplateAsync(req, _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao fazer upload do template.", details = ex.Message });
        }
    }

    [HttpPost("tags")]
    public async Task<ActionResult<TemplateDocumentoResponse>> SaveTags([FromBody] SaveTagsRequest request)
    {
        try
        {
            var result = await _templateService.SaveTagsAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erro ao salvar tags do template.", details = ex.Message });
        }
    }
}
