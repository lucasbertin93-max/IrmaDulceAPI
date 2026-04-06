using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Master,Administrativo")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    private int GetPessoaIdLogada()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "pessoaId");
        return claim != null ? int.Parse(claim.Value) : 0;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioResponse>>> GetAll()
    {
        var usuarios = await _usuarioService.GetAllAsync();
        return Ok(usuarios);
    }

    [HttpPut("{id}/perfil")]
    public async Task<IActionResult> AlterarPerfil(int id, [FromBody] UsuarioUpdateRoleRequest request)
    {
        try
        {
            await _usuarioService.AlterarPerfilAsync(id, GetPessoaIdLogada(), request.Perfil);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> AlternarStatus(int id)
    {
        try
        {
            await _usuarioService.AlternarStatusAcessoAsync(id, GetPessoaIdLogada());
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/reset-senha")]
    public async Task<IActionResult> ResetarSenha(int id, [FromBody] UsuarioResetPasswordRequest request)
    {
        try
        {
            await _usuarioService.ResetarSenhaAsync(id, GetPessoaIdLogada(), request.NovaSenha);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
