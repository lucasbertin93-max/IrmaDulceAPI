namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Define os dias letivos semanais em que a turma funciona.
/// </summary>
public class TurmaDiaLetivo
{
    public int Id { get; set; }
    
    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;

    public DayOfWeek DiaSemana { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }
}
