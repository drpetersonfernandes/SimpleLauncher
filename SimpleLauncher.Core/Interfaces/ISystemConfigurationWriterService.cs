namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Persists system configuration entries to the system XML file.
/// </summary>
public interface ISystemConfigurationWriterService
{
    /// <summary>
    ///     Asynchronously saves a system configuration to the XML file, creating or updating the entry.
    /// </summary>
    /// <param name="systemConfig">The system configuration to save.</param>
    /// <param name="originalSystemName">The original name of the system when renaming, or null when adding a new system.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveSystemAsync(ISystemManager systemConfig, string? originalSystemName = null);

    /// <summary>
    ///     Asynchronously deletes a system configuration entry by name from the XML file.
    /// </summary>
    /// <param name="systemName">The name of the system to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSystemAsync(string systemName);

    /// <summary>
    ///     Checks whether a system configuration with the specified name exists in the XML file.
    /// </summary>
    /// <param name="systemName">The name of the system to check.</param>
    /// <returns>True if the system exists; otherwise, false.</returns>
    bool SystemExists(string systemName);
}