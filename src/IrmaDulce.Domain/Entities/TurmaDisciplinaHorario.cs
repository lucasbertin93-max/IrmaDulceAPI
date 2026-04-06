namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Define os horários (dia da semana e turno) em que uma disciplina específica de uma turma será ministrada.
/// </summary>
public class TurmaDisciplinaHorario
{
    public int Id { get; set; }
    
    public int TurmaDisciplinaId { get; set; }
    public TurmaDisciplina TurmaDisciplina { get; set; } = null!;
    
    public DayOfWeek DiaSemana { get; set; }
    
    // 1 = Pré-Intervalo, 2 = Pós-Intervalo
    public int Turno { get; set; }
}
