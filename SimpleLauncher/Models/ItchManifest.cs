using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

// Adapted from PlayniteExtensions LaunchManifest
public class ItchManifest
{
    [JsonPropertyName("actions")]
    public List<ItchAction> Actions { get; set; }
}

public class ItchAction
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("args")]
    public List<string> Args { get; set; }
}