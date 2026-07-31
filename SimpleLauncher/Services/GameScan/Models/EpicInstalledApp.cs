using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.GameScan.Models;

public class EpicInstalledApp
{
    [JsonPropertyName("InstallLocation")]
    public string InstallLocation { get; set; } = null!;

    [JsonPropertyName("AppName")]
    public string AppName { get; set; } = null!;

    [JsonPropertyName("AppVersion")]
    public string AppVersion { get; set; } = null!;
}
