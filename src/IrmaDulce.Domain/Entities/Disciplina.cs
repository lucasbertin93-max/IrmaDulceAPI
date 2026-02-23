namespace IrmaDulce.Domain.Entities;

public class Disciplina
{
    public int Id { get; set; }
    public string IdFuncional { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public int CargaHoraria { get; set; } // Em horas
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    // Navegação
    public ICollection<DisciplinaCurso> DisciplinaCursos { get; set; } = new List<DisciplinaCurso>();
    public ICollection<TurmaDisciplina> TurmaDisciplinas { get; set; } = new List<TurmaDisciplina>();
}
