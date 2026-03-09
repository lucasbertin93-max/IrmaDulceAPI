namespace IrmaDulce.Domain.Entities;

/// <summary>
/// Configurações globais do sistema (singleton/chave-valor).
/// </summary>
public class ConfiguracaoEscolar
{
    public int Id { get; set; }

    /// <summary>
    /// Média mínima de aprovação (padrão: 7.0).
    /// </summary>
    public decimal MediaMinimaAprovacao { get; set; } = 7.0m;

    /// <summary>
    /// Frequência mínima em % (padrão: 75%).
    /// </summary>
    public decimal FrequenciaMinimaPercent { get; set; } = 75.0m;

    /// <summary>
    /// Horas-aula padrão por dia letivo (padrão: 4).
    /// </summary>
    public int HorasAulaPadraoPorDia { get; set; } = 4;

    /// <summary>
    /// Limite máximo de parcelas permitidas na escola (padrão: 26).
    /// </summary>
    public int PrazoMaximoParcelamento { get; set; } = 26;

    /// <summary>
    /// Multa de atraso em % (padrão: 2.0%).
    /// </summary>
    public decimal MultaAtrasoPercent { get; set; } = 2.0m;

    /// <summary>
    /// Juros mensais em % (padrão: 1.0%).
    /// </summary>
    public decimal JurosMensalPercent { get; set; } = 1.0m;

    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
}
