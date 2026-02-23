namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Registro de presença de um aluno em um dia letivo (DiarioClasse).
/// </summary>
public class PresencaAluno
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Pessoa Aluno { get; set; } = null!;

    public int DiarioClasseId { get; set; }
    public DiarioClasse DiarioClasse { get; set; } = null!;

    public bool Presente { get; set; }
    public bool FaltaJustificada { get; set; }
    public string? Justificativa { get; set; } // Ex: "Atestado médico"
}
