namespace SimpleLauncher.Interfaces;

/// <summary>
/// Applies and tracks the current application theme.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Applies the specified base theme and accent color to the application.
    /// </summary>
    /// <param name="baseTheme">The base theme name to apply.</param>
    /// <param name="accentColor">The accent color name to apply.</param>
    void ApplyTheme(string baseTheme, string accentColor);

    /// <summary>
    /// Gets the currently applied base theme name.
    /// </summary>
    string CurrentBaseTheme { get; }

    /// <summary>
    /// Gets the currently applied accent color name.
    /// </summary>
    string CurrentAccentColor { get; }
}
