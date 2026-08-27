namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides localized string resources for the application.
/// </summary>
public interface IResourceProvider
{
    /// <summary>
    /// Gets a localized string resource by key, returning the key itself if not found.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <returns>The localized string, or the key if the resource is not found.</returns>
    string GetString(string key);

    /// <summary>
    /// Gets a localized string resource by key, returning the specified default value if not found.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <param name="defaultValue">The default value to return if the resource is not found.</param>
    /// <returns>The localized string, or the default value if the resource is not found.</returns>
    string GetString(string key, string defaultValue);
}