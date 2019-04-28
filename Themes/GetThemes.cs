using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using MetroStart.Helpers;

namespace MetroStart
{
    public static class GetThemes
    {
        [FunctionName("themes")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
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