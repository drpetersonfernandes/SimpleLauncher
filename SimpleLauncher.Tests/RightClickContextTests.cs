using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class RightClickContextTests
{
    [Fact]
    public void ConstructorSetsFilePath()
    {
        var context = CreateContext(@"C:\roms\game.zip");
        Assert.Equal(@"C:\roms\game.zip", context.FilePath);
    }

    [Fact]
    public void ConstructorSetsFileNameWithExtension()
    {
        var context = CreateContext(fileNameWithExtension: "game.zip");
        Assert.Equal("game.zip", context.FileNameWithExtension);
    }

    [Fact]
    public void ConstructorSetsFileNameWithoutExtension()
    {
        var context = CreateContext(fileNameWithoutExtension: "game");
        Assert.Equal("game", context.FileNameWithoutExtension);
    }

    [Fact]
    public void ConstructorSetsSelectedSystemName()
    {
        var context = CreateContext(selectedSystemName: "NES");
        Assert.Equal("NES", context.SelectedSystemName);
    }

    [Fact]
    public void ConstructorSetsMachines()
    {
        var machines = new List<Services.MameManager.MameManager>();
        var context = CreateContext(machines: machines);
        Assert.Same(machines, context.Machines);
    }

    [Fact]
    public void ConstructorSetsFavoritesManager()
    {
        var context = CreateContext();
        Assert.NotNull(context.FavoritesManager);
    }

    [Fact]
    public void ConstructorSetsSettings()
    {
        var context = CreateContext();
        Assert.NotNull(context.Settings);
    }

    private static RightClickContext CreateContext(
        string filePath = "game.zip",
        string fileNameWithExtension = "game.zip",
        string fileNameWithoutExtension = "game",
        string selectedSystemName = "NES",
        List<Services.MameManager.MameManager>? machines = null)
    {
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
        var logErrors = new NoOpLogErrors();
        var credentialProtector = new NoOpCredentialProtector();
        var settings = new SettingsManager(configuration, logErrors, credentialProtector);
        var favoritesManager = new FavoritesManager();

        return new RightClickContext(
            filePath,
            fileNameWithExtension,
            fileNameWithoutExtension,
            selectedSystemName,
            new Services.SystemManager.SystemManager { SystemName = selectedSystemName },
            machines ?? [],
            favoritesManager,
            settings,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            loadingStateProvider: new NoOpLoadingState());
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpLoadingState : SimpleLauncher.Services.LoadingInterface.ILoadingState
    {
        public void SetLoadingState(bool isLoading, string? message = null)
        {
        }
    }
}
