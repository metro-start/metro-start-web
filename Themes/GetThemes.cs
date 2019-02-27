using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;

namespace MetroStart
{
    public static class GetThemes
    {
        [FunctionName("themes")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var themes = await ThemeEntity.GetAllThemes(log);

            return new OkObjectResult(
                JsonConvert.SerializeObject(
                    themes.Select(t => new
                    {
                        t.Author,
                        t.Title,
                        t.Online,
                        t.ThemeContent
                    })));
        }
    }
}