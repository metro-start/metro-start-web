using System;
using System.Threading.Tasks;
using MetroStart.Weather.Respnoses;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace MetroStart.Entities
{
   public class  WeatherEntity : TableEntity
    {
        public WeatherEntity(string location, string units, CurrentWeatherResponse currentWeather, WeatherForecastResponse weatherForecast)
        {
            PartitionKey = units;
            RowKey = location;

            CurrentWeather = currentWeather;
            CurrentWeatherModified = DateTime.Now;

            WeatherForecast = weatherForecast;
            CurrentWeatherModified = DateTime.Now;
        }

        public WeatherEntity()
        {
        }

        public string Location => RowKey;
        public string Units => PartitionKey;

        public DateTime CurrentWeatherModified { get; set; }
        public TimeSpan CurrentWeatherAge => DateTime.Now - CurrentWeatherModified;

        public DateTime WeatherForecastModified { get; set; }
        public TimeSpan WeatherForecastAge => DateTime.Now - CurrentWeatherModified;

        public string CurrentWeatherJson { get; set; }
        public CurrentWeatherResponse CurrentWeather
        {
            get => CurrentWeatherResponse.FromJson(CurrentWeatherJson);
            set => CurrentWeatherJson = value.ToJson();
        }

        public string WeatherForecastJson { get; set; }
        public WeatherForecastResponse WeatherForecast
        {
            get => WeatherForecastResponse.FromJson(WeatherForecastJson);
            set => WeatherForecastJson =value.ToJson();
        }

        public static async Task<CloudTable> GetCloudTable(ILogger log)
        {
            _ = log;
            if (Environment.GetEnvironmentVariable("METROSTART_TABLE_CONNECTION_STRING", EnvironmentVariableTarget.Process) is var connectionString)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("weather");

                // Create the table if it doesn't exist.
                await table.CreateIfNotExistsAsync();

                return table;
            }

            throw new ApplicationException("Could not find connectionString.");
        }
    }
}