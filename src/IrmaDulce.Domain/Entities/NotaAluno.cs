namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Nota de um aluno em uma avaliação específica.
/// </summary>
public class NotaAluno
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Pessoa Aluno { get; set; } = null!;

    public int AvaliacaoId { get; set; }
    public Avaliacao Avaliacao { get; set; } = null!;

    public decimal Nota { get; set; }
    public string? Observacao { get; set; }
    public DateTime DataLancamento { get; set; } = DateTime.UtcNow;
}
