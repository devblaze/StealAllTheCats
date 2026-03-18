using StealAllTheCats.BackgroundJobs;
using Xunit;

namespace StealAllTheCats.Tests.BackgroundJobs;

public class ImportQueueTests
{
    [Fact]
    public async Task EnqueueAndDequeue_ReturnsSameJobId()
    {
        var queue = new ImportQueue();

        await queue.EnqueueAsync(42);
        var result = await queue.DequeueAsync(CancellationToken.None);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Dequeue_PreservesOrder()
    {
        var queue = new ImportQueue();

        await queue.EnqueueAsync(1);
        await queue.EnqueueAsync(2);
        await queue.EnqueueAsync(3);

        Assert.Equal(1, await queue.DequeueAsync(CancellationToken.None));
        Assert.Equal(2, await queue.DequeueAsync(CancellationToken.None));
        Assert.Equal(3, await queue.DequeueAsync(CancellationToken.None));
    }

    [Fact]
    public async Task Dequeue_BlocksUntilItemAvailable()
    {
        var queue = new ImportQueue();
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => queue.DequeueAsync(cts.Token).AsTask());
    }

    [Fact]
    public async Task Dequeue_IsCancelledByToken()
    {
        var queue = new ImportQueue();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => queue.DequeueAsync(cts.Token).AsTask());
    }
}
