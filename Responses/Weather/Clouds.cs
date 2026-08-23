namespace MetroStart.Weather.Responses;

using System.Text.Json.Serialization;

public class Clouds
{
    [JsonPropertyName("all")]
    public long All { get; set; }
}
