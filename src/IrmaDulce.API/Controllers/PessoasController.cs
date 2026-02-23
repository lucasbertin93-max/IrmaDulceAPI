using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class PessoasController : ControllerBase
{
    private readonly IPessoaService _pessoaService;

    public PessoasController(IPessoaService pessoaService)
    {
        _pessoaService = pessoaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PessoaResponse>>> GetAll([FromQuery] PerfilUsuario? perfil)
    {
        var pessoas = await _pessoaService.GetAllAsync(perfil);
        return Ok(pessoas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PessoaResponse>> GetById(int id)
    {
        var pessoa = await _pessoaService.GetByIdAsync(id);
        return pessoa == null ? NotFound() : Ok(pessoa);
    }

    [HttpGet("funcional/{idFuncional}")]
    public async Task<ActionResult<PessoaResponse>> GetByIdFuncional(string idFuncional)
    {
        var pessoa = await _pessoaService.GetByIdFuncionalAsync(idFuncional);
        return pessoa == null ? NotFound() : Ok(pessoa);
    }

    [HttpPost]
    public async Task<ActionResult<PessoaResponse>> Criar([FromBody] PessoaCreateRequest request)
    {
        try
        {
            var pessoa = await _pessoaService.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = pessoa.Id }, pessoa);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PessoaResponse>> Atualizar(int id, [FromBody] PessoaCreateRequest request)
    {
        try
        {
            var pessoa = await _pessoaService.AtualizarAsync(id, request);
            return Ok(pessoa);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Desativar(int id)
    {
        try
        {
            await _pessoaService.DesativarAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
