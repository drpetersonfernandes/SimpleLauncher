using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Tests.TestHelpers;

/// <summary>
/// No-op implementation of <see cref="IResourceProvider"/> for unit tests.
/// Returns the key itself or the provided default value instead of loading localized resources.
/// </summary>
public class NoOpResourceProvider : IResourceProvider
{
    /// <summary>
    /// Returns the <paramref name="key"/> itself as the resource string.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <returns>The same <paramref name="key"/> value.</returns>
    public string GetString(string key)
    {
        return key;
    }

    /// <summary>
    /// Returns the <paramref name="defaultValue"/> instead of loading a localized resource.
    /// </summary>
    /// <param name="key">The resource key to look up.</param>
    /// <param name="defaultValue">The default value to return if the resource is not found.</param>
    /// <returns>The <paramref name="defaultValue"/>.</returns>
    public string GetString(string key, string defaultValue)
    {
        return defaultValue;
    }
}
