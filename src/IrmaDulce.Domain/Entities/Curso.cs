namespace IrmaDulce.Domain.Entities;

public class Curso
{
    public int Id { get; set; }
    public string IdFuncional { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int CargaHoraria { get; set; } // Total em horas
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Navegação
    public ICollection<DisciplinaCurso> DisciplinaCursos { get; set; } = new List<DisciplinaCurso>();
    public ICollection<Turma> Turmas { get; set; } = new List<Turma>();
}
