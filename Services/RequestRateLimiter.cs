using System.Collections.Concurrent;

namespace MetroStart.Services;

public sealed class RequestRateLimiter(TimeProvider timeProvider)
{
    private readonly ConcurrentDictionary<string, Window> windows = new();
    private int operationCount;

    public bool TryAcquire(string partition, string clientId, int limit, TimeSpan period)
    {
        var key = $"{partition}:{clientId}";
        var now = timeProvider.GetUtcNow();
        if (Interlocked.Increment(ref operationCount) % 256 == 0)
        {
            foreach (var stale in windows.Where(pair =>
                now - pair.Value.Start > TimeSpan.FromHours(1)))
            {
                windows.TryRemove(stale.Key, out _);
            }
        }
        var window = windows.AddOrUpdate(
            key,
            _ => new(now, 1),
            (_, existing) => now - existing.Start >= period
                ? new(now, 1)
                : existing with { Count = existing.Count + 1 });

        return window.Count <= limit;
    }

    private sealed record Window(DateTimeOffset Start, int Count);
}
