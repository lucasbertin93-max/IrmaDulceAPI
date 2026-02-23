using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace IrmaDulce.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IConfiguration _configuration;

    public AuthService(IUsuarioRepository usuarioRepo, IConfiguration configuration)
    {
        _usuarioRepo = usuarioRepo;
        _configuration = configuration;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _usuarioRepo.GetByLoginAsync(request.Login);
        if (usuario == null || !usuario.Ativo)
            throw new UnauthorizedAccessException("Login ou senha inválidos.");

        if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Login ou senha inválidos.");

        // Atualiza último acesso
        usuario.UltimoAcesso = DateTime.UtcNow;
        await _usuarioRepo.UpdateAsync(usuario);

        // Gera token JWT
        var token = GenerateJwtToken(usuario);
        var refreshToken = Guid.NewGuid().ToString("N");

        return new LoginResponse(
            Token: token,
            RefreshToken: refreshToken,
            Nome: usuario.Pessoa.NomeCompleto,
            Perfil: usuario.Perfil.ToString(),
            IdFuncional: usuario.Pessoa.IdFuncional
        );
    }

    public async Task<bool> RecuperarSenhaAsync(string login)
    {
        var usuario = await _usuarioRepo.GetByLoginAsync(login);
        if (usuario == null) return false;

        // TODO: Implementar envio de e-mail com link de recuperação
        // Por enquanto, apenas retorna true se o login existe
        return true;
    }

    private string GenerateJwtToken(Domain.Entities.Usuario usuario)
    {
        var key = _configuration["Jwt:Key"] ?? "IrmaDulce-SecretKey-Dev-2026-SuperSegura-256bits!";
        var issuer = _configuration["Jwt:Issuer"] ?? "IrmaDulce.API";
        var audience = _configuration["Jwt:Audience"] ?? "IrmaDulce.Client";
        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var mins) ? mins : 480;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, usuario.Pessoa.NomeCompleto),
            new Claim(ClaimTypes.Role, usuario.Perfil.ToString()),
            new Claim("pessoaId", usuario.PessoaId.ToString()),
            new Claim("idFuncional", usuario.Pessoa.IdFuncional),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
