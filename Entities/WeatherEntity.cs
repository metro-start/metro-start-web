using Azure;
using Azure.Data.Tables;
using MetroStart.Weather.Responses;

namespace MetroStart.Entities
{
    public class WeatherEntity : ITableEntity
    {
        public WeatherEntity()
        {
        }

        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Location => RowKey;
        public string Units => PartitionKey;

        public DateTime CurrentWeatherModified { get; set; }
        public TimeSpan CurrentWeatherAge => DateTime.UtcNow - CurrentWeatherModified.ToUniversalTime();

        public DateTime WeatherForecastModified { get; set; }
        public TimeSpan WeatherForecastAge => DateTime.UtcNow - WeatherForecastModified.ToUniversalTime();

        public string CurrentWeatherJson { get; set; } = string.Empty;
        public CurrentWeatherResponse? CurrentWeather
        {
            get => CurrentWeatherResponse.FromJson(CurrentWeatherJson);
            set
            {
                CurrentWeatherJson = value?.ToJson() ?? string.Empty;
                CurrentWeatherModified = DateTime.UtcNow;
            }
        }

        public string WeatherForecastJson { get; set; } = string.Empty;
        public WeatherForecastResponse? WeatherForecast
        {
            get => WeatherForecastResponse.FromJson(WeatherForecastJson);
            set
            {
                WeatherForecastJson = value?.ToJson() ?? string.Empty;
                WeatherForecastModified = DateTime.UtcNow;
            }
        }

        public WeatherEntity(
            string location,
            string units,
            CurrentWeatherResponse? currentWeather,
            WeatherForecastResponse? weatherForecast)
        {
            PartitionKey = units;
            RowKey = location;

            CurrentWeather = currentWeather;
            CurrentWeatherModified = DateTime.UtcNow;

            WeatherForecast = weatherForecast;
            WeatherForecastModified = DateTime.UtcNow;
        }
    }
}
