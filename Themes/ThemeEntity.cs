using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace MetroStart
{
    public class ThemeContent
    {
        public ThemeContent(string title_color, string background_color, string main_color, string options_color)
        {
            TitleColor = title_color;
            BackgroundColor = background_color;
            MainColor = main_color;
            OptionsColor = options_color;
        }

        public string TitleColor { get; set; }
        public string BackgroundColor { get; set; }
        public string MainColor { get; set; }
        public string OptionsColor { get; set; }
    }

    public class ThemeEntity : TableEntity
    {
        public ThemeEntity(string author, string title, Dictionary<string, string> themeContent)
        {
            PartitionKey = author;
            RowKey = title;
            ThemeContent = themeContent;
        }

        public ThemeEntity()
        {
        }

        public string Author => PartitionKey;
        public string Title => RowKey;
        public Dictionary<string, string> ThemeContent { get; set; }

        public static async Task<CloudTable> GetCloudTable(ILogger log)
        {
            if (System.Environment.GetEnvironmentVariable("METROSTART_TABLE_CONNECTION_STRING", EnvironmentVariableTarget.Process) is var connectionString)
            {
                log.LogInformation($"ConnectionString: {connectionString}");
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("theme");

                // Create the table if it doesn't exist.
                await table.CreateIfNotExistsAsync();

                return table;
            }

            throw new ApplicationException("Could not find connectionString.");
        }

        public static async Task<List<ThemeEntity>> GetAllThemes(ILogger log)
        {
            var table = await ThemeEntity.GetCloudTable(log);
            TableContinuationToken tableToken = null;

            var results = new List<ThemeEntity>();
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(new TableQuery<ThemeEntity>(), tableToken) ?? throw new ApplicationException("No segment was returned.");
                results.AddRange(segment.Results);
                tableToken = segment.ContinuationToken;
            } while (tableToken != null);

            log.LogDebug($"Returning cached weather response: {results.Count}");
            return results;
        }

        public static async Task<bool> ThemeExists(string title, ILogger log)
        {
            var table = await ThemeEntity.GetCloudTable(log);
            var results = await table.ExecuteQuerySegmentedAsync(
                new TableQuery<ThemeEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, title)),
                null) ?? throw new ApplicationException("No segment was returned.");

            return results?.Results.Count > 0;
        }
    }
}