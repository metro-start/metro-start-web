using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace MetroStart.Entities
{
    public class ThemeEntity : TableEntity
    {
        public ThemeEntity(string author, string title, bool online, Dictionary<string, string> themeContent)
        {
            PartitionKey = author;
            RowKey = title;
            Online = online;
            ThemeContent = themeContent;
        }

        public ThemeEntity()
        {
        }

        public string Author => PartitionKey;
        public string Title => RowKey;
        public bool Online { get; set; }
        public string ThemeContentJson { get; set; }
        public Dictionary<string, string> ThemeContent
        {
            get => JsonConvert.DeserializeObject<Dictionary<string, string>>(ThemeContentJson);
            set => ThemeContentJson = JsonConvert.SerializeObject(value);
        }

        public override string ToString() => $"(Author: {Author}, Title: {Title})";
    }
}