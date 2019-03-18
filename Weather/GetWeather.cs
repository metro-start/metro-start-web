using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MetroStart.Entities;
using MetroStart.Weather.Respnoses;
using System.Collections.Generic;
using System.Linq;

namespace MetroStart.Weather
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

            List<Task> updateTasks = new List<Task>();
            WeatherEntity weather = await WeatherHelpers.GetCachedWeather(location, units, log);
            updateTasks.Add(Task.Run(async () => await WeatherHelpers.UpdateCurrentWeather(weather, location, units, log)));
            updateTasks.Add(Task.Run(async () => await WeatherHelpers.UpdateWeatherForecast(weather, location, units, log)));

            if (updateTasks.Any())
            {
                await Task.WhenAll(updateTasks);
                await WeatherHelpers.CacheWeather(weather, log);
            }

            WeatherList firstForecast = weather.WeatherForecast.WeatherList?.FirstOrDefault();
            return new OkObjectResult(new
            {
                // Location
                weather.CurrentWeather.Name,
                weather.CurrentWeather.Sys.Country,

                // Temperatures
                weather.CurrentWeather.Main.Temp,
                firstForecast?.Temp.Max,
                firstForecast?.Temp.Min,
                weather.Units,

                // Conditions
                weather.CurrentWeather.Weather?.FirstOrDefault()?.Description,
                firstForecast?.Speed,
                firstForecast?.Rain,
                firstForecast?.Snow
            });
        }

    }
}
