using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Domain.Entities;

public class Pessoa
{
    public int Id { get; set; }
    public string IdFuncional { get; set; } = string.Empty; // A000001, D0001, AD0001
    public PerfilUsuario Perfil { get; set; }

    // Dados Pessoais
    public string NomeCompleto { get; set; } = string.Empty;
    public string RG { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public EstadoCivil EstadoCivil { get; set; }
    public DateTime DataNascimento { get; set; }
    public Sexo Sexo { get; set; } = Sexo.NaoInformado;
    public string Naturalidade { get; set; } = string.Empty;
    public string Nacionalidade { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Endereço
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string CEP { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string? PontoReferencia { get; set; }

    // Filiação
    public string NomePai { get; set; } = string.Empty;
    public string NomeMae { get; set; } = string.Empty;

    // Responsável Financeiro
    public int? ResponsavelFinanceiroId { get; set; }
    public Pessoa? ResponsavelFinanceiro { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Navegação
    public Usuario? Usuario { get; set; }
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<Mensalidade> Mensalidades { get; set; } = new List<Mensalidade>();
    public ICollection<PresencaAluno> Presencas { get; set; } = new List<PresencaAluno>();
    public ICollection<NotaAluno> Notas { get; set; } = new List<NotaAluno>();
}
