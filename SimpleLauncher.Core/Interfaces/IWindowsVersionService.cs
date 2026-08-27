namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides a method to retrieve the current Windows version string.
/// </summary>
public interface IWindowsVersionService
{
    /// <summary>
    /// Gets the current Windows version as a string.
    /// </summary>
    /// <returns>The Windows version string.</returns>
    string GetVersion();
}