using System.Security.Claims;
using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.API.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class DocumentosController : ControllerBase
{
    private readonly IDocumentoService _documentoService;
    private readonly DocumentProcessingChannel _channel;
    private readonly DocumentJobCache _jobCache;

    public DocumentosController(IDocumentoService documentoService, DocumentProcessingChannel channel, DocumentJobCache jobCache)
    {
        _documentoService = documentoService;
        _channel = channel;
        _jobCache = jobCache;
    }

    [HttpPost("emitir")]
    public async Task<IActionResult> EmitirDocumento([FromBody] EmitirDocumentoRequest request)
    {
        Console.WriteLine($"[DEBUG] Enfileirando emissão -> Aluno: {request.AlunoId} | Tipo: {request.TipoDocumento}");
        try
        {
            var operadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            // Generate JobId and insert to Cache
            var jobId = Guid.NewGuid();
            _jobCache.AddJob(jobId);

            var job = new DocumentJob
            {
                JobId = jobId,
                Request = request,
                OperadorId = operadorId
            };

            await _channel.AddJobAsync(job);

            return Accepted(new { message = "Processamento iniciado", jobId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"[DEV] {ex.Message} -> {ex.InnerException?.Message}" });
        }
    }

    [HttpGet("status/{jobId}")]
    public IActionResult ConsultarStatus(Guid jobId)
    {
        var status = _jobCache.GetStatus(jobId);
        if (status == null) return NotFound(new { message = "Job não encontrado." });

        return Ok(new
        {
            jobId = status.JobId,
            status = status.Status,
            error = status.ErrorMessage,
            isCompleted = status.FileBytes != null
        });
    }

    [HttpGet("download/{jobId}")]
    public IActionResult Download(Guid jobId)
    {
        var status = _jobCache.GetStatus(jobId);
        if (status == null || status.FileBytes == null) return NotFound(new { message = "Arquivo não encontrado ou ainda em processamento." });

        _jobCache.RemoveJob(jobId); // Clean up after successful download
        return File(status.FileBytes, "application/octet-stream", $"Documento_{jobId}.docx");
    }
}


