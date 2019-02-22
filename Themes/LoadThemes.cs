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

namespace MetroStart
{
    public static class LoadThemes
    {
        [FunctionName("_loadthemes")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var themes = await GetThemesFromGoogle(log);
            foreach (var theme in themes)
            {
                var themeEntity = ThemeEntity.CreateThemeEntity(theme);
                if (await ThemeEntity.ThemeExists(themeEntity.Title, log))
                {
                    return new ConflictResult();
                }

                await ThemeEntity.InsertTheme(themeEntity, log);
                return new OkResult();
            }

            return new OkObjectResult(JsonConvert.SerializeObject(themes));
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