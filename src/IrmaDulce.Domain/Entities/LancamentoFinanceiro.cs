using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Lançamento financeiro avulso (entradas e saídas não vinculadas a mensalidades).
/// </summary>
public class LancamentoFinanceiro
{
    public int Id { get; set; }
    public DateTime Data { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoLancamento Tipo { get; set; }

    public int? CategoriaId { get; set; }
    public CategoriaFinanceira? Categoria { get; set; }

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
