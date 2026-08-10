using Moq;
using SimpleLauncher.Avalonia.Services.GameLauncher;
using SimpleLauncher.Avalonia.Services.SystemManager;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.SystemConfiguration;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests the launch-gating contract: a config handler returning false aborts the
/// launch and clears the loading state (WPF parity), while a returning-true handler
/// lets the flow continue to the pre-flight checks.
/// </summary>
public class MinimalLauncherServiceGatingTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _isoPath;
    private readonly FakeLoadingState _loading = new();
    private readonly Mock<IMessageBoxLibraryService> _messageBox = new();
    private readonly Mock<IEmulatorConfigHandler> _handler = new();

    public MinimalLauncherServiceGatingTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SLTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _isoPath = Path.Combine(_tempDir, "game.iso");
        File.WriteAllText(_isoPath, "fake iso");
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
            // best effort cleanup
        }
    }

    private MinimalLauncherService CreateLauncher(IEnumerable<IEmulatorConfigHandler> handlers)
    {
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
        var logger = new Mock<ILogger>().Object;
        var systemManager = new SystemManagerService(config);

        var askAi = new AskAiToFixParameters(
            _messageBox.Object,
            new Mock<IParameterResolverService>().Object,
            new Mock<ISystemConfigurationWriterService>().Object,
            systemManager,
            logger);

        return new MinimalLauncherService(
            _messageBox.Object,
            handlers,
            new ChdMountService(),
            config,
            new Mock<IExtractionService>().Object,
            new Mock<IMountXisoFiles>().Object,
            askAi);
    }

    private static Emulator CreateEmulator(string name) => new()
    {
        EmulatorName = name,
        EmulatorLocation = "/usr/bin/fake-emulator",
        EmulatorParameters = "",
        ReceiveANotificationOnEmulatorError = false,
        ImagePackDownloadLink = "",
        ImagePackDownloadLink2 = "",
        ImagePackDownloadLink3 = "",
        ImagePackDownloadLink4 = "",
        ImagePackDownloadLink5 = "",
        ImagePackDownloadExtractPath = ""
    };

    private static ISystemManager CreateSystem() => Mock.Of<ISystemManager>(s =>
        s.SystemName == "Test System" &&
        s.FileFormatsToLaunch == new List<string> { ".iso" });

    [Fact]
    public async Task HandlerReturningFalse_AbortsLaunchAndClearsLoadingState()
    {
        _handler.Setup(h => h.IsMatch(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _handler.Setup(h => h.HandleConfigurationAsync(It.IsAny<LaunchContext>())).ReturnsAsync(false);

        var launcher = CreateLauncher([_handler.Object]);

        await launcher.LaunchRegularEmulatorAsync(
            _isoPath,
            "TestEmulator",
            CreateSystem(),
            CreateEmulator("TestEmulator"),
            "game.iso",
            new Mock<IWindowContext>().Object,
            _loading);

        // Handler ran with a populated context…
        _handler.Verify(h => h.HandleConfigurationAsync(It.Is<LaunchContext>(c =>
            c.EmulatorName == "TestEmulator" &&
            c.ResolvedFilePath == _isoPath)), Times.Once);

        // …the "Configuring emulator..." state was shown, then cleared, and nothing else ran.
        Assert.Contains(_loading.Calls, c => c.IsLoading && c.Message == "Configuring emulator...");
        Assert.True(_loading.Calls.Last().IsLoading == false, "Loading state must be cleared after abort");
        _messageBox.Verify(m => m.CustomErrorMessageBoxAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task HandlerReturningTrue_ProceedsPastHandlersToPreflight()
    {
        _handler.Setup(h => h.IsMatch(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _handler.Setup(h => h.HandleConfigurationAsync(It.IsAny<LaunchContext>())).ReturnsAsync(true);

        var launcher = CreateLauncher([_handler.Object]);

        // "RetroArch Test" without a -L parameter trips the pre-flight check AFTER handlers,
        // proving the launch continued past the handler block (and we never Process.Start).
        await launcher.LaunchRegularEmulatorAsync(
            _isoPath,
            "RetroArch Test",
            CreateSystem(),
            CreateEmulator("RetroArch Test"),
            "game.iso",
            new Mock<IWindowContext>().Object,
            _loading);

        _messageBox.Verify(m => m.CustomErrorMessageBoxAsync(
            It.Is<string>(t => t.Contains("-L", StringComparison.Ordinal)), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task NoMatchingHandler_SkipsHandlerBlock()
    {
        _handler.Setup(h => h.IsMatch(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var launcher = CreateLauncher([_handler.Object]);

        await launcher.LaunchRegularEmulatorAsync(
            _isoPath,
            "RetroArch Test",
            CreateSystem(),
            CreateEmulator("RetroArch Test"),
            "game.iso",
            new Mock<IWindowContext>().Object,
            _loading);

        _handler.Verify(h => h.HandleConfigurationAsync(It.IsAny<LaunchContext>()), Times.Never);
        // Flow reached pre-flight directly (no handler matched).
        _messageBox.Verify(m => m.CustomErrorMessageBoxAsync(
            It.Is<string>(t => t.Contains("-L", StringComparison.Ordinal)), It.IsAny<string>()), Times.Once);
    }

    private sealed class FakeLoadingState : ILoadingState
    {
        public List<(bool IsLoading, string? Message)> Calls { get; } = [];

        public void SetLoadingState(bool isLoading, string? message = null)
        {
            Calls.Add((isLoading, message));
        }
    }
}
