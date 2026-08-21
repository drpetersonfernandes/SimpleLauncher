using System.Net.Http;
using Moq;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.Services.Favorites;
using SimpleLauncher.Avalonia.Services.GameLauncher;
using SimpleLauncher.Avalonia.Services.PlayHistory;
using SimpleLauncher.Avalonia.Services.SystemManager;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.UsageStats;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.Services.SettingsManager;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.GameLauncher.Strategies;
using SimpleLauncher.Core.Services.GamePad;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the WPF-parity quick actions added to MainViewModel (Phase 11):
/// the letter filter bar, Feeling Lucky (random game), the MAME sort-order toggle,
/// and Ctrl+wheel card-size zoom. All I/O is isolated to temp ROM folders and a
/// temp system.xml (the same pattern as GameScannerServiceTests).
/// </summary>
public class MainViewModelQuickActionsTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), $"SL_QuickActionsTest_{Guid.NewGuid():N}");
    private readonly string _romsFolder;
    private readonly string _systemXmlPath;
    private readonly IConfiguration _config;
    private readonly MainViewModel _viewModel;
    private readonly Mock<IMessageBoxLibraryService> _messageBox = new();
    private readonly Mock<ILogger> _logger = new();
    private readonly Mock<IMameDataService> _mameData = new();

    public MainViewModelQuickActionsTests()
    {
        _romsFolder = Path.Combine(_tempRoot, "roms");
        Directory.CreateDirectory(_romsFolder);
        WriteGameFile("abyss.zip");
        WriteGameFile("beyond.zip");
        WriteGameFile("cantina.zip");
        WriteGameFile("123start.zip");
        WriteGameFile("zero.was");

        _systemXmlPath = Path.Combine(_tempRoot, "system.xml");
        File.WriteAllText(_systemXmlPath, $$"""
            <SystemConfigs>
              <SystemConfig>
                <SystemName>Test System</SystemName>
                <SystemFolders>
                  <SystemFolder>{{_romsFolder}}</SystemFolder>
                </SystemFolders>
                <SystemImageFolder>{{_romsFolder}}\images</SystemImageFolder>
                <FileFormatsToSearch>
                  <FormatToSearch>.zip</FormatToSearch>
                  <FormatToSearch>.was</FormatToSearch>
                </FileFormatsToSearch>
                <FileFormatsToLaunch>
                  <FormatToLaunch>.zip</FormatToLaunch>
                </FileFormatsToLaunch>
              </SystemConfig>
            </SystemConfigs>
            """);

        _config = TestEnvironment.ConfigurationFromJson($$"""{"SystemXmlPath": "{{_systemXmlPath.Replace("\\", @"\\")}}"}""");

        var settings = TestDependencies.Settings(_config, _messageBox);
        var systemManager = new SystemManagerService(_config);
        var loadingOrchestrator = new AvaloniaGameFileLoadingOrchestrator(
            new AvaloniaGameCacheService(), _logger.Object);
        var pagination = new AvaloniaPaginationService(TestDependencies.ResourceProvider().Object);

        _mameData.Setup(m => m.Lookup).Returns(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "beyond", "Beyond the Beyond" },
            { "cantina", "Cantina" }
        });

        _viewModel = new MainViewModel(
            new FavoritesManager(),
            new PlayHistoryManager(),
            systemManager,
            CreateLauncher(systemManager, settings),
            new Mock<IFindCoverImageService>().Object,
            new Stats(TestDependencies.HttpFactory(new HttpClient()).Object, _config, _logger.Object),
            settings,
            pagination,
            loadingOrchestrator,
            new Mock<IRetroAchievementsHashScanner>().Object,
            new Mock<IRetroAchievementsHashStore>().Object,
            new RetroAchievementsManager(),
            _messageBox.Object,
            _mameData.Object);

        // Paginate only above 1 million games so the test views are never sliced.
        _viewModel.ConfigurePagination(1_000_000);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true);
        }
        catch
        {
            // best effort cleanup
        }

        GC.SuppressFinalize(this);
    }

    private void WriteGameFile(string name)
    {
        File.WriteAllText(Path.Combine(_romsFolder, name), "fake rom");
    }

    private static MinimalLauncherService CreateLauncher(SystemManagerService systemManager, SettingsManagerService settings)
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
        var logger = new Mock<ILogger>().Object;
        var messageBox = new Mock<IMessageBoxLibraryService>();

        var askAi = new AskAiToFixParameters(
            messageBox.Object,
            new Mock<IParameterResolverService>().Object,
            new Mock<ISystemConfigurationWriterService>().Object,
            systemManager,
            logger);

        return new MinimalLauncherService(
            messageBox.Object,
            [],
            config,
            new Mock<IExtractionService>().Object,
            new Mock<IMountXisoFiles>().Object,
            new Mock<IMountChdFiles>().Object,
            new Mock<IMountZipFiles>().Object,
            askAi,
            settings,
            [new DefaultLaunchStrategy()],
            new PlayHistoryManager(),
            new Mock<Stats>(new Mock<IHttpClientFactory>().Object, config, logger).Object,
            new Mock<GamePadController>(messageBox.Object, config, logger).Object);
    }

    private int FullGameCount()
    {
        _viewModel.ClearLetterFilter();
        return _viewModel.Games.Count;
    }

    [Fact]
    public void LetterFilter_FiltersByFirstLetterOfFileName()
    {
        _viewModel.NavigateToAllGamesCommand.Execute(null);

        _viewModel.SetLetterFilter("B");

        var titles = _viewModel.Games.Select(static g => Path.GetFileName(g.FilePath)).ToList();
        Assert.True(titles.All(t => t.StartsWith("b", StringComparison.OrdinalIgnoreCase)));
        Assert.Contains("beyond.zip", titles);
        Assert.DoesNotContain("abyss.zip", titles);
        Assert.DoesNotContain("cantina.zip", titles);
    }

    [Fact]
    public void LetterFilter_HashMatchesDigitLedFiles()
    {
        _viewModel.NavigateToAllGamesCommand.Execute(null);
        _viewModel.SetLetterFilter("#");

        var titles = _viewModel.Games.Select(static g => Path.GetFileName(g.FilePath)).ToList();
        Assert.Contains("123start.zip", titles);
        Assert.DoesNotContain("abyss.zip", titles);
    }

    [Fact]
    public void LetterFilter_ClearRestoresAllGames()
    {
        _viewModel.NavigateToAllGamesCommand.Execute(null);
        _viewModel.SetLetterFilter("C");

        Assert.True(_viewModel.Games.Count < FullGameCount());

        _viewModel.ClearLetterFilter();

        Assert.Equal(FullGameCount(), _viewModel.Games.Count);
    }

    [Fact]
    public void RandomGame_ReturnsGameFromCurrentView()
    {
        _viewModel.NavigateToAllGamesCommand.Execute(null);

        var randomGame = _viewModel.GetRandomGame();

        Assert.NotNull(randomGame);
        Assert.Contains(_viewModel.Games, g => g.FilePath == randomGame!.FilePath);
    }

    [Fact]
    public void ToggleMameSortOrder_ReordersByMachineDescription()
    {
        _viewModel.NavigateToAllGamesCommand.Execute(null);

        _viewModel.ToggleMameSortOrder();

        var titles = _viewModel.Games.Select(static g => Path.GetFileNameWithoutExtension(g.FilePath)).ToList();

        // Machine-description order: "beyond" → "Beyond the Beyond" and "cantina" →
        // "Cantina" replace their file names for sorting; the other games keep theirs.
        // Keys: 123start, abyss, Beyond the Beyond, Cantina, zero
        Assert.Equal(3, titles.IndexOf("cantina"));
        Assert.Equal(2, titles.IndexOf("beyond"));
        Assert.Equal(0, titles.IndexOf("123start"));
    }

    [Fact]
    public void ZoomIn_IncreasesCardWidthWithinBounds()
    {
        _viewModel.CardWidth = 780;

        _viewModel.ZoomIn();

        Assert.True(_viewModel.CardWidth > 780);
        Assert.True(_viewModel.CardWidth <= 800);
    }

    [Fact]
    public void ZoomOut_DecreasesCardWidthWithinBounds()
    {
        _viewModel.CardWidth = 100;

        _viewModel.ZoomOut();

        Assert.True(_viewModel.CardWidth < 100);
        Assert.True(_viewModel.CardWidth >= 50);
    }

    [Fact]
    public void Zoom_ClampsAtLimits()
    {
        _viewModel.CardWidth = 800;
        _viewModel.ZoomIn();
        Assert.Equal(800, (int)_viewModel.CardWidth);

        _viewModel.CardWidth = 50;
        _viewModel.ZoomOut();
        Assert.Equal(50, (int)_viewModel.CardWidth);
    }
}