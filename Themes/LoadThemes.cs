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
using System.Net.Http;
using System.Linq;
using MetroStart.Entities;
using MetroStart.Helpers;

namespace MetroStart
{
    public static class LoadThemes
    {
        [FunctionName("loadthemes")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var table = await ThemeHelpers.GetCloudTable(log);
            var themeEntities = (await GetThemesFromGoogle(log)).Select(t => ThemeHelpers.CreateThemeEntity(t));
            foreach (var themeEntity in themeEntities)
            {
                try
                {
                    await ThemeHelpers.InsertTheme(themeEntity, table, log);
                }
                catch (Exception e)
                {
                    log.LogDebug($"Exception saving theme {themeEntity}: {e}");
                }
            }

            return new OkObjectResult(JsonConvert.SerializeObject(themeEntities));
        }

        static async Task<List<Dictionary<string, string>>> GetThemesFromGoogle(ILogger log)
        {
            var themesUrl = $"https://metro-start.appspot.com/themes.json";
            log.LogDebug($"Requesting themesUrl: {themesUrl}");

            var response = await new HttpClient().GetAsync($"{themesUrl}");
            response.EnsureSuccessStatusCode();

            var responseText = await response?.Content?.ReadAsStringAsync() ?? throw new InvalidDataException("Response was null");
            return JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(responseText) ?? throw new InvalidDataException("Deserialized object was null");
        }
    }
}