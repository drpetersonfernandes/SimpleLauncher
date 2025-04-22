using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using ControlzEx.Theming;
using SimpleLauncher.Services;
using System.Reflection;

namespace SimpleLauncher;

public partial class App
{
    public static SettingsManager Settings { get; private set; }

    // --- Add fields for single instance logic ---
    private Mutex _singleInstanceMutex;

    private bool _isFirstInstance;

    // Use a unique name for the mutex, e.g., based on the assembly GUID
    private const string MutexNamePrefix = "SimpleLauncher_SingleInstanceMutex_";
    // --- End fields for single instance logic ---


    protected override void OnStartup(StartupEventArgs e)
    {
        // --- Single Instance Check ---
        // Check if the application is being restarted via a specific command-line argument
        var isRestarting = e.Args.Any(static arg => arg.Equals("--restarting", StringComparison.OrdinalIgnoreCase));

        if (!isRestarting) // Only perform the mutex check if NOT restarting
        {
            try
            {
                // Generate a unique name based on the assembly GUID
                var assemblyGuid = Assembly.GetExecutingAssembly().GetType().GUID.ToString();
                var uniqueMutexName = $"{MutexNamePrefix}{assemblyGuid}";

                // Try to create or open the mutex
                // The 'out _isFirstInstance' parameter will be true if the mutex was created (first instance)
                // and false if it already existed (another instance is running).
                _singleInstanceMutex = new Mutex(true, uniqueMutexName, out _isFirstInstance);

                if (!_isFirstInstance)
                {
                    // Another instance is running. Inform the user (optional) and exit this instance.
                    // You might want to bring the existing instance to the foreground here instead of just showing a message.
                    // This requires finding the existing window handle, which is more complex.
                    // For now, we'll just show a message and exit.
                    MessageBoxLibrary.AnotherInstanceIsRunningMessageBox();

                    Shutdown(); // Exit the application

                    return; // Stop further startup logic
                }
            }
            catch (Exception ex)
            {
                // Handle potential errors during mutex creation (e.g., permissions issues)
                // Log the error and decide whether to proceed or exit.
                // Exiting is safer if we can't guarantee single instance.
                _ = LogErrors.LogErrorAsync(ex, "Failed to create or acquire single instance mutex.");

                MessageBoxLibrary.FailedToStartSimpleLauncherMessageBox();

                Shutdown();

                return;
            }
        }
        // --- End Single Instance Check ---

        // If we are the first instance (_isFirstInstance is true) OR we are restarting, proceed with normal startup
        base.OnStartup(e);
        Settings = new SettingsManager();
        ApplyTheme(Settings.BaseTheme, Settings.AccentColor);
        ApplyLanguage(Settings.Language);

        // If we are restarting, the MainWindow will be shown by StartupUri="MainWindow.xaml"
        // If we are the first instance, the MainWindow is also shown by StartupUri.
        // No extra Show() call is needed here.
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Release the mutex if this was the first instance and the mutex was successfully created
        // The restarting instance *was* the first instance, so it will release the mutex here.
        // The new instance (started with --restarting) didn't acquire the mutex, so _isFirstInstance will be false,
        // and it won't try to release it.
        if (_singleInstanceMutex != null && _isFirstInstance)
        {
            try
            {
                _singleInstanceMutex.ReleaseMutex();
                _singleInstanceMutex.Dispose();
            }
            catch (Exception ex)
            {
                _ = LogErrors.LogErrorAsync(ex, "Failed to release single instance mutex on exit.");
            }
        }

        base.OnExit(e);
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

            // Find and remove ALL existing localization dictionaries
            // Use ToList() to avoid modifying the collection while enumerating it
            var existingDictionaries = Resources.MergedDictionaries
                .Where(static d => d.Source?.OriginalString.Contains("strings.") ?? false)
                .ToList();

            foreach (var existingDictionary in existingDictionaries)
            {
                Resources.MergedDictionaries.Remove(existingDictionary);
            }

            // Create the resource dictionary for the requested culture
            var dictionary = new ResourceDictionary
            {
                Source = new Uri($"/resources/strings.{culture.Name}.xaml", UriKind.Relative)
            };

            try
            {
                // Attempt to add the new dictionary
                Resources.MergedDictionaries.Add(dictionary);

                // Apply the culture to the application
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
            }
            catch (Exception innerEx)
            {
                // Handle failure to load the requested language resource file
                var contextMessage = $"Failed to load language resources for {culture.Name} (requested {cultureCode}).";
                _ = LogErrors.LogErrorAsync(innerEx, contextMessage);

                // Notify user
                MessageBoxLibrary.FailedToLoadLanguageResourceMessageBox();

                // Fallback to English
                var fallbackDictionary = new ResourceDictionary
                {
                    Source = new Uri("/resources/strings.en.xaml", UriKind.Relative)
                };
                // Ensure the fallback is added even if the requested one failed
                Resources.MergedDictionaries.Add(fallbackDictionary);

                // Log the fallback usage (optional but helpful for debugging)
                _ = LogErrors.LogErrorAsync(new Exception("Fallback to English language resources."), "Using fallback language.");
            }
        }
        catch (Exception ex)
        {
            // This outer catch handles errors *before* attempting to load the new dictionary
            // (e.g., invalid cultureCode format).
            var contextMessage = $"Failed to determine or set culture for {cultureCode}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user (reusing the same message for simplicity)
            MessageBoxLibrary.FailedToLoadLanguageResourceMessageBox();

            // Ensure a language dictionary is present - add English fallback if none exists
            if (!Resources.MergedDictionaries.Any(static d => d.Source?.OriginalString.Contains("strings.") ?? false))
            {
                var fallbackDictionary = new ResourceDictionary
                {
                    Source = new Uri("/resources/strings.en.xaml", UriKind.Relative)
                };
                Resources.MergedDictionaries.Add(fallbackDictionary);
                _ = LogErrors.LogErrorAsync(new Exception("Fallback to English language resources due to initial culture error."), "Using fallback language.");
            }
        }
    }

    private static void ApplyTheme(string baseTheme, string accentColor)
    {
        try
        {
            ThemeManager.Current.ChangeTheme(Current, $"{baseTheme}.{accentColor}");
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to Apply Theme.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    public static void ApplyThemeToWindow(Window window)
    {
        var baseTheme = Settings.BaseTheme;
        var accentColor = Settings.AccentColor;
        ThemeManager.Current.ChangeTheme(window, $"{baseTheme}.{accentColor}");
    }

    public static void ChangeTheme(string baseTheme, string accentColor)
    {
        ApplyTheme(baseTheme, accentColor);
        Settings.BaseTheme = baseTheme;
        Settings.AccentColor = accentColor;
        Settings.Save();
    }
}