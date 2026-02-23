using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IrmaDulce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("recuperar-senha")]
    public async Task<IActionResult> RecuperarSenha([FromBody] string login)
    {
        var result = await _authService.RecuperarSenhaAsync(login);
        // Sempre retorna sucesso para não expor se o login existe
        return Ok(new { message = "Se o login existir, as instruções serão enviadas." });
    }
}
