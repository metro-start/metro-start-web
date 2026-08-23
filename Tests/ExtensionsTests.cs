using MetroStart;

namespace MetroStart.Tests;

public class ExtensionsTests
{
    [Theory]
    [InlineData(30, "30 seconds")]
    [InlineData(120, "2 minutes")]
    [InlineData(7_200, "2 hours")]
    [InlineData(172_800, "2 days")]
    public void FormatsDurationsAtExpectedGranularity(
        int totalSeconds,
        string expected)
    {
        Assert.Equal(
            expected,
            TimeSpan.FromSeconds(totalSeconds).ToHumanReadableString());
    }
}
