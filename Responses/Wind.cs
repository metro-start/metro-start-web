namespace MetroStart.Weather.Models
{
    using Newtonsoft.Json;

    public class Wind
    {
        [JsonProperty("speed")]
        public long Speed { get; set; }

        [JsonProperty("deg")]
        public long Deg { get; set; }
    }
}
