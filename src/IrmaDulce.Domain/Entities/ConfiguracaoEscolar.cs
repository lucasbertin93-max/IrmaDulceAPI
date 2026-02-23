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

    public DateTime UltimaAtualizacao { get; set; } = DateTime.UtcNow;
}
