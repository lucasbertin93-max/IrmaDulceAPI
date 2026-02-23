using IrmaDulce.Application.DTOs;
using IrmaDulce.Application.Interfaces;
using IrmaDulce.Domain.Interfaces;

namespace IrmaDulce.Application.Services;

public class ConfiguracaoService : IConfiguracaoService
{
    private readonly IConfiguracaoEscolarRepository _configRepo;

    public ConfiguracaoService(IConfiguracaoEscolarRepository configRepo)
    {
        _configRepo = configRepo;
    }

    public async Task<ConfiguracaoResponse> GetAsync()
    {
        var config = await _configRepo.GetConfigAsync();
        return new ConfiguracaoResponse(
            MediaMinimaAprovacao: config.MediaMinimaAprovacao,
            FrequenciaMinimaPercent: config.FrequenciaMinimaPercent,
            HorasAulaPadraoPorDia: config.HorasAulaPadraoPorDia
        );
    }

    public async Task<ConfiguracaoResponse> AtualizarAsync(ConfiguracaoRequest request)
    {
        var config = await _configRepo.GetConfigAsync();

        config.MediaMinimaAprovacao = request.MediaMinimaAprovacao;
        config.FrequenciaMinimaPercent = request.FrequenciaMinimaPercent;
        config.HorasAulaPadraoPorDia = request.HorasAulaPadraoPorDia;
        config.UltimaAtualizacao = DateTime.UtcNow;

        await _configRepo.UpdateAsync(config);

        return new ConfiguracaoResponse(
            MediaMinimaAprovacao: config.MediaMinimaAprovacao,
            FrequenciaMinimaPercent: config.FrequenciaMinimaPercent,
            HorasAulaPadraoPorDia: config.HorasAulaPadraoPorDia
        );
    }
}
