namespace Updater.Services;

/// <summary>
/// Represents the result of an update operation.
/// </summary>
internal class UpdateResult
{
    /// <summary>
    /// Gets or sets a value indicating whether the update operation succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the version string of the successfully installed update.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets the error message if the update operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user should perform a manual update.
    /// </summary>
    public bool RequiresManualUpdate { get; set; }
}
