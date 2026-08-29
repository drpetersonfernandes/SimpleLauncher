using System.Text.Json.Serialization;

namespace SimpleLauncher.Core.Models;

/// <summary>
///     Represents an installed Epic Games Store application with its location and version info.
/// </summary>
public class EpicInstalledApp
{
    /// <summary>
    ///     Gets or sets the installation directory path of the Epic app.
    /// </summary>
    [JsonPropertyName("InstallLocation")]
    public string InstallLocation { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the unique application name identifier.
    /// </summary>
    [JsonPropertyName("AppName")]
    public string AppName { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the version string of the installed application.
    /// </summary>
    [JsonPropertyName("AppVersion")]
    public string AppVersion { get; set; } = null!;
}