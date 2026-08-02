using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DownloadService;
using SimpleLauncher.Core.Services.EasyMode;
using SimpleLauncher.New.ViewModels;
using Serilog;

namespace SimpleLauncher.New.Tests;

/// <summary>
/// Smoke tests that build the real DI container (via App.ConfigureServices) and resolve
/// key services. Catches missing registrations that break windows at runtime
/// (e.g. the EasyModeWindow chain requiring IExtractionService for DownloadManager).
/// </summary>
public class DiSmokeTests
{
    private static IServiceProvider BuildContainer()
    {
        // App.ConfigureServices registers Log.Logger; ensure it is non-null first.
        Log.Logger ??= new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .CreateLogger();

        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        App.ConfigureServices(services, configuration);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void EasyModeViewModel_ResolvesFromContainer()
    {
        var provider = BuildContainer();
        using var vm = provider.GetRequiredService<EasyModeViewModel>();
        Assert.NotNull(vm);
    }

    [Fact]
    public void DownloadManager_ResolvesFromContainer()
    {
        var provider = BuildContainer();
        var dm = provider.GetRequiredService<DownloadManager>();
        Assert.NotNull(dm);
    }

    [Fact]
    public void EasyModeManager_ResolvesFromContainer()
    {
        var provider = BuildContainer();
        var em = provider.GetRequiredService<EasyModeManager>();
        Assert.NotNull(em);
    }

    [Fact]
    public void IExtractionService_ResolvesFromContainer()
    {
        var provider = BuildContainer();
        var es = provider.GetRequiredService<IExtractionService>();
        Assert.NotNull(es);
    }

    [Fact]
    public void ISystemConfigurationWriterService_ResolvesFromContainer()
    {
        var provider = BuildContainer();
        var writer = provider.GetRequiredService<ISystemConfigurationWriterService>();
        Assert.NotNull(writer);
    }
}
