using Azure;
using Azure.Data.Tables;
using MetroStart.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace MetroStart.Services;

public sealed class WeatherService(
    TableServiceClient tableService,
    OpenWeatherClient weatherClient,
    TimeProvider timeProvider,
    IMemoryCache memoryCache,
    ILogger<WeatherService> logger)
{
    private readonly TableClient table = tableService.GetTableClient("weather");

    public async Task<WeatherEntity?> GetAsync(
        string location,
        string units,
        CancellationToken cancellationToken)
    {
        var key = $"{units}:{location.ToUpperInvariant()}";
        var refreshLock = memoryCache.GetOrCreate(key, entry =>
        {
            entry.SetSize(1);
            entry.SetSlidingExpiration(TimeSpan.FromHours(1));
            return new SemaphoreSlim(1, 1);
        })!;

        await refreshLock.WaitAsync(cancellationToken);
        try
        {
            await table.CreateIfNotExistsAsync(cancellationToken);
            var cached = await table.GetEntityIfExistsAsync<WeatherEntity>(
                units,
                location,
                cancellationToken: cancellationToken);
            var weather = cached.HasValue
                ? cached.Value!
                : new WeatherEntity(location, units, null, null);

            var current = weather.CurrentWeather;
            var forecast = weather.WeatherForecast;
            var currentTask = current is null ||
                GetAge(weather.CurrentWeatherModified) > TimeSpan.FromHours(3)
                ? weatherClient.GetCurrentAsync(location, units, cancellationToken)
                : Task.FromResult(current);
            var forecastTask = forecast is null ||
                GetAge(weather.WeatherForecastModified) > TimeSpan.FromHours(9)
                ? weatherClient.GetForecastAsync(location, units, cancellationToken)
                : Task.FromResult(forecast);

            try
            {
                await Task.WhenAll(currentTask, forecastTask);
                weather.CurrentWeather = await currentTask;
                weather.WeatherForecast = await forecastTask;
                await SaveAsync(weather, cached.HasValue, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(
                    exception,
                    "Could not refresh weather for {Location}, {Units}",
                    location,
                    units);
            }

            return weather.CurrentWeather is null ||
                weather.WeatherForecast?.WeatherList.FirstOrDefault() is null
                ? null
                : weather;
        }
        finally
        {
            refreshLock.Release();
        }
    }

    private TimeSpan GetAge(DateTime modified) =>
        timeProvider.GetUtcNow().UtcDateTime - modified.ToUniversalTime();

    private async Task SaveAsync(
        WeatherEntity weather,
        bool exists,
        CancellationToken cancellationToken)
    {
        try
        {
            if (exists)
            {
                await table.UpdateEntityAsync(
                    weather,
                    weather.ETag,
                    TableUpdateMode.Replace,
                    cancellationToken);
            }
            else
            {
                await table.AddEntityAsync(weather, cancellationToken);
            }
        }
        catch (RequestFailedException exception) when (exception.Status is 409 or 412)
        {
            logger.LogInformation(
                "A newer cache entry already exists for {Location}, {Units}",
                weather.Location,
                weather.Units);
        }
    }
}
