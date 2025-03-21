using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using ControlzEx.Theming;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace SimpleLauncher;

public partial class App
{
    private static SettingsManager _settings;
    private static Mutex _singleInstanceMutex;
    private const string MutexName = "SimpleLauncherSingleInstanceMutex";

    protected override void OnStartup(StartupEventArgs e)
    {
        // Check if another instance is already running
        _singleInstanceMutex = new Mutex(true, MutexName, out var createdNew);

        if (!createdNew)
        {
            // Another instance is already running, show warning and activate existing instance
            MessageBox.Show(
                "Simple Launcher is already running. Only one instance can be launched at a time.",
                "Simple Launcher",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Activate the existing instance
            ActivateExistingInstance();
            Shutdown();
            return;
        }

        base.OnStartup(e);
        _settings = new SettingsManager();
        ApplyTheme(_settings.BaseTheme, _settings.AccentColor);
        ApplyLanguage(_settings.Language);
    }

    private void ActivateExistingInstance()
    {
        // Find the window of the existing instance
        var processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
        foreach (var process in processes)
        {
            // Skip the current process
            if (process.Id == Process.GetCurrentProcess().Id) continue;

            // Bring the window to front
            var mainWindowHandle = process.MainWindowHandle;
            if (mainWindowHandle != IntPtr.Zero)
            {
                // If the window is minimized, restore it
                if (IsIconic(mainWindowHandle))
                {
                    ShowWindow(mainWindowHandle, SW_RESTORE);
                }

                // Bring to front and activate
                SetForegroundWindow(mainWindowHandle);
                break;
            }
        }
    }

    // P/Invoke declarations for Windows API
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(IntPtr hWnd);

    private const int SW_RESTORE = 9;

    protected override void OnExit(ExitEventArgs e)
    {
        // Release the mutex when the application exits
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();

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

            // Load the resource dictionary
            var dictionary = new ResourceDictionary
            {
                Source = new Uri($"/resources/strings.{culture.Name}.xaml", UriKind.Relative)
            };

            // Replace the current localization dictionary
            var existingDictionary = Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("strings.") ?? false);

            if (existingDictionary != null)
            {
                Resources.MergedDictionaries.Remove(existingDictionary);
            }

            Resources.MergedDictionaries.Add(dictionary);

            // Apply the culture to the application
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Failed to load language resources for {cultureCode}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FailedToLoadLanguageResourceMessageBox();

            // Fallback to English
            var fallbackDictionary = new ResourceDictionary
            {
                Source = new Uri("/resources/strings.en.xaml", UriKind.Relative)
            };
            Resources.MergedDictionaries.Add(fallbackDictionary);
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
        var baseTheme = _settings.BaseTheme;
        var accentColor = _settings.AccentColor;
        ThemeManager.Current.ChangeTheme(window, $"{baseTheme}.{accentColor}");
    }

    public static void ChangeTheme(string baseTheme, string accentColor)
    {
        ApplyTheme(baseTheme, accentColor);
        _settings.BaseTheme = baseTheme;
        _settings.AccentColor = accentColor;
        _settings.Save();
    }
}