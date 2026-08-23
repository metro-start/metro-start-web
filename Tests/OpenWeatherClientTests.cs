using MetroStart.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace MetroStart.Tests;

public class OpenWeatherClientTests
{
    [Fact]
    public async Task EscapesLocationAndDeserializesSnakeCaseProperties()
    {
        var handler = new StubHandler("""
            {
              "name": "Vancouver",
              "main": { "temp": 10, "temp_min": 8.5, "temp_max": 12.5 },
              "sys": { "country": "CA" },
              "weather": []
            }
            """);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new("https://api.openweathermap.org/data/2.5/"),
        };
        var client = new OpenWeatherClient(
            httpClient,
            new AppConfiguration("unused", "secret"),
            NullLogger<OpenWeatherClient>.Instance);

        var weather = await client.GetCurrentAsync(
            "Vancouver & Coast",
            "metric",
            CancellationToken.None);

        Assert.Equal("Vancouver", weather.Name);
        Assert.Equal(8.5, weather.Main.TempMin);
        Assert.Contains("q=Vancouver%20%26%20Coast", handler.RequestUri?.Query);
    }

    private sealed class StubHandler(string response) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response),
            });
        }
    }
}
