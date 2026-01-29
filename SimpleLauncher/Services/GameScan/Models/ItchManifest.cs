using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.GameScan.Models;

public class ItchManifest
{
    [JsonPropertyName("actions")]
    public List<ItchAction> Actions { get; set; }
}