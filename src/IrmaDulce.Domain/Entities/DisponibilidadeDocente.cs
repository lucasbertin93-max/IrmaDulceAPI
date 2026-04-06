namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Indica os dias e horários em que o Professor tem disponibilidade para lecionar na escola.
/// </summary>
public class DisponibilidadeDocente
{
    public int Id { get; set; }

    public int DocenteId { get; set; }
    public Pessoa Docente { get; set; } = null!;

    public DayOfWeek DiaSemana { get; set; }
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFim { get; set; }
}
