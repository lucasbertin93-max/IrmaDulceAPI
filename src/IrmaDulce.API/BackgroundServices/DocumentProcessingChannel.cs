using System.Threading.Channels;
using IrmaDulce.Application.DTOs;

namespace IrmaDulce.API.BackgroundServices;

public class DocumentJob
{
    public Guid JobId { get; set; }
    public EmitirDocumentoRequest Request { get; set; } = null!;
    public int OperadorId { get; set; }
}

public class DocumentProcessingChannel
{
    private readonly Channel<DocumentJob> _channel;

    public DocumentProcessingChannel()
    {
        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<DocumentJob>(options);
    }

    public async Task AddJobAsync(DocumentJob job, CancellationToken ct = default)
    {
        await _channel.Writer.WriteAsync(job, ct);
    }

    public IAsyncEnumerable<DocumentJob> ReadAllAsync(CancellationToken ct = default)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}
