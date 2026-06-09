using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.CheckForFileLock;
using SimpleLauncher.Core.Services.CheckIfDirectoryIsWritable;
using SimpleLauncher.Core.Services.CleanAndDeleteFiles;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.DownloadService;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.Services.SanitizeInputString;
using SimpleLauncher.Core.ViewModels;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Avalonia.Views;
using IApplicationLifetime = SimpleLauncher.Core.Interfaces.IApplicationLifetime;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public class App : Application, IDisposable
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;
    private AvaloniaTrayIconManager? _trayIconManager;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        // Apply theme from settings
        var settings = ServiceProvider.GetRequiredService<SimpleLauncher.Core.Services.SettingsManager.SettingsManager>();
        settings.Load();
        var themeService = ServiceProvider.GetRequiredService<IThemeService>();
        themeService.ApplyTheme(settings.BaseTheme, settings.AccentColor);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
            };
            desktop.MainWindow = mainWindow;

            // Initialize tray icon
            var logErrors = ServiceProvider.GetRequiredService<ILogErrors>();
            var appLifetime = ServiceProvider.GetRequiredService<IApplicationLifetime>();
            _trayIconManager = new AvaloniaTrayIconManager(mainWindow, logErrors, appLifetime, ServiceProvider);

            // Handle window closing to minimize to tray
            mainWindow.Closing += (_, e) =>
            {
                mainWindow.Hide();
                e.Cancel = true;
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", true, true)
            .Build();
        services.AddSingleton<IConfiguration>(configuration);

        // Core services
        services.AddMemoryCache();
        services.AddHttpClient("LogErrorsClient");

        // Credential protection
        services.AddSingleton<ICredentialProtector, CrossPlatformCredentialProtector>();

        // SettingsManager (from Core)
        services.AddSingleton<SimpleLauncher.Core.Services.SettingsManager.SettingsManager>();

        // Logging and debugging
        services.AddSingleton<IDebugLogger, AvaloniaDebugLogger>();
        services.AddSingleton<ILogErrors, LogErrorsService>();

        // Sound effects (no-op for Avalonia until cross-platform audio is implemented)
        services.AddSingleton<IPlaySoundEffects, NoOpPlaySoundEffects>();

        // Gamepad (no-op for Avalonia until cross-platform gamepad is implemented)
        services.AddSingleton<IGamePadController, NoOpGamePadController>();

        // RetroAchievements services
        services.AddSingleton<RetroAchievementsManager>(static sp =>
        {
            var logErrors = sp.GetRequiredService<ILogErrors>();
            var debugLogger = sp.GetRequiredService<IDebugLogger>();
            return RetroAchievementsManager.LoadRetroAchievement(logErrors, debugLogger);
        });
        services.AddSingleton<RetroAchievementsService>();

        // Platform services (Avalonia implementations)
        services.AddSingleton<IDispatcherService, AvaloniaDispatcherService>();
        services.AddSingleton<IDeleteFilesService, DeleteFilesService>();
        services.AddSingleton<IMessageDialogService, AvaloniaMessageDialogService>();
        services.AddSingleton<IResourceProvider, AvaloniaResourceProvider>();
        services.AddSingleton<IApplicationLifetime, AvaloniaApplicationLifetime>();
        services.AddSingleton<IFilePickerService, AvaloniaFilePickerService>();
        services.AddSingleton<IImageLoader, AvaloniaImageLoader>();
        services.AddSingleton<IMessageBoxLibraryService, AvaloniaMessageBoxLibraryService>();
        services.AddSingleton<IThemeService, AvaloniaThemeService>();

        // Core service implementations
        services.AddSingleton<IWindowsVersionService, WindowsVersionService>();
        services.AddSingleton<IDirectoryValidationService, DirectoryValidationService>();
        services.AddSingleton<IFileLockService, FileLockService>();
        services.AddSingleton<IInputSanitizerService, InputSanitizerService>();
        services.AddSingleton<IBugReportFormatter, BugReportFormatterService>();
        services.AddSingleton<IFileFinderService, FileFinderService>();
        services.AddSingleton<IFormatFileSizeService, FormatFileSizeService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<FavoritesViewModel>();
        services.AddTransient<GlobalSearchViewModel>();
        services.AddTransient<PlayHistoryViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<EasyModeViewModel>();
        services.AddTransient<EditSystemViewModel>();
        services.AddTransient<FlashOverlayViewModel>();
        services.AddTransient<AvaloniaImageViewerViewModel>();
        services.AddTransient<AvaloniaRetroAchievementsViewModel>();
        services.AddTransient<AvaloniaRetroAchievementsForAGameViewModel>();
        services.AddTransient<DosBoxFileSelectionViewModel>();
        services.AddTransient<SystemSelectionViewModel>();
        services.AddTransient<WindowSelectionDialogViewModel>();
        services.AddTransient<UpdateLogViewModel>();
        services.AddTransient<UpdateHistoryViewModel>();
        services.AddTransient<AvaloniaSetFuzzyMatchingViewModel>();
        services.AddTransient<AvaloniaDebugViewModel>();
        services.AddTransient<AvaloniaRomHistoryViewModel>();
        services.AddTransient<AvaloniaDownloadImagePackViewModel>();
        services.AddTransient<AvaloniaRetroAchievementsSettingsViewModel>();
        services.AddTransient<AvaloniaInjectAresConfigViewModel>();
        services.AddTransient<AvaloniaInjectAzaharConfigViewModel>();
        services.AddTransient<AvaloniaInjectBlastemConfigViewModel>();
        services.AddTransient<AvaloniaInjectCemuConfigViewModel>();
        services.AddTransient<AvaloniaInjectDaphneConfigViewModel>();
        services.AddTransient<AvaloniaInjectDolphinConfigViewModel>();
        services.AddTransient<AvaloniaInjectDuckStationConfigViewModel>();
        services.AddTransient<AvaloniaInjectFlycastConfigViewModel>();
        services.AddTransient<AvaloniaInjectMameConfigViewModel>();
        services.AddTransient<AvaloniaInjectMednafenConfigViewModel>();
        services.AddTransient<AvaloniaInjectMesenConfigViewModel>();
        services.AddTransient<AvaloniaInjectPcsx2ConfigViewModel>();
        services.AddTransient<AvaloniaInjectRaineConfigViewModel>();
        services.AddTransient<AvaloniaInjectRedreamConfigViewModel>();
        services.AddTransient<AvaloniaInjectRetroArchConfigViewModel>();
        services.AddTransient<AvaloniaInjectRpcs3ConfigViewModel>();
        services.AddTransient<AvaloniaInjectSegaModel2ConfigViewModel>();
        services.AddTransient<AvaloniaInjectStellaConfigViewModel>();
        services.AddTransient<AvaloniaInjectSupermodelConfigViewModel>();
        services.AddTransient<AvaloniaInjectXeniaConfigViewModel>();
        services.AddTransient<AvaloniaInjectYumirConfigViewModel>();

        // Windows
        services.AddTransient<ImageViewerWindow>();
        services.AddTransient<FlashOverlayWindow>();
        services.AddTransient<RetroAchievementsWindow>();
        services.AddTransient<RetroAchievementsForAGameWindow>();
        services.AddTransient<DosBoxFileSelectionWindow>();
        services.AddTransient<SystemSelectionWindow>();
        services.AddTransient<WindowSelectionDialogWindow>();
        services.AddTransient<UpdateLogWindow>();
        services.AddTransient<UpdateHistoryWindow>();
        services.AddTransient<SetFuzzyMatchingWindow>();
        services.AddTransient<DebugWindow>();
        services.AddTransient<RomHistoryWindow>();
        services.AddTransient<DownloadImagePackWindow>();
        services.AddTransient<RetroAchievementsSettingsWindow>();
        services.AddTransient<InjectAresConfigWindow>();
        services.AddTransient<InjectAzaharConfigWindow>();
        services.AddTransient<InjectBlastemConfigWindow>();
        services.AddTransient<InjectCemuConfigWindow>();
        services.AddTransient<InjectDaphneConfigWindow>();
        services.AddTransient<InjectDolphinConfigWindow>();
        services.AddTransient<InjectDuckStationConfigWindow>();
        services.AddTransient<InjectFlycastConfigWindow>();
        services.AddTransient<InjectMameConfigWindow>();
        services.AddTransient<InjectMednafenConfigWindow>();
        services.AddTransient<InjectMesenConfigWindow>();
        services.AddTransient<InjectPcsx2ConfigWindow>();
        services.AddTransient<InjectRaineConfigWindow>();
        services.AddTransient<InjectRedreamConfigWindow>();
        services.AddTransient<InjectRetroArchConfigWindow>();
        services.AddTransient<InjectRpcs3ConfigWindow>();
        services.AddTransient<InjectSegaModel2ConfigWindow>();
        services.AddTransient<InjectStellaConfigWindow>();
        services.AddTransient<InjectSupermodelConfigWindow>();
        services.AddTransient<InjectXeniaConfigWindow>();
        services.AddTransient<InjectYumirConfigWindow>();
    }

    public void Dispose()
    {
        _trayIconManager?.Dispose();
        GC.SuppressFinalize(this);
    }
}
