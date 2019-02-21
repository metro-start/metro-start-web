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
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {

            string author = null;
            string title = null;
            Dictionary<string, string> themeContent = new Dictionary<string, string>();
            foreach (var (Key, Value) in req.GetQueryParameterDictionary())
            {
                if (Key.Equals("author", StringComparison.InvariantCultureIgnoreCase))
                {
                    author = Value;
                }
                else if (Key.Equals("title", StringComparison.InvariantCultureIgnoreCase))
                {
                    title = Value;
                }
                else
                {
                    themeContent.Add(Key, Value);
                }
            }

            _ = author.Nullable() ?? throw new ArgumentNullException(nameof(author));
            _ = title.Nullable() ?? throw new ArgumentNullException(nameof(title));

            if (await ThemeEntity.ThemeExists(title, log))
            {
                return new ConflictResult();
            }

            await InsertTheme(author, title, themeContent, log);
            return new OkResult();
        }

        static async Task<ThemeEntity> InsertTheme(string author, string title, Dictionary<string, string> themeContent, ILogger log)
        {
            var table = await ThemeEntity.GetCloudTable(log);

            var weatherEntity = new ThemeEntity(author, title, themeContent);
            TableOperation insertOperation = TableOperation.Insert(weatherEntity);

            // Execute the insert operation.
            log.LogDebug($"Saving new theme with author: {author}, title: {title}");
            return (await table.ExecuteAsync(insertOperation))?.Result as ThemeEntity ?? throw new InvalidDataException("Element was not cahced");
        }
    }
}