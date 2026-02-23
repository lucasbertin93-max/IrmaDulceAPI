namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Alocação de professor em turma/sala em determinado dia e horário.
/// </summary>
public class CronogramaAula
{
    public int Id { get; set; }

    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;

    public int DisciplinaId { get; set; }
    public Disciplina Disciplina { get; set; } = null!;

    public int DocenteId { get; set; }
    public Pessoa Docente { get; set; } = null!;

    public DateTime Data { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }
    public string? Sala { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
