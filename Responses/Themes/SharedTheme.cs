using System.Text.Json.Serialization;

namespace MetroStart.Themes.Responses
{
    public class SharedTheme
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("author")]
        public string Author { get; set; } = string.Empty;

        [JsonPropertyName("online")]
        public bool Online { get; set; }

        [JsonPropertyName("themeContent")]
        public Dictionary<string, string> ThemeContent { get; set; } = [];
    }
}
