namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Categoria de lançamento financeiro (ex: Salário, Aluguel, Material).
/// </summary>
public class CategoriaFinanceira
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;

    public ICollection<LancamentoFinanceiro> Lancamentos { get; set; } = new List<LancamentoFinanceiro>();
}
