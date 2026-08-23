using MetroStart.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using MetroStart.SystemEndpoints;

namespace MetroStart.Weather;

public sealed class GetWeather(
    WeatherService weatherService,
    RequestRateLimiter rateLimiter,
    TimeProvider timeProvider)
{
    [Function("weather")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!rateLimiter.TryAcquire(
            "weather",
            request.GetClientId(),
            60,
            TimeSpan.FromMinutes(1)))
        {
            return new StatusCodeResult(StatusCodes.Status429TooManyRequests);
        }

        var location = request.Query["location"].ToString().Trim();
        var units = request.Query["units"].ToString().ToLowerInvariant();
        if (!ApiValidation.IsValidWeatherRequest(location, units))
        {
            return new BadRequestObjectResult(
                "location is required and units must be metric or imperial.");
        }

        var weather = await weatherService.GetAsync(
            location,
            units,
            cancellationToken);
        var forecast = weather?.WeatherForecast?.WeatherList.FirstOrDefault();
        if (weather?.CurrentWeather is null || forecast is null)
        {
            return new ObjectResult("Weather data is temporarily unavailable.")
            {
                StatusCode = StatusCodes.Status502BadGateway,
            };
        }

        request.HttpContext.Response.Headers.CacheControl = "public, max-age=900";
        request.HttpContext.Response.Headers["X-API-Version"] = ApiContract.Version;
        return new OkObjectResult(new WeatherResponse(
            weather.CurrentWeather.Name,
            weather.CurrentWeather.Sys.Country,
            weather.CurrentWeather.Main.Temp,
            forecast.Main.TempMax,
            forecast.Main.TempMin,
            weather.Units,
            weather.CurrentWeather.Weather.FirstOrDefault()?.Description,
            forecast.Wind?.Speed ?? 0,
            forecast.Wind?.Deg ?? 0,
            forecast.Rain?.ThreeHours ?? 0,
            forecast.Snow?.ThreeHours ?? 0,
            weather.Timestamp is null
                ? null
                : (timeProvider.GetUtcNow() - weather.Timestamp.Value)
                    .ToHumanReadableString(),
            weather.CurrentWeatherAge.ToHumanReadableString(),
            weather.WeatherForecastAge.ToHumanReadableString()));
    }
}

public sealed record WeatherResponse(
    string Name,
    string Country,
    double Temp,
    double TempMax,
    double TempMin,
    string Units,
    string? Description,
    double WindSpeed,
    long WindDeg,
    double Rain3h,
    double Snow3h,
    string? EntityAge,
    string CurrentWeatherAge,
    string WeatherForecastAge);
