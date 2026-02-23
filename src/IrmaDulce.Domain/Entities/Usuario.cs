using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public int PessoaId { get; set; }
    public Pessoa Pessoa { get; set; } = null!;

    public string Login { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoAcesso { get; set; }
}
