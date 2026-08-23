using MetroStart.SystemEndpoints;
using MetroStart.Themes;
using MetroStart.Weather;
using System.Text.Json;

namespace MetroStart.Tests;

public class ApiContractTests
{
    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web);

    [Fact]
    public void WeatherContractMatchesMetroStartClient()
    {
        var response = new WeatherResponse(
            "Vancouver", "CA", 10, 12, 8, "metric", "cloudy",
            2, 180, 0, 0, null, "now", "now");

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(response, WebJson));

        Assert.Equal("Vancouver", json.RootElement.GetProperty("name").GetString());
        Assert.Equal(12, json.RootElement.GetProperty("tempMax").GetDouble());
        Assert.Equal("cloudy", json.RootElement.GetProperty("description").GetString());
    }

    [Fact]
    public void ThemeContractMatchesMetroStartClient()
    {
        var response = new ThemeResponse(
            "Metro:Night", "Metro", "Night", true,
            new() { ["backgroundColor"] = "#000000" });

        using var json = JsonDocument.Parse(JsonSerializer.Serialize(response, WebJson));

        Assert.Equal("Metro:Night", json.RootElement.GetProperty("id").GetString());
        Assert.Equal("Night", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("#000000", json.RootElement
            .GetProperty("themeContent").GetProperty("backgroundColor").GetString());
    }

    [Fact]
    public void ApiVersionIsSemanticAndMatchesAssemblyVersion()
    {
        Assert.True(Version.TryParse(ApiContract.Version, out var version));
        Assert.Equal(1, version.Major);
        Assert.Equal(ApiContract.Version, typeof(ApiContract).Assembly
            .GetName().Version?.ToString(3));
    }
}
