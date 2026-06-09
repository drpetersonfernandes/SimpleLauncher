namespace SimpleLauncher.Core.Interfaces;

public interface IThemeService
{
    void ApplyTheme(string baseTheme, string accentColor);
    string CurrentBaseTheme { get; }
    string CurrentAccentColor { get; }
}
