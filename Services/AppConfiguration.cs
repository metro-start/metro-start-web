namespace MetroStart.Services;

public sealed record AppConfiguration(
    string TableConnectionString,
    string WeatherApiKey)
{
    public static AppConfiguration FromEnvironment()
    {
        var tableConnectionString = Require("METROSTART_TABLE_CONNECTION_STRING");
        var weatherApiKey = Require("WEATHER_API_KEY");
        return new(tableConnectionString, weatherApiKey);
    }

    private static string Require(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"{name} is not configured.")
            : value;
    }
}
