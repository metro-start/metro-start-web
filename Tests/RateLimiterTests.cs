using MetroStart.Services;

namespace MetroStart.Tests;

public class RateLimiterTests
{
    [Fact]
    public void LimitsEachClientAndResetsAfterTheWindow()
    {
        var time = new TestTimeProvider();
        var limiter = new RequestRateLimiter(time);

        Assert.True(limiter.TryAcquire("weather", "client-a", 2, TimeSpan.FromMinutes(1)));
        Assert.True(limiter.TryAcquire("weather", "client-a", 2, TimeSpan.FromMinutes(1)));
        Assert.False(limiter.TryAcquire("weather", "client-a", 2, TimeSpan.FromMinutes(1)));
        Assert.True(limiter.TryAcquire("weather", "client-b", 2, TimeSpan.FromMinutes(1)));

        time.Advance(TimeSpan.FromMinutes(1));
        Assert.True(limiter.TryAcquire("weather", "client-a", 2, TimeSpan.FromMinutes(1)));
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private DateTimeOffset now = DateTimeOffset.UtcNow;

        public override DateTimeOffset GetUtcNow() => now;

        public void Advance(TimeSpan duration) => now += duration;
    }
}
