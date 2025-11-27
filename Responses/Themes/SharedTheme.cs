using Newtonsoft.Json;
using System.Collections.Generic;

namespace MetroStart.Themes.Responses
{
    public class SharedTheme
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("online")]
        public bool Online { get; set; }

        [JsonProperty("themeContent")]
        public Dictionary<string, string> ThemeContent { get; set; }
    }
}