namespace MetroStart.Weather.Responses
{
    using System.Text.Json.Serialization;

    public class Sys
    {
        [JsonPropertyName("type")]
        public long Type { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("message")]
        public double Message { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("sunrise")]
        public long Sunrise { get; set; }

        [JsonPropertyName("sunset")]
        public long Sunset { get; set; }
    }
}
