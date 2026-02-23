using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Matrícula: relação N:N entre Aluno (Pessoa) e Turma.
/// </summary>
public class Matricula
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Pessoa Aluno { get; set; } = null!;

    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;

    public StatusMatricula Status { get; set; } = StatusMatricula.Ativo;
    public DateTime DataMatricula { get; set; } = DateTime.UtcNow;
    public DateTime? DataSaida { get; set; }
}
