using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents the deserialized list of installed Epic Games Store applications.
/// </summary>
public class EpicInstalledAppList
{
    /// <summary>
    /// Gets or sets the list of installed Epic applications.
    /// </summary>
    [JsonPropertyName("InstallationList")]
    public IList<EpicInstalledApp> InstallationList { get; set; } = null!;
}
