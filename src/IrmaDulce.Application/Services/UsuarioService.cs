using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Enums;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPessoaRepository _pessoaRepo;

    public UsuarioService(IUsuarioRepository usuarioRepo, IPessoaRepository pessoaRepo)
    {
        _usuarioRepo = usuarioRepo;
        _pessoaRepo = pessoaRepo;
    }

    public async Task<IEnumerable<UsuarioResponse>> GetAllAsync()
    {
        var usuarios = await _usuarioRepo.GetAllWithPessoaAsync();
        return usuarios.Select(u => new UsuarioResponse(
            Id: u.Id,
            Login: u.Login,
            PessoaNome: u.Pessoa.NomeCompleto,
            IdFuncional: u.Pessoa.IdFuncional,
            Perfil: u.Perfil,
            Ativo: u.Ativo,
            UltimoAcesso: u.UltimoAcesso
        ));
    }

    public async Task AlterarPerfilAsync(int usuarioId, int solicitantePessoaId, PerfilUsuario novoPerfil)
    {
        var alvo = await _usuarioRepo.GetByIdAsync(usuarioId) ?? throw new KeyNotFoundException("Usuário não encontrado.");
        var solicitante = await _usuarioRepo.GetByPessoaIdAsync(solicitantePessoaId) ?? throw new UnauthorizedAccessException("Usuário solicitante inválido.");

        // Regra de Negócio: Administrativos não podem alterar ou promover Masters, nem a si mesmos na tela de listagem de forma a rebaixar.
        if (solicitante.Perfil != PerfilUsuario.Master)
        {
            if (alvo.Perfil == PerfilUsuario.Master || alvo.Perfil == PerfilUsuario.Administrativo)
            {
                throw new InvalidOperationException("Você não tem permissão para alterar o perfil de um Administrador ou Master.");
            }
            if (novoPerfil == PerfilUsuario.Master || novoPerfil == PerfilUsuario.Administrativo)
            {
                throw new InvalidOperationException("Você não tem permissão para promover um usuário para Administrador ou Master.");
            }
        }

        alvo.Perfil = novoPerfil;
        await _usuarioRepo.UpdateAsync(alvo);

        // Opcional: Sincronizar com Pessoa (pois a ficha reflete a ocupação oficial)
        var pessoa = await _pessoaRepo.GetByIdAsync(alvo.PessoaId);
        if (pessoa != null)
        {
            pessoa.Perfil = novoPerfil;
            await _pessoaRepo.UpdateAsync(pessoa);
        }
    }

    public async Task AlternarStatusAcessoAsync(int usuarioId, int solicitantePessoaId)
    {
        var alvo = await _usuarioRepo.GetByIdAsync(usuarioId) ?? throw new KeyNotFoundException("Usuário não encontrado.");
        var solicitante = await _usuarioRepo.GetByPessoaIdAsync(solicitantePessoaId) ?? throw new UnauthorizedAccessException("Usuário solicitante inválido.");

        if (solicitante.Perfil != PerfilUsuario.Master && alvo.Perfil == PerfilUsuario.Master)
            throw new InvalidOperationException("Você não tem permissão para suspender um Master.");

        if (alvo.Id == solicitante.Id)
            throw new InvalidOperationException("Você não pode suspender a si próprio.");

        alvo.Ativo = !alvo.Ativo;
        await _usuarioRepo.UpdateAsync(alvo);
    }

    public async Task ResetarSenhaAsync(int usuarioId, int solicitantePessoaId, string novaSenha)
    {
        if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6)
            throw new ArgumentException("A nova senha deve ter pelo menos 6 caracteres.");

        var alvo = await _usuarioRepo.GetByIdAsync(usuarioId) ?? throw new KeyNotFoundException("Usuário não encontrado.");
        var solicitante = await _usuarioRepo.GetByPessoaIdAsync(solicitantePessoaId) ?? throw new UnauthorizedAccessException("Usuário solicitante inválido.");

        if (solicitante.Perfil != PerfilUsuario.Master && (alvo.Perfil == PerfilUsuario.Master || alvo.Perfil == PerfilUsuario.Administrativo))
            throw new InvalidOperationException("Apenas o Master pode alterar senhas de Administradores ou Masters.");

        alvo.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);
        await _usuarioRepo.UpdateAsync(alvo);
    }
}
