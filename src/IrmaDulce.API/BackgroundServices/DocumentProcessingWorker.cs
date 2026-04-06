using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using IrmaDulce.Application.Interfaces;

namespace IrmaDulce.API.BackgroundServices;

public class DocumentProcessingWorker : BackgroundService
{
    private readonly DocumentProcessingChannel _channel;
    private readonly DocumentJobCache _jobCache;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DocumentProcessingWorker> _logger;

    public DocumentProcessingWorker(
        DocumentProcessingChannel channel,
        DocumentJobCache jobCache,
        IServiceProvider serviceProvider,
        ILogger<DocumentProcessingWorker> logger)
    {
        _channel = channel;
        _jobCache = jobCache;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DocumentProcessingWorker is starting.");

        await foreach (var job in _channel.ReadAllAsync(stoppingToken))
        {
            try
            {
                _jobCache.UpdateStatus(job.JobId, "Processando");
                _logger.LogInformation("Iniciando processamento do Job {JobId}", job.JobId);

                using var scope = _serviceProvider.CreateScope();
                var documentoService = scope.ServiceProvider.GetRequiredService<IDocumentoService>();

                // Processa o doc pesado
                var fileBytes = await documentoService.EmitirDocumentoAsync(job.Request, job.OperadorId);

                // Salva o resultado no cache com o Base64 ou num temp dir
                _jobCache.SetCompleted(job.JobId, fileBytes);
                
                _logger.LogInformation("Job {JobId} concluído com sucesso.", job.JobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar Job {JobId}.", job.JobId);
                _jobCache.SetError(job.JobId, ex.Message);
            }
        }
    }
}
