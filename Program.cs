using Azure.Data.Tables;
using MetroStart.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.UseMiddleware<ApiExceptionMiddleware>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();
builder.Services.AddSingleton(AppConfiguration.FromEnvironment());
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMemoryCache(options => options.SizeLimit = 1_000);
builder.Services.AddSingleton(provider => new TableServiceClient(
    provider.GetRequiredService<AppConfiguration>().TableConnectionString));
builder.Services.AddSingleton<RequestRateLimiter>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<WeatherService>();
builder.Services.AddHttpClient<OpenWeatherClient>(client =>
{
    client.BaseAddress = new("https://api.openweathermap.org/data/2.5/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

builder.Build().Run();
