using System.Windows;
using ControlzEx.Theming;

namespace SimpleLauncher
{
    public partial class App
    {
        private static SettingsConfig _settings;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _settings = new SettingsConfig();
            ApplyTheme(_settings.BaseTheme, _settings.AccentColor);
        }

        public static void ChangeTheme(string baseTheme, string accentColor)
        {
            ApplyTheme(baseTheme, accentColor);
            _settings.BaseTheme = baseTheme;
            _settings.AccentColor = accentColor;
            _settings.Save();
        }

        private static void ApplyTheme(string baseTheme, string accentColor)
        {
            ThemeManager.Current.ChangeTheme(Application.Current, $"{baseTheme}.{accentColor}");
        }

        public static void ApplyThemeToWindow(Window window)
        {
            string baseTheme = _settings.BaseTheme;
            string accentColor = _settings.AccentColor;
            ThemeManager.Current.ChangeTheme(window, $"{baseTheme}.{accentColor}");
        }
    }
}