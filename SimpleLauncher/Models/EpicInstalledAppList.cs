using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

public class EpicInstalledAppList
{
    [JsonPropertyName("InstallationList")]
    public IList<EpicInstalledApp> InstallationList { get; set; } = null!;
}
