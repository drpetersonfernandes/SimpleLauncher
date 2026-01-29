using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.GameScan.Models;

public class ItchAction
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("args")]
    public List<string> Args { get; set; }
}