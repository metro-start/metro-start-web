using MetroStart.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System.Security.Cryptography;
using System.Text;

namespace MetroStart.Themes;

public sealed class GetThemes(ThemeService themes)
{
    [Function("themes")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var pageSize = int.TryParse(request.Query["pageSize"], out var requestedSize)
            ? Math.Clamp(requestedSize, 1, 100)
            : 100;
        var page = await themes.GetPageAsync(
            pageSize,
            request.Query["continuationToken"],
            cancellationToken);

        var fingerprint = string.Join('|', page.Items.Select(theme =>
            $"{theme.PartitionKey}:{theme.RowKey}:{theme.Timestamp:O}:{theme.ETag}"));
        var etag = $"\"{Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(fingerprint)))}\"";
        if (request.Headers.IfNoneMatch == etag)
        {
            return new StatusCodeResult(StatusCodes.Status304NotModified);
        }
        request.HttpContext.Response.Headers.CacheControl = "public, max-age=300";
        request.HttpContext.Response.Headers.ETag = etag;
        request.HttpContext.Response.Headers["X-API-Version"] = "1";
        if (page.ContinuationToken is not null)
        {
            request.HttpContext.Response.Headers["X-Continuation-Token"] =
                page.ContinuationToken;
        }

        return new OkObjectResult(page.Items.Select(theme => new
        {
            Id = $"{theme.Author}:{theme.Title}",
            theme.Author,
            theme.Title,
            theme.Online,
            theme.ThemeContent,
        }));
    }
}
