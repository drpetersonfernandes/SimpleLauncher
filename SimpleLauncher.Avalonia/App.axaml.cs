using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.CleanAndDeleteFiles;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.ViewModels;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Avalonia.Views;
using IApplicationLifetime = SimpleLauncher.Core.Interfaces.IApplicationLifetime;

namespace SimpleLauncher.Avalonia;

public class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
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

        // SettingsManager (from Core)
        services.AddSingleton<SimpleLauncher.Core.Services.SettingsManager.SettingsManager>();

        // Logging and debugging
        services.AddSingleton<IDebugLogger, AvaloniaDebugLogger>();
        services.AddSingleton<ILogErrors, LogErrorsService>();

        // Sound effects (no-op for Avalonia until cross-platform audio is implemented)
        services.AddSingleton<IPlaySoundEffects, NoOpPlaySoundEffects>();

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

        // Windows
        services.AddTransient<ImageViewerWindow>();
        services.AddTransient<FlashOverlayWindow>();
        services.AddTransient<RetroAchievementsWindow>();
        services.AddTransient<RetroAchievementsForAGameWindow>();
    }
}
