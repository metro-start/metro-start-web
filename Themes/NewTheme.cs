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
using System.Linq;
using MetroStart.Entities;
using MetroStart.Helpers;
using MetroStart.Themes.Responses;

namespace MetroStart
{
    public static class NewTheme
    {
        [FunctionName("newtheme")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var jsonText = await req.ReadAsStringAsync();
            log.LogInformation(jsonText);
            var sharedTheme = JsonConvert.DeserializeObject<SharedTheme>(await req.ReadAsStringAsync());
            var themeEntity = ThemeHelpers.CreateThemeEntity(sharedTheme, log);
            if (await ThemeHelpers.ThemeExists(themeEntity.Title, log))
            {
                return new ConflictResult();
            }

            var table = await ThemeHelpers.GetCloudTable(log);
            await ThemeHelpers.InsertTheme(themeEntity, table, log);
            return new OkResult();
        }
    }
}