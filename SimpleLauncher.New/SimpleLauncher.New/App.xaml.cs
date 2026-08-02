using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using SimpleLauncher.New.Services;
using SimpleLauncher.New.Services.Favorites;
using SimpleLauncher.New.Services.GameLauncher;
using SimpleLauncher.New.Services.GameLauncher.Handlers;
using SimpleLauncher.New.Services.GameScan;
using SimpleLauncher.New.Services.PlayHistory;
using SimpleLauncher.New.Services.RetroAchievements;
using SimpleLauncher.New.Services.SystemManager;
using SimpleLauncher.New.Services.WpfServices;
using SimpleLauncher.New.ViewModels;
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

namespace SimpleLauncher.New;

/// <summary>
/// Application entry point handling DI container setup, single-instance enforcement, and global error handling.
/// </summary>
public partial class App : IDisposable
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
    protected override void OnStartup(StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
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

            Shutdown();
            return;
        }

        // Configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
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

        Log.Information("SimpleLauncher.New starting up");

        // DI container
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection, configuration);

        ServiceProvider = serviceCollection.BuildServiceProvider();

        // Initialize the bug report sink with DI services (queues Warning+ events to the API)
        bugReportSink.Initialize(
            ServiceProvider.GetRequiredService<IHttpClientFactory>(),
            ServiceProvider.GetRequiredService<IConfiguration>(),
            ServiceProvider.GetRequiredService<IDeleteFilesService>(),
            appDataLogFolder);

        // Start listening for second-instance signals
        _ = ListenForSecondInstanceAsync();

        // Show main window
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        ApplyDarkTitleBar(mainWindow);
        mainWindow.Show();
    }

    /// <summary>
    /// Registers all services, ViewModels, and windows in the DI container.
    /// </summary>
    internal static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
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
            client.DefaultRequestHeaders.Add("User-Agent", "SimpleLauncher.New/1.0");
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

        // ── WPF host services (implement Core interfaces) ──
        services.AddSingleton<IDispatcherService, WpfDispatcherService>();
        services.AddSingleton<IFilePickerService, WpfFilePickerService>();
        services.AddSingleton<IResourceProvider, WpfResourceProvider>();
        services.AddSingleton<IWindowContext, WpfWindowContext>();
        services.AddSingleton<IApplicationLifetime, WpfApplicationLifetime>();
        services.AddSingleton<IMessageBoxLibraryService, MessageBoxLibraryService>();

        // ── Core services (from SimpleLauncher.Core) ──
        services.AddSingleton<DataFileLocation>();
        services.AddSingleton<InputSanitizerService>();
        services.AddSingleton<WindowsVersionService>();
        services.AddSingleton<BugReportApiSink>();
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

        // ── New app-specific services ──
        services.AddSingleton<SystemArtRatioService>();

        // ── ViewModels ──
        services.AddSingleton<MainViewModel>();
        services.AddTransient<EasyModeViewModel>();

        // ── App services (Phase 4–6) ──
        services.AddSingleton(_ => FavoritesManager.LoadFavorites(Log.Logger));
        services.AddSingleton(_ => PlayHistoryManager.LoadPlayHistory(Log.Logger));
        services.AddSingleton<SystemManagerService>();
        services.AddSingleton<ILauncherService, MinimalLauncherService>();
        services.AddSingleton<LocalizationService>();
        services.AddSingleton<StorefrontGameScanner>();
        services.AddSingleton<ChdMountService>();
        services.AddSingleton<RetroAchievementsService>();

        // ── Emulator config handlers (21) ──
        services.AddSingleton<IEmulatorConfigHandler, RetroArchConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, Pcsx2ConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, DuckStationConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, DolphinConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, MameConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, FlycastConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, Rpcs3ConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, XeniaConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, CemuConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, AresConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, AzaharConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, BlastemConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, DaphneConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, MednafenConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, MesenConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, RaineConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, RedreamConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, SegaModel2ConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, StellaConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, SupermodelConfigHandler>();
        services.AddSingleton<IEmulatorConfigHandler, YumirConfigHandler>();

        // ── Windows (transient — new instance each resolve) ──
        // NOTE: GameDetailWindow is intentionally NOT registered — it takes per-game
        // constructor arguments (GameCardViewModel + MainViewModel) and is created manually.
        services.AddTransient<MainWindow>();
        services.AddTransient<PreferencesWindow>();
        services.AddTransient<EasyModeWindow>();
        services.AddTransient<EditSystemWindow>();
    }

    /// <summary>
    /// Applies Windows dark title bar to the main window via DWM.
    /// </summary>
    private static void ApplyDarkTitleBar(Window window)
    {
        window.SourceInitialized += (_, _) =>
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
            // DWMWA_USE_IMMERSIVE_DARK_MODE = 20 (Windows 10 20H1+) / 19 (pre-20H1)
            int useDarkMode = 1;
            const int darkModeAttributeWin11 = 20;
            const int darkModeAttributeWin10 = 19;

            // Try Win11/Win10 20H1+ attribute first
            var result = DwmSetWindowAttribute(hwnd, darkModeAttributeWin11, ref useDarkMode, sizeof(int));
            if (result != 0)
            {
                // Fallback to older attribute
                _ = DwmSetWindowAttribute(hwnd, darkModeAttributeWin10, ref useDarkMode, sizeof(int));
            }
        };
    }

    /// <summary>
    /// Listens for a signal from a second instance and brings the main window to foreground.
    /// </summary>
    private async Task ListenForSecondInstanceAsync()
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

            await Dispatcher.InvokeAsync(() =>
            {
                if (Current.MainWindow is { } window)
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

    private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled dispatcher exception");
        LogExceptionToFile(e.Exception);

        // If the main window is no longer visible (startup template crash, window already
        // closed, etc.), continuing with e.Handled = true would leave a headless process
        // running in the background with no way to close it. Shut down instead.
        if (Current.MainWindow is not { IsVisible: true })
        {
            Log.Fatal(e.Exception, "Main window is not visible; shutting down to avoid a background process");
            Current.Shutdown();
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

    #region P/Invoke

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    #endregion

    #region IDisposable

    /// <summary>
    /// Flushes pending log events on application exit.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            Log.CloseAndFlush();
        }
        catch (Exception ex)
        {
            // Never let logging issues block shutdown
            System.Diagnostics.Debug.WriteLine($"CloseAndFlush failed: {ex.Message}");
        }

        base.OnExit(e);
    }

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
