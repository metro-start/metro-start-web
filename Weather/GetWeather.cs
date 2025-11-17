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
using MetroStart.Helpers;

namespace MetroStart.Weather
{
    public static class GetWeather
    {
        [FunctionName("weather")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                string location = req.Query["location"].ToString() ?? throw new ArgumentNullException(nameof(location));
                string units = req.Query["units"].ToString() ?? throw new ArgumentNullException(nameof(units));

                List<Task<bool>> updateTasks = new List<Task<bool>>();
                WeatherEntity weather = await WeatherHelpers.GetCachedWeather(location, units, log)
                                        ?? new WeatherEntity(location, units, null, null);

                updateTasks.Add(Task.Run(async () =>
                {
                    if (WeatherHelpers.CurrentWeatherRequiresUpdate(weather))
                    {
                        try
                        {
                            weather.CurrentWeather = await WeatherHelpers.GetCurrentWeather(location, units, log);
                            weather.CurrentWeatherModified = DateTime.Now;
                            return true;
                        }
                        catch (Exception e)
                        {
                            log.LogError(e, $"Could not update the current weather for: {location}, {units}");
                        }
                    }
                    else
                    {
                        log.LogInformation($"Current weather update not required for: {location}, {units}");
                    }
                    return false;
                }));
                updateTasks.Add(Task.Run(async () =>
                {
                    if (WeatherHelpers.WeatherForecastRequiresUpdate(weather))
                    {
                        try
                        {
                            weather.WeatherForecast = await WeatherHelpers.GetWeatherForecast(location, units, log);
                            weather.WeatherForecastModified = DateTime.Now;
                            return true;
                        }
                        catch (Exception e)
                        {
                            log.LogError(e, $"Could not update the weather forecast for: {location}, {units}");
                        }
                    }
                    else
                    {
                        log.LogInformation($"Weather forecast update not required for: {location}, {units}");
                    }
                    return false;
                }));

                if ((await Task.WhenAll(updateTasks)).Any(res => res))
                {
                    await WeatherHelpers.CacheWeather(weather, log);
                }

                var firstForecast = weather.WeatherForecast?.WeatherList?.FirstOrDefault();
                return new OkObjectResult(new
                {
                    // Location
                    weather.CurrentWeather.Name,
                    weather.CurrentWeather.Sys.Country,

                    // Temperatures
                    weather.CurrentWeather.Main.Temp,
                    firstForecast.Main.TempMax,
                    firstForecast.Main.TempMin,
                    weather.Units,

                    // Conditions
                    weather.CurrentWeather.Weather?.FirstOrDefault()?.Description,
                    WindSpeed = firstForecast?.Wind?.Speed ?? 0,
                    WindDeg = firstForecast?.Wind?.Deg ?? 0,
                    Rain3h = firstForecast?.Rain?.ThreeHours ?? 0,
                    Snow3h = firstForecast?.Snow?.ThreeHours ?? 0,

                    // Ages
                    EntityAge = (DateTime.Now - weather.Timestamp.ToLocalTime()).ToHumanReadableString(),
                    CurrentWeatherAge = weather.CurrentWeatherAge.ToHumanReadableString(),
                    WeatherForecastAge = weather.WeatherForecastAge.ToHumanReadableString()
                });
            }
            catch (Exception e)
            {
                log.LogWarning(e, $"Exception! {e.Message}! {e.StackTrace}");
                throw e;
            }
        }
    }
}
