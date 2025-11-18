using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MetroStart.Helpers;
using Microsoft.Azure.Functions.Worker;

namespace MetroStart
{
    public class GetThemes
    {
        [Function("themes")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger<GetThemes> log)
        {
            var themes = await ThemeHelpers.GetAllThemes(log);

            return new OkObjectResult(
                    themes.Select(t => new
                    {
                        t.Author,
                        t.Title,
                        t.Online,
                        t.ThemeContent
                    }));
        }
    }
}