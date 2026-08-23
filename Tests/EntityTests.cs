using MetroStart.Entities;
using MetroStart.Weather.Responses;

namespace MetroStart.Tests;

public class EntityTests
{
    [Fact]
    public void ThemeEntityRoundTripsThemeContent()
    {
        var entity = new ThemeEntity(
            "Metro Start",
            "Clean",
            true,
            new Dictionary<string, string> { ["mainColor"] = "#fff" });

        Assert.Equal("Metro Start", entity.PartitionKey);
        Assert.Equal("Clean", entity.RowKey);
        Assert.Equal("#fff", entity.ThemeContent["mainColor"]);
    }

    [Fact]
    public void WeatherEntityRoundTripsResponsesAndUsesUtcAges()
    {
        var entity = new WeatherEntity(
            "Vancouver, CA",
            "metric",
            new CurrentWeatherResponse { Name = "Vancouver" },
            new WeatherForecastResponse
            {
                WeatherList = [new WeatherList { Main = new Main { Temp = 12 } }],
            });

        Assert.Equal("Vancouver", entity.CurrentWeather?.Name);
        Assert.Equal(12, entity.WeatherForecast?.WeatherList[0].Main.Temp);
        Assert.InRange(entity.CurrentWeatherAge, TimeSpan.Zero, TimeSpan.FromSeconds(1));
    }
}
