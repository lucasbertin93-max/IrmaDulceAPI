using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Mensalidade de um aluno.
/// </summary>
public class Mensalidade
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Pessoa Aluno { get; set; } = null!;

    public int MesReferencia { get; set; } // 1-12
    public int AnoReferencia { get; set; }
    public decimal Valor { get; set; }
    public DateTime DataVencimento { get; set; }
    public DateTime? DataPagamento { get; set; }
    public StatusMensalidade Status { get; set; } = StatusMensalidade.EmAberto;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    // Navegação
    public PagamentoEscola? PagamentoEscola { get; set; }
}
