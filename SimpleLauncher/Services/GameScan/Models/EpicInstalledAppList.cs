using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.GameScan.Models;

public class EpicInstalledAppList
{
    [JsonPropertyName("InstallationList")]
    public List<EpicInstalledApp> InstallationList { get; set; }
}