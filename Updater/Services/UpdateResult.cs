namespace Updater.Services;

/// <summary>
/// Represents the result of an update operation.
/// </summary>
public class UpdateResult
{
    public bool Success { get; set; }
    public string? Version { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresManualUpdate { get; set; }
}
