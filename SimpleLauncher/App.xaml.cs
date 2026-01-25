using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using ControlzEx.Theming;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SevenZip;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;
using SimpleLauncher.Services.GameScanLogic;
using SimpleLauncher.Services.RetroAchievements;

namespace SimpleLauncher;

public partial class App : IDisposable
{
    public static IServiceProvider ServiceProvider { get; private set; }
    public static IConfiguration Configuration { get; set; }

    private Mutex _singleInstanceMutex;
    private bool _isFirstInstance;
    private const string UniqueMutexIdentifier = "A8E2B9C1-F5D7-4E0A-8B3C-6D1E9F0A7B4C";
    private const string MutexName = "SimpleLauncher_SingleInstanceMutex_" + UniqueMutexIdentifier;
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    protected override void OnStartup(StartupEventArgs e)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true);

        Configuration = builder.Build();

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
        serviceCollection.AddHttpClient("GameImageClient", static client =>
        {
            var apiUrl = Configuration.GetValue<string>("ApiSettings:GameImageUrl");
            client.BaseAddress = new Uri(apiUrl ?? "https://simple-launcher-api.doutorpeterson.workers.dev/");
        });
        serviceCollection.AddHttpClient("EasyModeClient", static client =>
        {
            // Set the base address for the EasyMode configuration API
            client.BaseAddress = new Uri("https://www.purelogiccode.com/simplelauncheradmin/");
        });

        // Register IConfiguration
        serviceCollection.AddSingleton<IConfiguration>(Configuration);

        // Register IMemoryCache
        serviceCollection.AddMemoryCache();

        // Register Managers as singletons
        serviceCollection.AddSingleton<ILogErrors, LogErrorsService>();
        serviceCollection.AddSingleton(static _ =>
        {
            var sm = new SettingsManager();
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

        ServiceProvider = serviceCollection.BuildServiceProvider();

        // --- Single Instance Check ---
        // Catch args
        var isRestarting = e.Args.Any(static arg => arg.Equals("--restarting", StringComparison.OrdinalIgnoreCase));
        var isDebugMode = e.Args.Any(static arg => arg.Equals("-debug", StringComparison.OrdinalIgnoreCase));
        var displayHistoryWindow = e.Args.Any(static arg => arg.Equals("-whatsnew", StringComparison.OrdinalIgnoreCase));

        // Initialize DebugLogger early
        DebugLogger.Initialize(isDebugMode);

        // Initialize SevenZipSharp library path
        InitializeSevenZipSharp();

        // Delete temp folders and unneeded files
        _ = Task.Run(CleanSimpleLauncherFolder.CleanupTrash);

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
        MountZipFiles.Configure(Configuration);

        // Manually create and show the MainWindow using DI
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        Current.MainWindow = mainWindow;
        mainWindow.Show();

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

    private static void InitializeSevenZipSharp()
    {
        try
        {
            string dllName;
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.Arm64:
                    dllName = "7z_arm64.dll";
                    break;
                case Architecture.X64:
                    dllName = "7z_x64.dll";
                    break;
                default:
                    // Notify developer
                    var errorMessage = $"Unsupported architecture for 'Simple Launcher': {RuntimeInformation.ProcessArchitecture}";
                    _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, errorMessage);

                    // Notify user
                    MessageBoxLibrary.UnsupportedArchitectureMessageBox();

                    Current.Shutdown();
                    return;
            }

            var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dllName);

            if (File.Exists(dllPath))
            {
                SevenZipBase.SetLibraryPath(dllPath);
                DebugLogger.Log($"SevenZipSharp library path set to: {dllPath}");
            }
            else
            {
                // Notify developer
                var errorMessage = $"Could not find the required 7-Zip library: {dllName} in {BaseDirectory}";
                _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, errorMessage);

                // Notify user
                MessageBoxLibrary.SevenZipDllNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to initialize SevenZipSharp library.");

            // Notify user
            MessageBoxLibrary.FailedToInitializeSevenZipMessageBox();
        }
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

            var dictionary = new ResourceDictionary();

            try
            {
                dictionary.Source = new Uri($"pack://application:,,,/resources/strings.{culture.Name}.xaml", UriKind.Absolute);
            }
            catch (Exception)
            {
                dictionary.Source = new Uri("pack://application:,,,/resources/strings.en.xaml", UriKind.Absolute);
                culture = new CultureInfo("en-US");
            }

            try
            {
                // Attempt to add the new dictionary
                Resources.MergedDictionaries.Add(dictionary);

                // Apply the culture to the application
                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

                DebugLogger.Log("Resource language file has been applied.");
            }
            catch (Exception innerEx)
            {
                // Notify developer
                var contextMessage = $"Failed to load language resources for {culture.Name} (requested {cultureCode}).";
                _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(innerEx, contextMessage);

                // Notify user
                MessageBoxLibrary.FailedToLoadLanguageResourceMessageBox();

                // Fallback to English
                var fallbackDictionary = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/resources/strings.en.xaml", UriKind.Absolute)
                };

                // Ensure the fallback is added even if the requested one failed
                Resources.MergedDictionaries.Add(fallbackDictionary);

                // Apply English culture metadata for consistent formatting
                var englishCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = englishCulture;
                Thread.CurrentThread.CurrentUICulture = englishCulture;

                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(englishCulture.IetfLanguageTag)));

                // Notify developer
                _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "Fallback to English language resources.");
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            // This outer catch handles errors *before* attempting to load the new dictionary
            // (e.g., invalid cultureCode format).
            var contextMessage = $"Failed to determine or set culture for {cultureCode}";
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FailedToLoadLanguageResourceMessageBox();

            // Use English fallback if none exists
            if (!Resources.MergedDictionaries.Any(static d => d.Source?.OriginalString.Contains("strings.") ?? false))
            {
                var fallbackDictionary = new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/resources/strings.en.xaml", UriKind.Absolute)
                };
                Resources.MergedDictionaries.Add(fallbackDictionary);

                // Apply English culture metadata for consistent formatting
                var englishCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = englishCulture;
                Thread.CurrentThread.CurrentUICulture = englishCulture;

                FrameworkElement.LanguageProperty.OverrideMetadata(
                    typeof(FrameworkElement),
                    new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(englishCulture.IetfLanguageTag)));

                // Notify developer
                _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "Fallback to English language resources due to initial culture error.");
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
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
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
            ThemeManager.Current.ChangeTheme(window, $"{baseTheme}.{accentColor}");
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to apply theme to window {window.GetType().Name}.");
        }
    }

    public static void ChangeTheme(string baseTheme, string accentColor)
    {
        ApplyTheme(baseTheme, accentColor);
        // Get the singleton SettingsManager instance
        var settings = ServiceProvider.GetRequiredService<SettingsManager>();
        settings.BaseTheme = baseTheme;
        settings.AccentColor = accentColor;
        settings.Save();

        DebugLogger.Log("Theme has been applied.");
        DebugLogger.Log($"Saved theme settings: {baseTheme}.{accentColor}");
    }

    public void Dispose()
    {
        _singleInstanceMutex?.Dispose();
        GC.SuppressFinalize(this);
    }
}