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
    public static class NewTheme
    {
        [FunctionName("newtheme")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            var themeEntity = ThemeEntity.CreateThemeEntity(req.GetQueryParameterDictionary());
            if (await ThemeEntity.ThemeExists(themeEntity.Title, log))
            {
                return new ConflictResult();
            }

            var table = await ThemeEntity.GetCloudTable(log);
            await ThemeEntity.InsertTheme(themeEntity, table, log);
            return new OkResult();
        }
    }
}