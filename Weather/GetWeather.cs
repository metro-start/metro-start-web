using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using System.Data.SqlClient;
using System.Net.Http;
using MetroStart.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MetroStart
{
    public static class GetWeather
    {
        [FunctionName("weather")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string location = req.Query["location"].ToString() ?? throw new ArgumentNullException(nameof(location));
            string units = req.Query["units"].ToString() ?? throw new ArgumentNullException(nameof(units));

            return new OkObjectResult(
                (await GetCachedWeather(location, units, log))?.WeatherResponse ??
                (await CacheWeather(location, units, await GetCurrentWeather(location, units, log), log)).WeatherResponse);
        }

        static async Task<OpenWeatherResponse> GetCurrentWeather(string location, string units, ILogger log)
        {
            var weatherKey = System.Environment.GetEnvironmentVariable("WEATHER_API_KEY", EnvironmentVariableTarget.Process).Nullable()
            ?? throw new ArgumentNullException("WEATHER_API_KEY");

            var weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?q={location}&units={units}";
            log.LogDebug($"Requesting weatherUrl: {weatherUrl}");

            var response = await new HttpClient().GetAsync($"{weatherUrl}&APPID={weatherKey}");
            response.EnsureSuccessStatusCode();

            var responseText = await response?.Content?.ReadAsStringAsync() ?? throw new InvalidDataException("Response was null");
            return OpenWeatherResponse.FromJson(responseText) ?? throw new InvalidDataException("WeatherResponse was null");
        }

        static async Task<WeatherEntity> GetCachedWeather(string location, string units, ILogger log)
        {
            var table = await WeatherEntity.GetCloudTable(log);
            TableResult retrievedResult = await table.ExecuteAsync(TableOperation.Retrieve<WeatherEntity>(units, location));

            if (retrievedResult?.Result is WeatherEntity weatherEntity)
            {
                if (weatherEntity.Age < TimeSpan.FromHours(1))
                {
                    log.LogDebug($"Returning cached weather response: {weatherEntity.CreationDate}");
                    return weatherEntity;
                }

                var deleteOp = TableOperation.Delete(weatherEntity);
                TableResult deleteRes = await table.ExecuteAsync(deleteOp);
                log.LogInformation($"Removed expired weather with location: {location}, units: {units}, age: {weatherEntity.Age}");
            }

            return null;
        }

        static async Task<WeatherEntity> CacheWeather(string location, string units, OpenWeatherResponse weather, ILogger log)
        {
            var table = await WeatherEntity.GetCloudTable(log);

            var weatherEntity = new WeatherEntity(location, units, DateTime.Now, weather);
            TableOperation insertOperation = TableOperation.Insert(weatherEntity);

            // Execute the insert operation.
            log.LogDebug($"Caching current weather with location: {location}, units: {units}");
            return (await table.ExecuteAsync(insertOperation))?.Result as WeatherEntity ?? throw new InvalidDataException("Element was not cached");
        }
    }
}
