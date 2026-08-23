using MetroStart.Services;
using MetroStart.Themes.Responses;

namespace MetroStart.Tests;

public class ValidationTests
{
    [Fact]
    public void WeatherRequestRequiresSupportedUnitsAndSafeLocation()
    {
        Assert.True(ApiValidation.IsValidWeatherRequest("Vancouver, CA", "metric"));
        Assert.False(ApiValidation.IsValidWeatherRequest("", "metric"));
        Assert.False(ApiValidation.IsValidWeatherRequest("Vancouver?admin=true", "metric"));
        Assert.False(ApiValidation.IsValidWeatherRequest("Vancouver", "kelvin"));
    }

    [Fact]
    public void ThemeValidationRejectsInvalidStorageKeysAndOversizedContent()
    {
        var theme = new SharedTheme
        {
            Author = "Metro Start",
            Title = "Clean",
            ThemeContent = new Dictionary<string, string> { ["mainColor"] = "#fff" },
        };

        Assert.True(ApiValidation.IsValidTheme(theme));

        theme.Title = "invalid/title";
        Assert.False(ApiValidation.IsValidTheme(theme));

        theme.Title = "Clean";
        theme.ThemeContent = Enumerable.Range(0, 51)
            .ToDictionary(index => index.ToString(), _ => "value");
        Assert.False(ApiValidation.IsValidTheme(theme));
    }
}
