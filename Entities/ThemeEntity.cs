using Azure;
using Azure.Data.Tables;
using System.Text.Json;

namespace MetroStart.Entities;

public class ThemeEntity : ITableEntity
{
    public ThemeEntity()
    {
    }
    public ThemeEntity(string author, string title, bool online, Dictionary<string, string> themeContent)
    {
        PartitionKey = author;
        RowKey = title;
        Online = online;
        ThemeContent = themeContent;
    }

    public string PartitionKey { get; set; } = string.Empty;
    public string RowKey { get; set; } = string.Empty;
    public string Author => PartitionKey;
    public string Title => RowKey;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public bool Online { get; set; }
    public string ThemeContentJson { get; set; } = "{}";
    public Dictionary<string, string> ThemeContent
    {
        get => string.IsNullOrEmpty(ThemeContentJson)
            ? []
            : JsonSerializer.Deserialize<Dictionary<string, string>>(ThemeContentJson) ?? [];
        set => ThemeContentJson = JsonSerializer.Serialize(value ?? []);
    }
    public override string ToString() => $"(Author: {Author}, Title: {Title})";
}
