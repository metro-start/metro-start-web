using MetroStart.Weather.Responses;
using System.Text.Json;

namespace MetroStart.Services;

public sealed class OpenWeatherClient(
    HttpClient client,
    AppConfiguration configuration,
    ILogger<OpenWeatherClient> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public Task<CurrentWeatherResponse> GetCurrentAsync(
        string location,
        string units,
        CancellationToken cancellationToken) =>
        GetAsync<CurrentWeatherResponse>("weather", location, units, cancellationToken);

    public Task<WeatherForecastResponse> GetForecastAsync(
        string location,
        string units,
        CancellationToken cancellationToken) =>
        GetAsync<WeatherForecastResponse>("forecast", location, units, cancellationToken);

    private async Task<T> GetAsync<T>(
        string endpoint,
        string location,
        string units,
        CancellationToken cancellationToken)
    {
        var uri = $"{endpoint}?q={Uri.EscapeDataString(location)}" +
            $"&units={Uri.EscapeDataString(units)}" +
            $"&APPID={Uri.EscapeDataString(configuration.WeatherApiKey)}";
        logger.LogInformation(
            "Requesting OpenWeatherMap {Endpoint} for {Location}",
            endpoint,
            location);
        using var response = await client.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidDataException($"OpenWeatherMap returned no {endpoint} data.");
    }
}
