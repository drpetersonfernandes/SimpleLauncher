using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
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
            ApplyLanguage(_settings.Language);
        }

        private void ApplyLanguage(string cultureCode = null)
        {
            try
            {
                // Determine the culture code (default to CurrentUICulture if not provided)
                var culture = string.IsNullOrEmpty(cultureCode)
                    ? CultureInfo.CurrentUICulture
                    : new CultureInfo(cultureCode);

                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;

                // Load the embedded resource dictionary
                string resourceName = $"SimpleLauncher.resources.strings.{culture.Name}.xaml";

                var assembly = typeof(App).Assembly;
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new Exception($"Resource {resourceName} not found.");
                    }

                    var dictionary = (ResourceDictionary)XamlReader.Load(stream);

                    // Replace the current localization dictionary
                    var existingDictionary = Resources.MergedDictionaries
                        .FirstOrDefault(d => d.Source?.OriginalString.Contains("strings.") ?? false);

                    if (existingDictionary != null)
                    {
                        Resources.MergedDictionaries.Remove(existingDictionary);
                    }

                    Resources.MergedDictionaries.Add(dictionary);
                }

                // Apply the culture to the application
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load language resources: {ex.Message}",
                    "Language Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to English
                ApplyFallbackLanguage();
            }
        }

        private void ApplyFallbackLanguage()
        {
            try
            {
                string resourceName = "SimpleLauncher.resources.strings.en.xaml";
                var assembly = typeof(App).Assembly;
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new Exception($"Fallback resource {resourceName} not found.");
                    }

                    var fallbackDictionary = (ResourceDictionary)XamlReader.Load(stream);
                    Resources.MergedDictionaries.Add(fallbackDictionary);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load fallback language resources: {ex.Message}",
                    "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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