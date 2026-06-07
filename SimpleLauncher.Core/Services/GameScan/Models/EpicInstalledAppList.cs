using System.Text.Json.Serialization;

namespace SimpleLauncher.Core.Services.GameScan.Models;

public class EpicInstalledAppList
{
    [JsonPropertyName("InstallationList")]
    public List<EpicInstalledApp> InstallationList { get; set; }
}