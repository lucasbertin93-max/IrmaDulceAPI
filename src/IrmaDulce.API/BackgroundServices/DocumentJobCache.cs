using System.Collections.Concurrent;

namespace IrmaDulce.API.BackgroundServices;

public class JobStatusInfo
{
    public Guid JobId { get; set; }
    public string Status { get; set; } = "Pendente";
    public string? ErrorMessage { get; set; }
    public byte[]? FileBytes { get; set; }
}

public class DocumentJobCache
{
    private readonly ConcurrentDictionary<Guid, JobStatusInfo> _jobs = new();

    public void AddJob(Guid jobId)
    {
        _jobs.TryAdd(jobId, new JobStatusInfo { JobId = jobId });
    }

    public void UpdateStatus(Guid jobId, string status)
    {
        if (_jobs.TryGetValue(jobId, out var info))
        {
            info.Status = status;
        }
    }

    public void SetCompleted(Guid jobId, byte[] fileBytes)
    {
        if (_jobs.TryGetValue(jobId, out var info))
        {
            info.Status = "Concluido";
            info.FileBytes = fileBytes;
        }
    }

    public void SetError(Guid jobId, string message)
    {
        if (_jobs.TryGetValue(jobId, out var info))
        {
            info.Status = "Erro";
            info.ErrorMessage = message;
        }
    }

    public JobStatusInfo? GetStatus(Guid jobId)
    {
        _jobs.TryGetValue(jobId, out var info);
        return info;
    }

    public void RemoveJob(Guid jobId)
    {
        _jobs.TryRemove(jobId, out _);
    }
}
