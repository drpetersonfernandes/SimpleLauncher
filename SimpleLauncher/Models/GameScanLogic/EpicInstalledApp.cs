using System.Text.Json.Serialization;

namespace SimpleLauncher.Models.GameScanLogic;

public class EpicInstalledApp
{
    [JsonPropertyName("InstallLocation")]
    public string InstallLocation { get; set; }

    [JsonPropertyName("AppName")]
    public string AppName { get; set; }

    [JsonPropertyName("AppVersion")]
    public string AppVersion { get; set; }
}