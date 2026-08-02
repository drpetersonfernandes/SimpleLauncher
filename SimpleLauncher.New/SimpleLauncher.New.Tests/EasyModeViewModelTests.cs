using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.DownloadService;
using SimpleLauncher.Core.Services.EasyMode;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.New.ViewModels;
using Serilog;

namespace SimpleLauncher.New.Tests;

/// <summary>
/// Regression tests for the EasyMode state machine: selecting a system must enable
/// the Download buttons (Idle state), disable them for already-downloaded components,
/// and gate the Add System button on required downloads.
/// </summary>
public class EasyModeViewModelTests
{
    private static EasyModeViewModel CreateViewModel()
    {
        Log.Logger ??= new LoggerConfiguration().CreateLogger();

        var configuration = new ConfigurationBuilder().Build();
        var logger = new Mock<ILogger>().Object;

        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        var settings = new SettingsManagerService(
            configuration, logger, new Mock<ICredentialProtector>().Object);

        var easyModeManager = new EasyModeManager(logger, configuration, httpFactory.Object, logger);
        var downloadManager = new DownloadManager(
            httpFactory.Object,
            new Mock<IExtractionService>().Object,
            logger,
            new Mock<IResourceProvider>().Object,
            new Mock<IDispatcherService>().Object);

        return new EasyModeViewModel(
            easyModeManager,
            downloadManager,
            new Mock<IMessageBoxLibraryService>().Object,
            logger,
            configuration,
            new PlaySoundEffects(settings, logger));
    }

    private static EasyModeSystemConfig BuildSystem(string emulatorLink = "https://example.com/emu.zip",
        string? coreLink = "https://example.com/core.zip",
        string imagePackLink = "https://example.com/pack1.zip")
    {
        return new EasyModeSystemConfig
        {
            SystemName = "NES",
            SystemFolder = @"%BASEFOLDER%\roms\NES",
            SystemImageFolder = @"%BASEFOLDER%\images\NES",
            FileFormatsToSearch = ["nes"],
            FileFormatsToLaunch = ["nes"],
            Emulators = new EmulatorsConfig
            {
                Emulator = new EmulatorConfig
                {
                    EmulatorName = "Mesen",
                    EmulatorDownloadLink = emulatorLink,
                    EmulatorDownloadExtractPath = "%BASEFOLDER%\\emulators",
                    CoreDownloadLink = coreLink ?? "",
                    CoreDownloadExtractPath = "%BASEFOLDER%\\cores",
                    ImagePackDownloadLink = imagePackLink,
                    ImagePackDownloadExtractPath = "%BASEFOLDER%\\images"
                }
            }
        };
    }

    [Fact]
    public void SelectingSystem_WithDownloadsPending_EnablesDownloadButtons()
    {
        using var vm = CreateViewModel();
        vm.SelectedSystem = BuildSystem();

        // Download links exist and files are not on disk → Idle → buttons ENABLED
        Assert.False(vm.IsEmulatorDownloaded, "Download Emulator button must be enabled");
        Assert.False(vm.IsCoreDownloaded, "Download Core button must be enabled");
        Assert.False(vm.IsImagePack1Downloaded, "Image Pack 1 button must be enabled");
        Assert.True(vm.IsImagePack1Available, "Image Pack 1 must be visible (link + extract path)");

        // Required components not downloaded yet → Add System stays disabled
        Assert.False(vm.IsAddSystemEnabled);

        // Default folder shown in the textbox
        Assert.Contains("roms\\NES", vm.SystemFolderPath);
    }

    [Fact]
    public void SelectingSystem_WithNoCoreLink_MarksCoreAsReady()
    {
        using var vm = CreateViewModel();
        vm.SelectedSystem = BuildSystem(coreLink: null);

        // No core download offered → core counts as downloaded → Core button disabled
        Assert.True(vm.IsCoreDownloaded, "Core button must be disabled when no core download is offered");
        // Emulator still pending → Add System still disabled
        Assert.False(vm.IsAddSystemEnabled);
    }

    [Fact]
    public void SelectingNullSystem_DisablesAllButtons()
    {
        using var vm = CreateViewModel();
        vm.SelectedSystem = null;

        Assert.True(vm.IsEmulatorDownloaded);
        Assert.True(vm.IsCoreDownloaded);
        Assert.False(vm.IsImagePack1Available);
        Assert.False(vm.IsAddSystemEnabled);
        Assert.Equal("", vm.SystemFolderPath);
    }

    [Fact]
    public void SelectingSystem_WithNoImagePackLink_HidesPack()
    {
        using var vm = CreateViewModel();
        vm.SelectedSystem = BuildSystem(imagePackLink: "");

        Assert.False(vm.IsImagePack1Available, "Image Pack 1 must be hidden when no link exists");
    }
}
