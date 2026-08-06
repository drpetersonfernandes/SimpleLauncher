using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.Services.AvaloniaServices;
using SimpleLauncher.Avalonia.Services.Favorites;
using SimpleLauncher.Avalonia.Services.GameLauncher;
using SimpleLauncher.Avalonia.Services.GameScan;
using SimpleLauncher.Avalonia.Services.InjectEmulatorConfig;
using SimpleLauncher.Avalonia.Services.PlayHistory;
using SimpleLauncher.Avalonia.Services.RetroAchievements;
using SimpleLauncher.Avalonia.Services.SystemManager;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services;
using SimpleLauncher.Core.Services.CheckForFileLock;
using SimpleLauncher.Core.Services.CleanAndDeleteFiles;
using SimpleLauncher.Core.Services.Converters;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.DownloadService;
using SimpleLauncher.Core.Services.EasyMode;
using SimpleLauncher.Core.Services.ExternalToolLauncher;
using SimpleLauncher.Core.Services.ExtractFiles;
using SimpleLauncher.Core.Services.FindCoverImage;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;
using SimpleLauncher.Core.Services.GetListOfFiles;
using SimpleLauncher.Core.Services.MameData;
using SimpleLauncher.Core.Services.ParameterResolver;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.Services.SanitizeInputString;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Core.Services.SystemConfiguration;
using SimpleLauncher.Core.Services.UsageStats;
using SimpleLauncher.Core.Services.WpfServices;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Application entry point handling DI container setup, single-instance enforcement, and global error handling.
/// </summary>
public class App : Application, IDisposable
{
    /// <summary>
    /// Gets the application's dependency injection service provider.
    /// </summary>
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    private Mutex? _singleInstanceMutex;
    private bool _isFirstInstance;
    private const string UniqueMutexIdentifier = "D7F1A8B2-C4E6-9D0F-7A3B-5C1E2F8A6D9B";
    private const string MutexName = "SimpleLauncherNew_SingleInstanceMutex_" + UniqueMutexIdentifier;
    private const string EventName = "SimpleLauncherNew_SingleInstanceEvent_" + UniqueMutexIdentifier;
    private EventWaitHandle? _instanceSignal;

    /// <summary>
    /// Handles application startup including DI registration, single-instance check, and theme initialization.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Dispatcher.UIThread.UnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        // Single-instance enforcement
        _singleInstanceMutex = new Mutex(true, MutexName, out _isFirstInstance);
        _instanceSignal = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);

        if (!_isFirstInstance)
        {
            // Signal the first instance to come to foreground
            try
            {
                _instanceSignal.Set();
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Failed to signal first instance");
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime nonFirstInstanceLifetime)
            {
                // Do NOT call Shutdown() synchronously here: the dispatcher main loop has not
                // started yet, and DoShutdown() -> Dispatcher.UIThread.InvokeShutdown() leaves
                // the dispatcher permanently shut down, so StartCore() then throws
                // "Cannot perform requested operation because the Dispatcher shut down" when it
                // calls Dispatcher.UIThread.MainLoop(...). Post the shutdown instead so it runs
                // once the main loop is pumping (same clean-exit path as closing the main window).
                Dispatcher.UIThread.Post(() => nonFirstInstanceLifetime.Shutdown());
            }

            return;
        }

        // Configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true);

        var configuration = builder.Build();

        // Serilog setup
        var appDataLogFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SimpleLauncher");
        Directory.CreateDirectory(appDataLogFolder);

        // Sink that forwards Warning+ events to the bug report API
        var bugReportSink = new BugReportApiSink();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Debug(outputTemplate: "[{Level}] {Timestamp:HH:mm:ss.fff} - {Message}{NewLine}{Exception}")
            .WriteTo.Async(a => a.File(
                Path.Combine(appDataLogFolder,
                    configuration.GetValue<string>("LogPath") ?? "error_user.log"),
                LogEventLevel.Warning,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"))
            .WriteTo.Sink(bugReportSink)
            .CreateLogger();

        Log.Information("SimpleLauncher.Avalonia starting up");

        // DI container
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, configuration, bugReportSink);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Initialize the bug report sink with DI services (queues Warning+ events to the API)
        bugReportSink.Initialize(
            ServiceProvider.GetRequiredService<IHttpClientFactory>(),
            ServiceProvider.GetRequiredService<IConfiguration>(),
            ServiceProvider.GetRequiredService<IDeleteFilesService>(),
            appDataLogFolder);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.ShutdownMode = ShutdownMode.OnMainWindowClose;
            lifetime.Exit += (_, _) => Dispose();

            // Start listening for second-instance signals
            _ = ListenForSecondInstanceAsync(lifetime);

            // Show main window
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            lifetime.MainWindow = mainWindow;
            mainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Registers all services, ViewModels, and windows in the DI container.
    /// </summary>
    internal static void ConfigureServices(IServiceCollection services, IConfiguration configuration, BugReportApiSink? bugReportSink = null)
    {
        // Register configuration
        services.AddSingleton(configuration);

        // Register the Serilog logger so ILogger ctor parameters resolve to Log.Logger
        services.AddSingleton(Log.Logger);

        // ── Named HttpClient factories ──
        services.AddHttpClient("LogErrorsClient");
        services.AddHttpClient("StatsClient");
        services.AddHttpClient("UpdateCheckerClient");
        services.AddHttpClient("RetroAchievementsClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "SimpleLauncher.Avalonia/1.0");
        });
        services.AddHttpClient("GameImageClient");
        services.AddHttpClient("EasyModeClient", client =>
        {
            // Set the base address for the EasyMode configuration API
            var easyModeUrl = configuration.GetValue<string>("Urls:EasyModeApi")
                              ?? "https://www.purelogiccode.com/simplelauncheradmin/";
            if (!easyModeUrl.EndsWith('/'))
            {
                easyModeUrl += '/';
            }

            client.BaseAddress = new Uri(easyModeUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddHttpClient("GameClassificationClient");
        services.AddHttpClient("ParameterResolverClient");
        services.AddHttpClient("DownloadClient");

        // ── Host services (implement Core interfaces) ──
        services.AddSingleton<IDispatcherService, AvaloniaDispatcherService>();
        services.AddSingleton<IFilePickerService, AvaloniaFilePickerService>();
        services.AddSingleton<Core.Interfaces.IResourceProvider, AvaloniaResourceProvider>();
        services.AddSingleton<IWindowContext, AvaloniaWindowContext>();
        services.AddSingleton<Core.Interfaces.IApplicationLifetime, AvaloniaApplicationLifetime>();
        services.AddSingleton<IMessageBoxLibraryService, MessageBoxLibraryService>();

        // ── Core services (from SimpleLauncher.Core) ──
        services.AddSingleton<DataFileLocation>();
        services.AddSingleton<InputSanitizerService>();
        services.AddSingleton<WindowsVersionService>();
        // Register the SAME instance wired to Serilog (see OnFrameworkInitializationCompleted)
        // so DI consumers share one sink instead of an uninitialized dead instance.
        if (bugReportSink is not null)
        {
            services.AddSingleton(bugReportSink);
        }

        services.AddSingleton<SettingsManagerService>();
        services.AddSingleton<MameDataService>();
        services.AddSingleton<RetroAchievementsManager>();
        services.AddSingleton<IRetroAchievementsFileHasher, RetroAchievementsFileHasher>();
        services.AddSingleton<IRetroAchievementsEmulatorConfiguratorService, RetroAchievementsEmulatorConfiguratorService>();
        services.AddSingleton<IRetroAchievementsSystemMatcher, RetroAchievementsSystemMatcher>();
        services.AddSingleton<ParameterResolverService>();
        services.AddSingleton<IParameterResolverService>(sp => sp.GetRequiredService<ParameterResolverService>());
        services.AddSingleton<GetListOfFilesService>();
        services.AddSingleton<IGetListOfFilesService>(sp => sp.GetRequiredService<GetListOfFilesService>());
        services.AddSingleton<FindCoverImageService>();
        services.AddSingleton<IFindCoverImageService>(sp => sp.GetRequiredService<FindCoverImageService>());
        services.AddSingleton<ExtractionService>();
        services.AddSingleton<IExtractionService>(sp => sp.GetRequiredService<ExtractionService>());
        services.AddSingleton<DownloadManager>();
        services.AddSingleton<DiscConverter>();
        services.AddSingleton<EasyModeManager>();
        services.AddSingleton<ExternalToolLauncherService>();
        services.AddSingleton<PlaySoundEffects>();
        services.AddSingleton<IPlaySoundEffects>(sp => sp.GetRequiredService<PlaySoundEffects>());
        services.AddSingleton<SystemConfigurationWriterService>();
        services.AddSingleton<ISystemConfigurationWriterService>(sp => sp.GetRequiredService<SystemConfigurationWriterService>());
        services.AddSingleton<Stats>();

        // Mount services
        services.AddSingleton<IMountZipFiles, MountZipFiles>();
        services.AddSingleton<IMountIsoFiles, MountIsoFiles>();
        services.AddSingleton<IMountChdFiles, MountChdFiles>();
        services.AddSingleton<IMountXisoFiles, MountXisoFiles>();
        services.AddSingleton<FileFinderService>();
        services.AddSingleton<IFileLockService, FileLockService>();

        // File/disk services (non-static wrappers)
        services.AddSingleton<CleanTempFolderService>();
        services.AddSingleton<ICleanTempFolderService>(sp => sp.GetRequiredService<CleanTempFolderService>());
        services.AddSingleton<CleanSimpleLauncherFolderService>();
        services.AddSingleton<ICleanSimpleLauncherFolderService>(sp => sp.GetRequiredService<CleanSimpleLauncherFolderService>());
        services.AddSingleton<DeleteFilesService>();
        services.AddSingleton<IDeleteFilesService>(sp => sp.GetRequiredService<DeleteFilesService>());
        services.AddSingleton<FormatFileSizeService>();
        services.AddSingleton<IFormatFileSizeService>(sp => sp.GetRequiredService<FormatFileSizeService>());

        // ── Credential protector ──
        services.AddSingleton<ICredentialProtector, WindowsCredentialProtector>();

        // ── App-specific services ──
        services.AddSingleton<SystemArtRatioService>();

        // ── ViewModels ──
        services.AddSingleton<MainViewModel>();
        services.AddTransient<EasyModeViewModel>();

        // ── App services (Phase 4–6) ──
        services.AddSingleton(_ => FavoritesManager.LoadFavorites(Log.Logger));
        services.AddSingleton(_ => PlayHistoryManager.LoadPlayHistory(Log.Logger));
        services.AddSingleton<SystemManagerService>();
        // Single shared launcher instance: MainViewModel reads LastPlayTime from the
        // concrete type, so ILauncherService and MinimalLauncherService must resolve
        // to the SAME instance.
        services.AddSingleton<MinimalLauncherService>();
        services.AddSingleton<ILauncherService>(sp => sp.GetRequiredService<MinimalLauncherService>());
        services.AddSingleton<AskAiToFixParameters>();
        services.AddSingleton<EmulatorPathResolver>();
        services.AddSingleton<LocalizationService>();
        services.AddSingleton<StorefrontGameScanner>();
        services.AddSingleton<ChdMountService>();
        services.AddSingleton<RetroAchievementsService>();

        // NOTE: The 21 IEmulatorConfigHandler implementations and their Inject*ConfigWindow
        // dialogs are NOT ported yet (deferred scope). MinimalLauncherService will simply
        // find no matching handlers until they are added.

        // ── Windows (transient — new instance each resolve) ──
        // NOTE: GameDetailWindow is intentionally NOT registered — it takes per-game
        // constructor arguments (GameCardViewModel + MainViewModel) and is created manually.
        services.AddTransient<MainWindow>();
        services.AddTransient<PreferencesWindow>();
        services.AddTransient<EasyModeWindow>();
        services.AddTransient<EditSystemWindow>();
        // Factory so callers can pass a pre-selected system name to EditSystemWindow
        // (the plain AddTransient above always resolves with the optional null default).
        services.AddTransient<Func<string?, EditSystemWindow>>(sp =>
            preSelectedSystemName => new EditSystemWindow(
                sp.GetRequiredService<PlaySoundEffects>(),
                sp.GetRequiredService<IConfiguration>(),
                sp.GetRequiredService<IMessageBoxLibraryService>(),
                sp.GetRequiredService<ILogger>(),
                sp.GetRequiredService<ISystemConfigurationWriterService>(),
                sp.GetRequiredService<SystemManagerService>(),
                sp.GetRequiredService<IFilePickerService>(),
                preSelectedSystemName));
    }

    /// <summary>
    /// Listens for a signal from a second instance and brings the main window to foreground.
    /// </summary>
    private async Task ListenForSecondInstanceAsync(IClassicDesktopStyleApplicationLifetime lifetime)
    {
        while (_instanceSignal is not null)
        {
            try
            {
                await Task.Run(() => _instanceSignal.WaitOne());
            }
            catch (ObjectDisposedException)
            {
                Log.Debug("Instance signal disposed; stopping second-instance listener");
                break;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (lifetime.MainWindow is { } window)
                {
                    if (window.WindowState == WindowState.Minimized)
                    {
                        window.WindowState = WindowState.Normal;
                    }

                    window.Activate();
                    window.Topmost = true;
                    window.Topmost = false;
                    window.Focus();
                }
            });
        }
    }

    #region Global Exception Handlers

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        Log.Fatal(ex, "Unhandled domain exception");
        if (ex != null)
            LogExceptionToFile(ex);
    }

    private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled dispatcher exception");
        LogExceptionToFile(e.Exception);

        var lifetime = (Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime);

        // If the main window is no longer visible (startup template crash, window already
        // closed, etc.), shutting down avoids a headless process running in the background.
        if (lifetime?.MainWindow is not { IsVisible: true })
        {
            Log.Fatal(e.Exception, "Main window is not visible; shutting down to avoid a background process");
            lifetime?.Shutdown();
            return;
        }

        e.Handled = true; // Prevent crash; app continues
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        LogExceptionToFile(e.Exception);
        e.SetObserved();
    }

    private static void LogExceptionToFile(Exception ex)
    {
        try
        {
            var crashPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SimpleLauncher", "crash_new.log");
            File.AppendAllText(crashPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (Exception logEx)
        {
            Log.Error(logEx, "Failed to write crash log");
        }
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Cleans up the single-instance mutex and event handle.
    /// </summary>
    public void Dispose()
    {
        _singleInstanceMutex?.Dispose();
        _instanceSignal?.Dispose();
        GC.SuppressFinalize(this);
    }

    #endregion
}
