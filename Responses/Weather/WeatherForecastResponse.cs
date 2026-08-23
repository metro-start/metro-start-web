namespace MetroStart.Weather.Responses
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class WeatherForecastResponse
    {
        [JsonPropertyName("cod")]
        public string Cod { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public double Message { get; set; }

        [JsonPropertyName("city")]
        public City? City { get; set; }

        [JsonPropertyName("cnt")]
        public long Count { get; set; }

        [JsonPropertyName("list")]
        public WeatherList[] WeatherList { get; set; } = [];

        public static WeatherForecastResponse? FromJson(string json) =>
            JsonSerializer.Deserialize<WeatherForecastResponse>(json);

        public string ToJson() => JsonSerializer.Serialize(this);
    }

    public class City
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("coord")]
        public Coord? Coord { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }

    public class WeatherList
    {
        [JsonPropertyName("dt")]
        public long Dt { get; set; }

        [JsonPropertyName("main")]
        public Main Main { get; set; } = new();

        [JsonPropertyName("weather")]
        public Weather[] Weather { get; set; } = [];

        [JsonPropertyName("wind")]
        public Wind? Wind { get; set; }

        [JsonPropertyName("rain")]
        public Rain? Rain { get; set; }

        [JsonPropertyName("snow")]
        public Snow? Snow { get; set; }
    }

    public class Snow
    {
        [JsonPropertyName("3h")]
        public double ThreeHours { get; set; }
    }

    public class Rain
    {
        [JsonPropertyName("3h")]
        public double ThreeHours { get; set; }
    }

    public class Temp
    {
        [JsonPropertyName("day")]
        public double Day { get; set; }

        [JsonPropertyName("min")]
        public double Min { get; set; }

        [JsonPropertyName("max")]
        public double Max { get; set; }

        [JsonPropertyName("night")]
        public double Night { get; set; }

        [JsonPropertyName("eve")]
        public double Eve { get; set; }

        [JsonPropertyName("morn")]
        public double Morn { get; set; }
    }
}
