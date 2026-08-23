using MetroStart.Weather.Responses;

namespace MetroStart.Tests;

public class WeatherSerializationTests
{
    [Fact]
    public void ForecastDeserializesWindAndFractionalPrecipitation()
    {
        const string json = """
            {
              "cod": "200",
              "message": 0,
              "cnt": 1,
              "list": [{
                "dt": 1,
                "main": { "temp": 10, "temp_min": 8, "temp_max": 12 },
                "weather": [],
                "wind": { "speed": 3.5, "deg": 180 },
                "rain": { "3h": 0.25 }
              }]
            }
            """;

        var forecast = WeatherForecastResponse.FromJson(json);

        Assert.NotNull(forecast);
        Assert.Equal(3.5, forecast.WeatherList[0].Wind?.Speed);
        Assert.Equal(0.25, forecast.WeatherList[0].Rain?.ThreeHours);
    }
}
