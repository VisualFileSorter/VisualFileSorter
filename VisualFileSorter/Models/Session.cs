using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VisualFileSorter.Models
{
    public class Session
    {
        public Session()
        {
            FileQueue = new List<string>();
            SortFolders = new List<SortFolderJson>();
        }

        public string Serialize()
        {
            var options = new JsonSerializerOptions{ WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        public Session Deserialize(string sessionJson)
        {
            var options = new JsonSerializerOptions { AllowTrailingCommas = true };
            return JsonSerializer.Deserialize<Session>(sessionJson, options);
        }

        public List<string> FileQueue { get; set; }
        public List<SortFolderJson> SortFolders { get; set; }
    }

    public class SortFolderJson
    {
        public SortFolderJson()
        {
            SortSrcFiles = new List<string>();
        }

        public string FullName { get; set; }
        public string Shortcut { get; set; }
        public List<string> SortSrcFiles { get; set; }
    }
}
