using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MetroStart.Entities;
using MetroStart.Themes.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MetroStart.Helpers
{
    public static class ThemeHelpers
    {
        public static async Task<CloudTable> GetCloudTable(ILogger log)
        {
            if (System.Environment.GetEnvironmentVariable("METROSTART_TABLE_CONNECTION_STRING", EnvironmentVariableTarget.Process) is var connectionString)
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("themes");

                // Create the table if it doesn't exist.
                await table.CreateIfNotExistsAsync();

                return table;
            }

            throw new ApplicationException("Could not find connectionString.");
        }

        public static ThemeEntity CreateThemeEntity(SharedTheme theme, ILogger log)
        {
            return new ThemeEntity(
                theme.Author.Nullable() ?? throw new ArgumentNullException(nameof(theme.Author)),
                theme.Title.Nullable() ?? throw new ArgumentNullException(nameof(theme.Title)),
                theme.Online,
                theme.ThemeContent);
        }

        public static ThemeEntity CreateThemeEntity(IDictionary<string, string> flatTheme, ILogger log)
        {
            string author = null;
            string title = null;
            bool online = true;
            Dictionary<string, string> themeContent = new Dictionary<string, string>();
            foreach (var (Key, Value) in flatTheme)
            {
                log.LogInformation($"Adding Key={Key}, Value={Value}");
                if (Key.Equals("author", StringComparison.InvariantCultureIgnoreCase))
                {
                    author = Value;
                }
                else if (Key.Equals("title", StringComparison.InvariantCultureIgnoreCase))
                {
                    title = Value;
                }
                else if (Key.Equals("online", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!bool.TryParse(Value, out online))
                    {
                        online = true;
                    }
                }
                else
                {
                    themeContent.Add(Key, Value);
                }
            }

            return new ThemeEntity(
                author.Nullable() ?? throw new ArgumentNullException(nameof(author)),
                title.Nullable() ?? throw new ArgumentNullException(nameof(title)),
                online,
                themeContent);
        }

        public static async Task<ThemeEntity> InsertTheme(ThemeEntity themeEntity, CloudTable table, ILogger log)
        {
            TableOperation insertOperation = TableOperation.Insert(themeEntity);

            // Execute the insert operation.
            log.LogDebug($"Saving new theme {themeEntity}");
            return (await table.ExecuteAsync(insertOperation))?.Result as ThemeEntity ?? throw new InvalidDataException($"Theme {themeEntity} was not saved");
        }

        public static async Task<List<ThemeEntity>> GetAllThemes(ILogger log)
        {
            var table = await GetCloudTable(log);
            TableContinuationToken tableToken = null;

            var results = new List<ThemeEntity>();
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(new TableQuery<ThemeEntity>(), tableToken) ?? throw new ApplicationException("No segment was returned.");
                results.AddRange(segment.Results);
                tableToken = segment.ContinuationToken;
            } while (tableToken != null);

            return results;
        }

        public static async Task<bool> ThemeExists(string title, ILogger log)
        {
            var table = await GetCloudTable(log);
            var results = await table.ExecuteQuerySegmentedAsync(
                new TableQuery<ThemeEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, title)),
                null) ?? throw new ApplicationException("No segment was returned.");

            return results?.Results.Count > 0;
        }
    }
}