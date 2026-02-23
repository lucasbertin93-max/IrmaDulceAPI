namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Avaliação cadastrada para uma disciplina dentro de uma turma.
/// </summary>
public class Avaliacao
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty; // Ex: "Prova 1", "Trabalho Final"
    public string? Descricao { get; set; }
    public DateTime? DataAplicacao { get; set; }

    /// <summary>
    /// Peso da avaliação para cálculo de média ponderada.
    /// Padrão: 1 (média aritmética simples se todos forem 1).
    /// </summary>
    public decimal Peso { get; set; } = 1;

    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;

    public int DisciplinaId { get; set; }
    public Disciplina Disciplina { get; set; } = null!;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Navegação
    public ICollection<NotaAluno> Notas { get; set; } = new List<NotaAluno>();
}
