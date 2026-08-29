namespace SimpleLauncher.Core.Models;

/// <summary>
///     Represents a system configuration entry with its name and helper text description.
/// </summary>
public class SystemHelper
{
    /// <summary>
    ///     The name of the system.
    /// </summary>
    public string SystemName { get; init; } = null!;

    /// <summary>
    ///     The helper text or description for the system.
    /// </summary>
    public string SystemHelperText { get; init; } = null!;
}