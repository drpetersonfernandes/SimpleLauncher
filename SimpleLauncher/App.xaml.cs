using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ControlzEx.Theming;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.CleanAndDeleteFiles;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.DownloadService;
using SimpleLauncher.Services.ExtractFiles;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GameLauncher.Handlers;
using SimpleLauncher.Services.GameLauncher.Strategies;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.GameScan;
using SimpleLauncher.Services.LaunchTools;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.MountFiles;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.UsageStats;
using UpdateChecker = SimpleLauncher.Services.CheckForUpdates.UpdateChecker;

namespace SimpleLauncher;

public partial class App : IDisposable
{
    public static IServiceProvider ServiceProvider { get; private set; }

    private Mutex _singleInstanceMutex;
    private bool _isFirstInstance;
    private const string UniqueMutexIdentifier = "A8E2B9C1-F5D7-4E0A-8B3C-6D1E9F0A7B4C";
    private const string MutexName = "SimpleLauncher_SingleInstanceMutex_" + UniqueMutexIdentifier;

    protected override void OnStartup(StartupEventArgs e)
    {
        // Detect if the application is running from a temporary extraction folder
        // (e.g., user double-clicked the .exe inside a ZIP/RAR archive)
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var tempDir = Path.GetTempPath();
        if (baseDir.StartsWith(tempDir, StringComparison.OrdinalIgnoreCase))
        {
            MessageBoxLibrary.PleaseExtractApplicationFirst();
            Shutdown();
            return;
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true);

        var configuration = builder.Build();

        var serviceCollection = new ServiceCollection();
        // Register IHttpClientFactory and named clients
        serviceCollection.AddHttpClient("LogErrorsClient");
        serviceCollection.AddHttpClient("StatsClient");
        serviceCollection.AddHttpClient("UpdateCheckerClient");
        serviceCollection.AddHttpClient("SupportWindowClient");
        serviceCollection.AddHttpClient("RetroAchievementsClient", static client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "SimpleLauncher/1.0");
        });
        serviceCollection.AddHttpClient("GameImageClient", client =>
        {
            var apiUrl = configuration.GetValue<string>("ApiSettings:GameImageUrl") ?? "https://simple-launcher-api.doutorpeterson.workers.dev/";
            client.BaseAddress = new Uri(apiUrl);
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Add("User-Agent", "SimpleLauncher/1.0");
        });
        serviceCollection.AddHttpClient("EasyModeClient", client =>
        {
            // Set the base address for the EasyMode configuration API
            var easyModeUrl = configuration.GetValue<string>("Urls:EasyModeApi") ?? "https://www.purelogiccode.com/simplelauncheradmin/";
            client.BaseAddress = new Uri(easyModeUrl);
            if (!easyModeUrl.EndsWith('/'))
            {
                // ReSharper disable once RedundantAssignment
                easyModeUrl += "/";
            }
        });
        serviceCollection.AddHttpClient("GameClassificationClient", client =>
        {
            var classificationUrl = configuration.GetValue<string>("Urls:GameClassificationApi") ?? "https://www.purelogiccode.com/simplelauncheradmin/";
            client.BaseAddress = new Uri(classificationUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "SimpleLauncher/1.0");
        });

        // Register IConfiguration
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        // Register IMemoryCache
        serviceCollection.AddMemoryCache();

        // Register Managers as singletons
        // Register Managers as singletons
        serviceCollection.AddSingleton<ILogErrors, LogErrorsService>();
        serviceCollection.AddSingleton(static provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var sm = new SettingsManager(config);
            sm.Load();
            return sm;
        });
        serviceCollection.AddSingleton<UpdateChecker>();
        serviceCollection.AddSingleton<Stats>();
        serviceCollection.AddSingleton<PlaySoundEffects>();
        serviceCollection.AddSingleton<GamePadController>();
        serviceCollection.AddTransient<DownloadManager>();
        serviceCollection.AddSingleton<GameLauncher>();
        serviceCollection.AddSingleton<ILaunchTools, LaunchTools>();
        serviceCollection.AddSingleton<IExtractionService, ExtractionService>();
        serviceCollection.AddSingleton<RetroAchievementsService>();
        serviceCollection.AddSingleton(static _ => FavoritesManager.LoadFavorites());
        serviceCollection.AddSingleton(static _ => PlayHistoryManager.LoadPlayHistory());
        serviceCollection.AddSingleton(static _ => RetroAchievementsManager.LoadRetroAchievement());
        serviceCollection.AddSingleton<GameScannerService>();
        serviceCollection.AddTransient<MainWindow>();

        // Handlers
        serviceCollection.AddSingleton<IEmulatorConfigHandler, AresConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, AzaharConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, BlastemConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, CemuConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, DaphneConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, DolphinConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, DuckStationConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, FlycastConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, MameConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, MednafenConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, MesenConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, Pcsx2ConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, RaineConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, RedreamConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, RetroArchConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, Rpcs3ConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, SegaModel2ConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, StellaConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, SupermodelConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, XeniaConfigHandler>();
        serviceCollection.AddSingleton<IEmulatorConfigHandler, YumirConfigHandler>();

        // Strategies
        serviceCollection.AddSingleton<ILaunchStrategy, ChdToCueStrategy>();
        serviceCollection.AddSingleton<ILaunchStrategy, ChdMountStrategy>();
        serviceCollection.AddSingleton<ILaunchStrategy, PbpToCueStrategy>();
        serviceCollection.AddSingleton<ILaunchStrategy, XisoMountStrategy>();
        serviceCollection.AddSingleton<ILaunchStrategy, ZipMountStrategy>();
        serviceCollection.AddSingleton<ILaunchStrategy, DefaultLaunchStrategy>();

        ServiceProvider = serviceCollection.BuildServiceProvider();

        // --- Single Instance Check ---
        // Catch args
        var isRestarting = e.Args.Any(static arg => arg.Equals("--restarting", StringComparison.OrdinalIgnoreCase));
        var isDebugMode = e.Args.Any(static arg => arg.Equals("-debug", StringComparison.OrdinalIgnoreCase));
        var displayHistoryWindow = e.Args.Any(static arg => arg.Equals("-whatsnew", StringComparison.OrdinalIgnoreCase));

        // Initialize DebugLogger early
        DebugLogger.Initialize(isDebugMode);

        // Delete temp folders and unneeded files
        _ = Task.Run(CleanSimpleLauncherFolder.CleanupTrash);
        // _ = Task.Run(CleanSimpleLauncherFolder.CleanupTempFiles);

        if (!isRestarting) // Only perform the mutex check if NOT restarting
        {
            try
            {
                // Try to create or open the mutex
                // The 'out _isFirstInstance' parameter will be true if the mutex was created (first instance)
                // and false if it already existed (another instance is running).
                _singleInstanceMutex = new Mutex(true, MutexName, out _isFirstInstance);
            }
            catch (AbandonedMutexException)
            {
                // The mutex was abandoned by a previous instance (e.g., due to a crash).
                // This means we successfully acquired it, and we are now the first instance.
                // The 'out _isFirstInstance' parameter would already be true in this case,
                // but we explicitly set it for clarity and to ensure the flow continues as a first instance.
                _isFirstInstance = true;
                DebugLogger.Log("Mutex was abandoned by a previous instance, but successfully acquired by this instance. Proceeding as first instance.");
                // No need to call ILogErrors.LogErrorAsync here, as it's not a critical error preventing startup,
                // but rather an informational event about a previous abnormal shutdown.
            }
            catch (Exception ex)
            {
                // Handle other general exceptions during mutex creation/acquisition (e.g., access denied, out of memory).
                // Notify developer about the failure.
                _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to create or acquire single instance mutex.");

                // Notify user
                MessageBoxLibrary.FailedToStartSimpleLauncherMessageBox();

                Shutdown();

                return;
            }

            // After attempting to acquire the mutex (and handling AbandonedMutexException),
            // check if this is truly the first instance.
            if (!_isFirstInstance)
            {
                // Another instance is running. Inform the user and exit this instance.
                MessageBoxLibrary.AnotherInstanceIsRunningMessageBox();

                Shutdown();

                return; // Stop further startup logic
            }
        }
        // --- End Single Instance Check ---

        base.OnStartup(e);

        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

        // Get the singleton SettingsManager instance
        var settingsManager = ServiceProvider.GetRequiredService<SettingsManager>();
        ApplyTheme(settingsManager.BaseTheme, settingsManager.AccentColor);
        ApplyLanguage(settingsManager.Language);

        // --- Initialize services that need configuration ---
        MountZipFiles.Configure(configuration);

        // Manually create and show the MainWindow using DI
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        Current.MainWindow = mainWindow;
        mainWindow.Show();

        // Call ApplicationStats API on startup
        _ = ApplicationStats.CallApplicationStatsAsync(configuration);

        // Show UpdateHistoryWindow if -whatsnew argument is present
        // This is done after ensuring we're the single instance and after initialization
        if (displayHistoryWindow)
        {
            // Use Dispatcher.BeginInvoke to show the window after the main window is loaded
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    var updateHistoryWindow = new UpdateHistoryWindow();
                    updateHistoryWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error showing UpdateHistoryWindow with -whatsnew argument.";
                    _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
                }
            }));
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            var gamePadController = ServiceProvider.GetRequiredService<GamePadController>();
            // Dispose gamepad resources
            gamePadController?.Stop();
            gamePadController?.Dispose();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to dispose gamepad resources.");
        }

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
                // Notify developer
                _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to release single instance mutex on exit.");
            }
            finally
            {
                _singleInstanceMutex.Dispose();
            }
        }

        Dispose();
        base.OnExit(e);
    }

    private static void ApplyLanguage(string languageCode)
    {
        try
        {
            var culture = new CultureInfo(languageCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Load and apply the resource dictionary for the selected language
            var resourceDictionary = new ResourceDictionary();
            var resourcePath = $"/SimpleLauncher;component/resources/strings.{languageCode}.xaml";
            resourceDictionary.Source = new Uri(resourcePath, UriKind.Relative);

            // Add the new dictionary to the application's resources
            // Find and remove any existing language dictionaries first
            var existingLanguageDictionaries = Current.Resources.MergedDictionaries
                .Where(static d => d.Source != null && d.Source.OriginalString.Contains("/resources/strings."))
                .ToList();

            foreach (var dict in existingLanguageDictionaries)
            {
                Current.Resources.MergedDictionaries.Remove(dict);
            }

            Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }
        catch (Exception ex)
        {
            // Log the error using the LogErrorsService
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to Apply Language.");

            // Fallback to English if loading the specified language fails
            if (languageCode != "en")
            {
                try
                {
                    var fallbackDictionary = new ResourceDictionary
                    {
                        Source = new Uri("/SimpleLauncher;component/resources/strings.en.xaml", UriKind.Relative)
                    };
                    Current.Resources.MergedDictionaries.Add(fallbackDictionary);
                }
                catch (Exception fallbackEx)
                {
                    // If even English fails, something is seriously wrong
                    _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(fallbackEx, "Failed to apply English as fallback language.");
                }

                // Notify developer
                _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "Fallback to English language resources due to initial culture error.");
            }
        }
    }

    private static void ApplyTheme(string baseTheme, string accentColor)
    {
        try
        {
            // Handle Theme Sync Mode (Adaptive)
            ThemeManager.Current.ThemeSyncMode = baseTheme == "Adaptive" ? ThemeSyncMode.SyncAll : ThemeSyncMode.DoNotSync;
            switch (baseTheme)
            {
                case "Adaptive":
                    ThemeManager.Current.SyncTheme();
                    return;
                // Handle High Contrast
                case "HighContrast":
                {
                    InternalChangeTheme(Current, "Dark", accentColor);
                    ApplyCustomThemeOverride("Theme.HighContrast.xaml");
                    return;
                }
                // Handle Custom Theme (Midnight)
                case "Midnight":
                {
                    InternalChangeTheme(Current, "Dark", accentColor);
                    ApplyCustomThemeOverride("Theme.Midnight.xaml");
                    return;
                }
                default:
                    // Standard Themes (Light/Dark)
                    RemoveCustomThemeOverrides();
                    InternalChangeTheme(Current, baseTheme, accentColor);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to Apply Theme.";
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
        }
    }

    private static void InternalChangeTheme(object target, string baseTheme, string accentColor)
    {
        if (IsCustomAccent(accentColor))
        {
            var color = GetColorForAccent(accentColor);
            var theme = new Theme(
                $"{baseTheme}.{accentColor}",
                $"{baseTheme} ({accentColor})",
                baseTheme,
                accentColor,
                color,
                new SolidColorBrush(color),
                true,
                false
            );

            switch (target)
            {
                case Application app:
                    ThemeManager.Current.ChangeTheme(app, theme);
                    break;
                case Window win:
                    ThemeManager.Current.ChangeTheme(win, theme);
                    break;
            }
        }
        else
        {
            switch (target)
            {
                case Application app:
                    ThemeManager.Current.ChangeTheme(app, $"{baseTheme}.{accentColor}");
                    break;
                case Window win:
                    ThemeManager.Current.ChangeTheme(win, $"{baseTheme}.{accentColor}");
                    break;
            }
        }
    }

    private static void ApplyCustomThemeOverride(string fileName)
    {
        try
        {
            RemoveCustomThemeOverrides();
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri($"/SimpleLauncher;component/resources2/{fileName}", UriKind.Relative)
            };
            Current.Resources.MergedDictionaries.Add(resourceDictionary);
        }
        catch (Exception ex)
        {
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to apply custom theme override: {fileName}");
        }
    }

    private static void RemoveCustomThemeOverrides()
    {
        var customThemes = Current.Resources.MergedDictionaries
            .Where(static d => d.Source != null && (d.Source.OriginalString.Contains("Theme.HighContrast.xaml") || d.Source.OriginalString.Contains("Theme.Midnight.xaml")))
            .ToList();

        foreach (var dict in customThemes)
        {
            Current.Resources.MergedDictionaries.Remove(dict);
        }
    }

    private static void ApplyCustomThemeOverrideToWindow(Window window, string fileName)
    {
        try
        {
            RemoveCustomThemeOverridesFromWindow(window);
            var resourceDictionary = new ResourceDictionary
            {
                Source = new Uri($"/SimpleLauncher;component/resources2/{fileName}", UriKind.Relative)
            };
            window.Resources.MergedDictionaries.Add(resourceDictionary);
        }
        catch (Exception ex)
        {
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to apply custom theme override to window {window.GetType().Name}: {fileName}");
        }
    }

    private static void RemoveCustomThemeOverridesFromWindow(Window window)
    {
        var customThemes = window.Resources.MergedDictionaries
            .Where(static d => d.Source != null && (d.Source.OriginalString.Contains("Theme.HighContrast.xaml") || d.Source.OriginalString.Contains("Theme.Midnight.xaml")))
            .ToList();

        foreach (var dict in customThemes)
        {
            window.Resources.MergedDictionaries.Remove(dict);
        }
    }

    public static void ApplyThemeToWindow(Window window)
    {
        // Get the singleton SettingsManager instance
        var settings = ServiceProvider.GetRequiredService<SettingsManager>();
        var baseTheme = settings.BaseTheme;
        var accentColor = settings.AccentColor;
        try
        {
            switch (baseTheme)
            {
                case "Adaptive":
                    var detectedTheme = ThemeManager.Current.DetectTheme();
                    if (detectedTheme != null)
                    {
                        ThemeManager.Current.ChangeTheme(window, detectedTheme);
                    }

                    return;
                case "HighContrast":
                {
                    InternalChangeTheme(window, "Dark", accentColor);
                    ApplyCustomThemeOverrideToWindow(window, "Theme.HighContrast.xaml");
                    return;
                }
                case "Midnight":
                {
                    InternalChangeTheme(window, "Dark", accentColor);
                    ApplyCustomThemeOverrideToWindow(window, "Theme.Midnight.xaml");
                    return;
                }
                default:
                    RemoveCustomThemeOverridesFromWindow(window);
                    InternalChangeTheme(window, baseTheme, accentColor);
                    break;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to apply theme to window {window.GetType().Name}.");
        }
    }

    private static bool IsCustomAccent(string accentColor)
    {
        return accentColor switch
        {
            "Maroon" or "OliveDrab" or "Plum" or "SkyBlue" => true,
            _ => false
        };
    }

    private static Color GetColorForAccent(string accentColor)
    {
        return accentColor switch
        {
            "Maroon" => Colors.Maroon,
            "OliveDrab" => Colors.OliveDrab,
            "Plum" => Colors.Plum,
            "SkyBlue" => Colors.SkyBlue,
            _ => Colors.Blue // Default fallback
        };
    }

    public static void ChangeTheme(string baseTheme, string accentColor)
    {
        // Get the singleton SettingsManager instance
        var settings = ServiceProvider.GetRequiredService<SettingsManager>();
        settings.BaseTheme = baseTheme;
        settings.AccentColor = accentColor;
        settings.Save();

        ApplyTheme(baseTheme, accentColor);

        // Apply theme to all currently open windows
        foreach (Window window in Current.Windows)
        {
            ApplyThemeToWindow(window);
        }

        DebugLogger.Log("Theme has been applied to all windows.");
        DebugLogger.Log($"Saved theme settings: {baseTheme}.{accentColor}");
    }

    public void Dispose()
    {
        _singleInstanceMutex?.Dispose();
        GC.SuppressFinalize(this);
    }
}