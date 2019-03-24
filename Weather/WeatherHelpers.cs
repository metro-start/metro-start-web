using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using MetroStart.Entities;
using MetroStart.Weather.Respnoses;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace MetroStart.Weather
{
    public static class WeatherHelpers
    {
        public static int ForecastDays = 3;
        public static HttpClient Client { get; } = new HttpClient();

        public static async Task<WeatherEntity> GetCachedWeather(string location, string units, ILogger log)
        {
            var table = await WeatherEntity.GetCloudTable(log);
            TableResult retrievedResult = await table.ExecuteAsync(TableOperation.Retrieve<WeatherEntity>(units, location));
            return retrievedResult?.Result as WeatherEntity;
        }

        // Save the weather entity.
        public static async Task CacheWeather(WeatherEntity weather, ILogger log)
        {
            var table = await WeatherEntity.GetCloudTable(log);
            TableOperation insertOperation = TableOperation.InsertOrReplace(weather);

            var result = await table.ExecuteAsync(insertOperation);
            log.LogInformation($"Caching current weather with location: {weather.Location}, units: {weather.Units} and mod dates:({weather.WeatherForecastModified.ToLongTimeString()}, {weather.CurrentWeatherModified.ToLongTimeString()}) with result: {result.HttpStatusCode}");
        }

        // Tries to update the current weather.
        // returns: True if the current weather was updated, false if it does not require updating.
        public static async Task<bool> UpdateCurrentWeather(WeatherEntity weather, ILogger log)
        {
            if (weather.CurrentWeather == null || weather.CurrentWeatherAge > TimeSpan.FromHours(3))
            {
                if (await GetCurrentWeather(weather.Location, weather.Units, log) is CurrentWeatherResponse currentWeather)
                {
                    weather.CurrentWeather = await GetCurrentWeather(weather.Location, weather.Units, log);
                    return true;
                }
                else
                {
                    log.LogInformation("Could not get the current weather information.");
                }
            }

            log.LogInformation($"Current weather does not reqiure updating. Age: {weather.CurrentWeatherAge.ToHumanReadableString()}");
            return false;
        }

        // Tries to update the weather forecast.
        // returns: True if the weather forecast was updated, false if it does not require updating.
        public static async Task<bool> UpdateWeatherForecast(WeatherEntity weather, ILogger log)
        {
            if (weather.WeatherForecast == null || weather.WeatherForecastAge > TimeSpan.FromHours(9))
            {
                weather.WeatherForecast = await GetWeatherForecast(weather.Location, weather.Units, log);
                return true;
            }

            log.LogInformation($"Weather forecast does not reqiure updating. Age: {weather.WeatherForecastAge.ToHumanReadableString()}");
            return false;
        }

        // Gets the current weather from the remote service.
        // returns: The current weather response.
        public static async Task<CurrentWeatherResponse> GetCurrentWeather(string location, string units, ILogger log)
        {
            var responseText = await MakeOpenWeatherResponse($"weather?q={location}&units={units}", log);
            return CurrentWeatherResponse.FromJson(responseText);
        }

        // Gets the weather forecast from the remote service.
        // returns: The weather forecast response.
        public static async Task<WeatherForecastResponse> GetWeatherForecast(string location, string units, ILogger log)
        {
            var responseText = await MakeOpenWeatherResponse($"forecast?q={location}&units={units}", log);
            return WeatherForecastResponse.FromJson(responseText);
        }

        // Make a request for weather data from OpenWeatherMap.
        public static async Task<string> MakeOpenWeatherResponse(string urlAction, ILogger log)
        {
            var weatherKey = System.Environment.GetEnvironmentVariable("WEATHER_API_KEY", EnvironmentVariableTarget.Process).Nullable()
            ?? throw new ArgumentNullException("WEATHER_API_KEY");

            var weatherUrl = $"https://api.openweathermap.org/data/2.5/{urlAction}";
            log.LogInformation($"Requesting weatherUrl: {weatherUrl}");

            var response = await Client.GetAsync($"{weatherUrl}&APPID={weatherKey}");
            response.EnsureSuccessStatusCode();
            return await response?.Content?.ReadAsStringAsync() ?? throw new InvalidDataException("Response was null");
        }
    }
}
