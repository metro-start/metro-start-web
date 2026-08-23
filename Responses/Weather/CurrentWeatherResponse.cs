namespace MetroStart.Weather.Responses
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class CurrentWeatherResponse
    {
        [JsonPropertyName("cod")]
        public long Cod { get; set; }

        [JsonPropertyName("coord")]
        public Coord? Coord { get; set; }

        [JsonPropertyName("weather")]
        public Weather[] Weather { get; set; } = [];

        [JsonPropertyName("base")]
        public string Base { get; set; } = string.Empty;

        [JsonPropertyName("main")]
        public Main Main { get; set; } = new();

        [JsonPropertyName("visibility")]
        public long Visibility { get; set; }

        [JsonPropertyName("wind")]
        public Wind? Wind { get; set; }

        [JsonPropertyName("clouds")]
        public Clouds? Clouds { get; set; }

        [JsonPropertyName("dt")]
        public long Dt { get; set; }

        [JsonPropertyName("sys")]
        public Sys Sys { get; set; } = new();

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        public static CurrentWeatherResponse? FromJson(string json) =>
            JsonSerializer.Deserialize<CurrentWeatherResponse>(json);

        public string ToJson() => JsonSerializer.Serialize(this);
    }
}
