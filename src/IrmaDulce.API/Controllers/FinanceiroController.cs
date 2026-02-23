using System.Security.Claims;
using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class FinanceiroController : ControllerBase
{
    private readonly IFinanceiroService _financeiroService;

    public FinanceiroController(IFinanceiroService financeiroService)
    {
        _financeiroService = financeiroService;
    }

    // Dashboard
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardFinanceiroResponse>> GetDashboard(
        [FromQuery] DateTime inicio, [FromQuery] DateTime fim)
    {
        var dashboard = await _financeiroService.GetDashboardAsync(inicio, fim);
        return Ok(dashboard);
    }

    // Mensalidades
    [HttpGet("mensalidades")]
    public async Task<ActionResult<IEnumerable<MensalidadeResponse>>> GetMensalidades(
        [FromQuery] int? alunoId, [FromQuery] StatusMensalidade? status,
        [FromQuery] int? mes, [FromQuery] int? ano)
    {
        var mensalidades = await _financeiroService.GetMensalidadesAsync(alunoId, status, mes, ano);
        return Ok(mensalidades);
    }

    [HttpGet("mensalidades/aluno/{alunoId}")]
    [Authorize] // Aluno pode ver seus boletos
    public async Task<ActionResult<IEnumerable<MensalidadeResponse>>> GetMensalidadesAluno(int alunoId)
    {
        var mensalidades = await _financeiroService.GetMensalidadesAsync(alunoId, null, null, null);
        return Ok(mensalidades);
    }

    [HttpPost("mensalidades/gerar")]
    public async Task<IActionResult> GerarMensalidades([FromBody] GerarMensalidadesRequest request)
    {
        await _financeiroService.GerarMensalidadesAsync(request);
        return Ok(new { message = "Mensalidades geradas com sucesso." });
    }

    [HttpPost("mensalidades/pagamento")]
    public async Task<IActionResult> RegistrarPagamento([FromBody] RegistrarPagamentoRequest request)
    {
        try
        {
            var operadorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            await _financeiroService.RegistrarPagamentoAsync(request, operadorId);
            return Ok(new { message = "Pagamento registrado com sucesso." });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPut("mensalidades/{id}")]
    public async Task<ActionResult<MensalidadeResponse>> AtualizarMensalidade(
        int id, [FromBody] AtualizarMensalidadeRequest request)
    {
        try
        {
            var mensalidade = await _financeiroService.AtualizarMensalidadeAsync(id, request.Valor, request.DataVencimento);
            return Ok(mensalidade);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("mensalidades/{id}")]
    public async Task<IActionResult> DeletarMensalidade(int id)
    {
        try
        {
            await _financeiroService.DeletarMensalidadeAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    // Lançamentos (Entradas/Saídas)
    [HttpGet("lancamentos")]
    public async Task<ActionResult<IEnumerable<LancamentoResponse>>> GetLancamentos(
        [FromQuery] DateTime inicio, [FromQuery] DateTime fim, [FromQuery] TipoLancamento? tipo)
    {
        var lancamentos = await _financeiroService.GetLancamentosAsync(inicio, fim, tipo);
        return Ok(lancamentos);
    }

    [HttpPost("lancamentos")]
    public async Task<ActionResult<LancamentoResponse>> AdicionarLancamento([FromBody] LancamentoRequest request)
    {
        var lancamento = await _financeiroService.AdicionarLancamentoAsync(request);
        return Created("", lancamento);
    }
}

/// <summary>
/// DTO auxiliar para atualização de mensalidade (valor + vencimento).
/// </summary>
public record AtualizarMensalidadeRequest(decimal Valor, DateTime DataVencimento);
