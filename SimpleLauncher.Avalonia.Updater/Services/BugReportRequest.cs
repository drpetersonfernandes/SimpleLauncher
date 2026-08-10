using System.Text.Json.Serialization;

namespace SimpleLauncher.Avalonia.Updater.Services;

/// <summary>
/// Data transfer object for bug report requests
/// </summary>
internal class BugReportRequest
{
    /// <summary>
    /// Gets or sets the detailed error message and context information.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the name of the application that generated the bug report.
    /// </summary>
    [JsonPropertyName("applicationName")]
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the version of the application that generated the bug report.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets information about the user or machine that generated the bug report.
    /// </summary>
    [JsonPropertyName("userInfo")]
    public string? UserInfo { get; set; }

    /// <summary>
    /// Gets or sets the environment name (e.g., Debug or Release) where the bug occurred.
    /// </summary>
    [JsonPropertyName("environment")]
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets the complete stack trace of the exception.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    public string? StackTrace { get; set; }
}
