using System;
using System.Threading.Tasks;
using MetroStart.Models;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace MetroStart.Models
{
    public class WeatherEntity : TableEntity
    {
        public WeatherEntity(string location, string units, DateTime creationDate, OpenWeatherResponse weatherResponse)
        {
            PartitionKey = units;
            RowKey = location;

            CreationDate = creationDate;
            WeatherResponse = weatherResponse;
        }

        public WeatherEntity()
        {
        }

        public string Location => RowKey;
        public string Units => PartitionKey;

        public DateTime CreationDate { get; set; }
        public TimeSpan Age => DateTime.Now - CreationDate;

        public string WeatherResponseJson { get; set; }
        public OpenWeatherResponse WeatherResponse
        {
            get => JsonConvert.DeserializeObject<OpenWeatherResponse>(WeatherResponseJson);
            set => WeatherResponseJson = JsonConvert.SerializeObject(value);
        }

        public static async Task<CloudTable> GetCloudTable(ILogger log)
        {
            if (System.Environment.GetEnvironmentVariable("METROSTART_TABLE_CONNECTION_STRING", EnvironmentVariableTarget.Process) is var connectionString)
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