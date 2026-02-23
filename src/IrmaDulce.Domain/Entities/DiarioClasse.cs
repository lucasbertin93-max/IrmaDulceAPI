namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Registro diário de aula: vincula data + docente + turma + disciplina.
/// </summary>
public class DiarioClasse
{
    public int Id { get; set; }

    public DateTime Data { get; set; }

    public int DocenteId { get; set; }
    public Pessoa Docente { get; set; } = null!;

    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;

    public int DisciplinaId { get; set; }
    public Disciplina Disciplina { get; set; } = null!;

    /// <summary>
    /// Quantidade de horas-aula ministradas neste dia (configurável).
    /// </summary>
    public int QuantidadeHorasAula { get; set; }

    /// <summary>
    /// Conteúdo ministrado nesta data.
    /// </summary>
    public string? ConteudoMinistrado { get; set; }

    /// <summary>
    /// Observações gerais do dia.
    /// </summary>
    public string? Observacoes { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Navegação
    public ICollection<PresencaAluno> Presencas { get; set; } = new List<PresencaAluno>();
}
