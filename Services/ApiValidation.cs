using MetroStart.Themes.Responses;

namespace MetroStart.Services;

public static class ApiValidation
{
    public static bool IsValidWeatherRequest(string location, string units) =>
        !string.IsNullOrWhiteSpace(location) &&
        location.Length <= 100 &&
        !ContainsInvalidTableKeyCharacters(location) &&
        units is "metric" or "imperial";

    public static bool IsValidTheme(SharedTheme theme) =>
        !string.IsNullOrWhiteSpace(theme.Title) &&
        !string.IsNullOrWhiteSpace(theme.Author) &&
        theme.Title.Length <= 100 &&
        theme.Author.Length <= 100 &&
        theme.ThemeContent.Count <= 50 &&
        theme.ThemeContent.All(pair =>
            pair.Key.Length is > 0 and <= 100 &&
            pair.Value is not null &&
            pair.Value.Length <= 2_000) &&
        !ContainsInvalidTableKeyCharacters(theme.Title) &&
        !ContainsInvalidTableKeyCharacters(theme.Author);

    public static bool ContainsInvalidTableKeyCharacters(string value) =>
        value.IndexOfAny(['/', '\\', '#', '?']) >= 0 || value.Any(char.IsControl);
}
