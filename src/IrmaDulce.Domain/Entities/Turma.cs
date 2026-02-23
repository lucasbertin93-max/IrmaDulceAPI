namespace IrmaDulce.Domain.Entities;

public class Turma
{
    public int Id { get; set; }
    public string IdFuncional { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Horario { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    public int CursoId { get; set; }
    public Curso Curso { get; set; } = null!;

    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Navegação
    public ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
    public ICollection<TurmaDisciplina> TurmaDisciplinas { get; set; } = new List<TurmaDisciplina>();
    public ICollection<DiarioClasse> DiarioClasses { get; set; } = new List<DiarioClasse>();
    public ICollection<CronogramaAula> Cronogramas { get; set; } = new List<CronogramaAula>();
}
