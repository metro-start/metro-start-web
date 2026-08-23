namespace MetroStart.Weather.Responses;

using System.Text.Json.Serialization;

public class Wind
{
    [JsonPropertyName("speed")]
    public double Speed { get; set; }

    [JsonPropertyName("deg")]
    public long Deg { get; set; }
}
