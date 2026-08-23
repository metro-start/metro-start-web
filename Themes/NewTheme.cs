using MetroStart.Services;
using MetroStart.Themes.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Text.Json;

namespace MetroStart.Themes;

public sealed class NewTheme(
    ThemeService themes,
    RequestRateLimiter rateLimiter)
{
    [Function("newtheme")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!rateLimiter.TryAcquire(
            "newtheme",
            request.GetClientId(),
            10,
            TimeSpan.FromMinutes(1)))
        {
            return new StatusCodeResult(StatusCodes.Status429TooManyRequests);
        }
        if (request.ContentLength > 65_536)
        {
            return new BadRequestObjectResult("The theme payload is too large.");
        }

        SharedTheme? theme;
        try
        {
            theme = await request.ReadFromJsonAsync<SharedTheme>(cancellationToken);
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("The theme JSON is invalid.");
        }
        if (theme is null || !ApiValidation.IsValidTheme(theme))
        {
            return new BadRequestObjectResult("The theme is invalid.");
        }

        return await themes.TryAddAsync(theme, cancellationToken)
            ? new OkResult()
            : new ConflictResult();
    }
}
