namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Disciplinas associadas a uma turma (importadas do curso ao criar a turma).
/// </summary>
public class TurmaDisciplina
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;
    public int DisciplinaId { get; set; }
    public Disciplina Disciplina { get; set; } = null!;
}
