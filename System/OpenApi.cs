using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace MetroStart.SystemEndpoints;

public sealed class OpenApi
{
    [Function("openapi")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "openapi.json")] HttpRequest request) =>
        new OkObjectResult(new
        {
            openapi = "3.1.0",
            info = new { title = "Metro Start API", version = "1.0.0" },
            paths = new Dictionary<string, object>
            {
                ["/api/weather"] = Operation("get", "Get cached weather"),
                ["/api/themes"] = Operation("get", "List shared themes"),
                ["/api/newtheme"] = Operation("post", "Share a theme"),
                ["/api/health"] = Operation("get", "Service health"),
            },
        });

    private static object Operation(string method, string summary) =>
        new Dictionary<string, object>
        {
            [method] = new
            {
                summary,
                responses = new Dictionary<string, object>
                {
                    ["200"] = new { description = "Successful response" },
                },
            },
        };
}
