using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using ControlzEx.Theming;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services;
using SimpleLauncher.Managers;

namespace SimpleLauncher;

public partial class App
{
    public static SettingsManager Settings { get; private set; }
    public static IServiceProvider ServiceProvider { get; private set; }

    // --- Add fields for single instance logic ---
    private Mutex _singleInstanceMutex;

    private bool _isFirstInstance;

    private const string UniqueMutexIdentifier = "A8E2B9C1-F5D7-4E0A-8B3C-6D1E9F0A7B4C";

    private const string MutexName = "SimpleLauncher_SingleInstanceMutex_" + UniqueMutexIdentifier;
    // --- End fields for single instance logic ---

    protected override void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        // Register IHttpClientFactory and named clients
        serviceCollection.AddHttpClient("LogErrorsClient");
        serviceCollection.AddHttpClient("StatsClient");
        serviceCollection.AddHttpClient("UpdateCheckerClient");
        serviceCollection.AddHttpClient("SupportWindowClient");
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // --- Single Instance Check ---
        // Catch args
        var isRestarting = e.Args.Any(static arg => arg.Equals("--restarting", StringComparison.OrdinalIgnoreCase));
        var isDebugMode = e.Args.Any(static arg => arg.Equals("-debug", StringComparison.OrdinalIgnoreCase));
        var displayHistoryWindow = e.Args.Any(static arg => arg.Equals("-whatsnew", StringComparison.OrdinalIgnoreCase));

        // Initialize DebugLogger early
        DebugLogger.Initialize(isDebugMode);

        if (displayHistoryWindow)
        {
            try
            {
                var updateHistoryWindow = new UpdateHistoryWindow();
                updateHistoryWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error in the OnStartup method.";
                DebugLogger.LogException(ex, contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }

        if (!isRestarting) // Only perform the mutex check if NOT restarting
        {
            try
            {
                // Try to create or open the mutex
                // The 'out _isFirstInstance' parameter will be true if the mutex was created (first instance)
                // and false if it already existed (another instance is running).
                _singleInstanceMutex = new Mutex(true, MutexName, out _isFirstInstance);

                if (!_isFirstInstance)
                {
                    // Another instance is running. Inform the user (optional) and exit this instance.
                    MessageBoxLibrary.AnotherInstanceIsRunningMessageBox();

                    Shutdown(); // Exit the application

                    return; // Stop further startup logic
                }
            }
            catch (Exception ex)
            {
                // Log the error using the new DebugLogger (if initialized) and old LogErrors (always)
                DebugLogger.LogException(ex, "Failed to create or acquire single instance mutex.");
                _ = LogErrors.LogErrorAsync(ex, "Failed to create or acquire single instance mutex.");

                // Notify user
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
        // The new instance (started with --restarting) didn't acquire the mutex, so _isFirstInstance will be false,
        // and it won't try to release it.
        if (_singleInstanceMutex != null && _isFirstInstance)
        {
            try
            {
                _singleInstanceMutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                // Log the error using the new DebugLogger (if initialized) and old LogErrors (always)
                DebugLogger.LogException(ex, "Failed to release single instance mutex on exit.");
                _ = LogErrors.LogErrorAsync(ex, "Failed to release single instance mutex on exit.");
            }
            finally
            {
                _singleInstanceMutex.Dispose();
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

                DebugLogger.Log($"Applied language: {culture.Name}"); // Log successful language change
            }
            catch (Exception innerEx)
            {
                // Notify developer
                var contextMessage = $"Failed to load language resources for {culture.Name} (requested {cultureCode}).";
                DebugLogger.LogException(innerEx, contextMessage); // Log using debug logger
                _ = LogErrors.LogErrorAsync(innerEx, contextMessage); // Log using error logger

                // Notify user
                MessageBoxLibrary.FailedToLoadLanguageResourceMessageBox();

                // Fallback to English
                var fallbackDictionary = new ResourceDictionary
                {
                    Source = new Uri("/resources/strings.en.xaml", UriKind.Relative)
                };
                // Ensure the fallback is added even if the requested one failed
                Resources.MergedDictionaries.Add(fallbackDictionary);

                // Notify developer
                DebugLogger.Log("Fallback to English language resources."); // Log fallback
                _ = LogErrors.LogErrorAsync(new Exception("Fallback to English language resources."), "Using fallback language.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            // This outer catch handles errors *before* attempting to load the new dictionary
            // (e.g., invalid cultureCode format).
            var contextMessage = $"Failed to determine or set culture for {cultureCode}";
            DebugLogger.LogException(ex, contextMessage); // Log using debug logger
            _ = LogErrors.LogErrorAsync(ex, contextMessage); // Log using error logger

            // Notify user
            MessageBoxLibrary.FailedToLoadLanguageResourceMessageBox();

            // Use English fallback if none exists
            if (!Resources.MergedDictionaries.Any(static d => d.Source?.OriginalString.Contains("strings.") ?? false))
            {
                var fallbackDictionary = new ResourceDictionary
                {
                    Source = new Uri("/resources/strings.en.xaml", UriKind.Relative)
                };
                Resources.MergedDictionaries.Add(fallbackDictionary);

                // Notify developer
                DebugLogger.Log("Fallback to English language resources due to initial culture error."); // Log fallback
                _ = LogErrors.LogErrorAsync(new Exception("Fallback to English language resources due to initial culture error."), "Using fallback language.");
            }
        }
    }

    private static void ApplyTheme(string baseTheme, string accentColor)
    {
        try
        {
            ThemeManager.Current.ChangeTheme(Current, $"{baseTheme}.{accentColor}");
            DebugLogger.Log($"Applied theme: {baseTheme}.{accentColor}"); // Log theme change
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to Apply Theme.";
            DebugLogger.LogException(ex, contextMessage); // Log using debug logger
            _ = LogErrors.LogErrorAsync(ex, contextMessage); // Log using error logger
        }
    }

    public static void ApplyThemeToWindow(Window window)
    {
        var baseTheme = Settings.BaseTheme;
        var accentColor = Settings.AccentColor;
        try
        {
            ThemeManager.Current.ChangeTheme(window, $"{baseTheme}.{accentColor}");
            DebugLogger.Log($"Applied theme to window {window.GetType().Name}: {baseTheme}.{accentColor}"); // Log theme change for window
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, $"Failed to apply theme to window {window.GetType().Name}."); // Log using debug logger
            _ = LogErrors.LogErrorAsync(ex, $"Failed to apply theme to window {window.GetType().Name}."); // Log using error logger
        }
    }

    public static void ChangeTheme(string baseTheme, string accentColor)
    {
        ApplyTheme(baseTheme, accentColor);
        Settings.BaseTheme = baseTheme;
        Settings.AccentColor = accentColor;
        Settings.Save();
        DebugLogger.Log($"Saved theme settings: {baseTheme}.{accentColor}"); // Log saving settings
    }
}