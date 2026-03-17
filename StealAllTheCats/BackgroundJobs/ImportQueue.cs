using System.Threading.Channels;

namespace StealAllTheCats.BackgroundJobs;

public class ImportQueue
{
    private readonly Channel<int> _channel = Channel.CreateUnbounded<int>();

    public async ValueTask EnqueueAsync(int jobId)
    {
        await _channel.Writer.WriteAsync(jobId);
    }

    public async ValueTask<int> DequeueAsync(CancellationToken ct)
    {
        return await _channel.Reader.ReadAsync(ct);
    }
}
