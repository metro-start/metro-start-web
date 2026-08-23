using Microsoft.AspNetCore.Http;

namespace MetroStart.Services;

public static class HttpRequestExtensions
{
    public static string GetClientId(this HttpRequest request) =>
        request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
