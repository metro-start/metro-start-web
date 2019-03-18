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

        public static async Task CacheWeather(WeatherEntity weather, ILogger log)
        {
            var table = await WeatherEntity.GetCloudTable(log);
            TableOperation insertOperation = TableOperation.InsertOrReplace(weather);

            var result = await table.ExecuteAsync(insertOperation);
            log.LogInformation($"Caching current weather with location: {weather.Location}, units: {weather.Units} with result: {result.HttpStatusCode}");
        }

        public static async Task<bool> UpdateCurrentWeather(WeatherEntity weather, string location, string units, ILogger log)
        {
            log.LogInformation($"Updating current weather: {location}, forecast age: {weather.CurrentWeatherAge}");
            if (weather.CurrentWeather == null || weather.CurrentWeatherAge > TimeSpan.FromHours(3))
            {
                weather.CurrentWeather = await GetCurrentWeather(location, units, log);
                weather.CurrentWeatherModified = DateTime.Now;
                return true;
            }

            return false;
        }

        public static async Task<bool> UpdateWeatherForecast(WeatherEntity weather, string location, string units, ILogger log)
        {
            log.LogInformation($"Updating weather forecast: {location}, forecast age: {weather.WeatherForecastAge}, created: {weather.WeatherForecastModified}");
            if (weather.WeatherForecast == null || weather.WeatherForecastAge > TimeSpan.FromHours(9))
            {
                weather.WeatherForecast = await GetWeatherForecast(location, units, log);
                weather.WeatherForecastModified = DateTime.Now;
                return true;
            }

            return false;
        }

        public static async Task<CurrentWeatherResponse> GetCurrentWeather(string location, string units, ILogger log)
        {
            try
            {
                var responseText = await MakeOpenWeatherResponse($"weather?q={location}&units={units}", log);
                return CurrentWeatherResponse.FromJson(responseText);
            }
            catch (Exception e)
            {
                log.LogInformation(e, "Could not get current weather.");
                return null;
            }
        }

        public static async Task<WeatherForecastResponse> GetWeatherForecast(string location, string units, ILogger log)
        {
            try
            {
                var responseText = await MakeOpenWeatherResponse($"forecast?q={location}&units={units}", log);
                return WeatherForecastResponse.FromJson(responseText);
            }
            catch (Exception e)
            {
                log.LogInformation(e, "Could not get weather forecast.");
                return null;
            }
        }

        public static async Task<string> MakeOpenWeatherResponse(string urlAction, ILogger log)
        {
            var weatherKey = System.Environment.GetEnvironmentVariable("WEATHER_API_KEY", EnvironmentVariableTarget.Process).Nullable()
            ?? throw new ArgumentNullException("WEATHER_API_KEY");

            var weatherUrl = $"https://api.openweathermap.org/data/2.5/{urlAction}";
            log.LogInformation($"Requesting weatherUrl: {weatherUrl}&APPID={weatherKey}");

            var response = await Client.GetAsync($"{weatherUrl}&APPID={weatherKey}");
            response.EnsureSuccessStatusCode();
            return await response?.Content?.ReadAsStringAsync() ?? throw new InvalidDataException("Response was null");
        }
    }
}
