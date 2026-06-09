using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Avalonia.Services;
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
        services.AddHttpClient();

        // SettingsManager (from Core)
        services.AddSingleton<SimpleLauncher.Core.Services.SettingsManager.SettingsManager>();

        // Platform services (Avalonia implementations)
        services.AddSingleton<IDispatcherService, AvaloniaDispatcherService>();
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
    }
}
