namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Tabela de associação Many-to-Many entre Curso e Disciplina (grade curricular).
/// </summary>
public class DisciplinaCurso
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;
    public int DisciplinaId { get; set; }
    public Disciplina Disciplina { get; set; } = null!;
    public int? Semestre { get; set; } // Semestre/Período em que é oferecida
}
