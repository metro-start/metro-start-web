namespace MetroStart.Weather.Respnoses
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using MetroStart.Weather.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class WeatherForecastResponse
    {
        [JsonProperty("cod")]
        public string Cod { get; set; }

        [JsonProperty("message")]
        public double Message { get; set; }

        [JsonProperty("city")]
        public City City { get; set; }

        [JsonProperty("cnt")]
        public long Count { get; set; }

        [JsonProperty("list")]
        public WeatherList[] WeatherList { get; set; }

        public static WeatherForecastResponse FromJson(string json) => JsonConvert.DeserializeObject<WeatherForecastResponse>(json, Converter.Settings);

        public string ToJson() => JsonConvert.SerializeObject(this, Converter.Settings);

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
                {
                    new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
                }
            };
        }
    }

    public class City
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("coord")]
        public Coord Coord { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class WeatherList
    {
        [JsonProperty("dt")]
        public long Dt { get; set; }

        [JsonProperty("main")]
        public Main Main { get; set; }

        [JsonProperty("weather")]
        public Weather[] Weather { get; set; }

        [JsonProperty("speed")]
        public Wind Wind { get; internal set; }

        [JsonProperty("rain")]
        public Rain Rain { get; internal set; }

        [JsonProperty("snow")]
        public Snow Snow { get; internal set; }
    }

    public class Wind
    {
        [JsonProperty("speed")]
        public long Speed { get; set; }
        [JsonProperty("deg")]
        public long Deg { get; set; }
    }

    public class Snow
    {
        [JsonProperty("3h")]
        public long ThreeHours { get; set; }
    }

    public class Rain
    {
        [JsonProperty("3h")]
        public long ThreeHours { get; set; }
    }

    public class Temp
    {
        [JsonProperty("day")]
        public double Day { get; set; }

        [JsonProperty("min")]
        public double Min { get; set; }

        [JsonProperty("max")]
        public double Max { get; set; }

        [JsonProperty("night")]
        public double Night { get; set; }

        [JsonProperty("eve")]
        public double Eve { get; set; }

        [JsonProperty("morn")]
        public double Morn { get; set; }
    }
}
