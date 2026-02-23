using IrmaDulce.Domain.Enums;

namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Pagamento recebido presencialmente na escola.
/// </summary>
public class PagamentoEscola
{
    public int Id { get; set; }

    public int MensalidadeId { get; set; }
    public Mensalidade Mensalidade { get; set; } = null!;

    public decimal ValorPago { get; set; }
    public MetodoPagamento MetodoPagamento { get; set; }
    public DateTime DataPagamento { get; set; }

    /// <summary>
    /// Usuário que registrou o pagamento.
    /// </summary>
    public int OperadorId { get; set; }
    public Usuario Operador { get; set; } = null!;

    public string? Observacao { get; set; }
}
