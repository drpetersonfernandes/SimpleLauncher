using Moq;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using CoreMessageBoxResult = SimpleLauncher.Core.Models.MessageBoxResult;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the GlobalStatsWindow ViewModel (Phase 4.1 port). UI-thread work is
/// pumped by the dedicated headless UI thread, and file listing is mocked.
/// </summary>
public class GlobalStatsViewModelTests
{
    private static GlobalStatsViewModel CreateVm(
        out Mock<IMessageBoxLibraryService> messageBox,
        out Mock<IGetListOfFilesService> getFiles,
        Mock<IFilePickerService>? filePicker = null)
    {
        HeadlessAvalonia.EnsureInitialized();
        messageBox = TestDependencies.MessageBox();
        getFiles = new Mock<IGetListOfFilesService>();
        var config = TestEnvironment.ConfigurationFromJson("""{"ImageExtensions": [".png", ".jpg"]}""");
        return new GlobalStatsViewModel(
            config,
            TestDependencies.Logger().Object,
            getFiles.Object,
            messageBox.Object,
            TestDependencies.ResourceProvider().Object,
            (filePicker ?? new Mock<IFilePickerService>()).Object);
    }

    private static SystemManagerConfig System(string name, string folder, params string[] files)
    {
        return new SystemManagerConfig
        {
            SystemName = name,
            SystemFolders = [folder],
            FileFormatsToSearch = [".nes"],
            FileFormatsToLaunch = [".nes"],
            SystemImageFolder = "",
            Emulators = new List<Emulator> { new() { EmulatorName = "Mesen" } },
            DisableRecursiveSearch = false,
            GroupByFolder = false
        };
    }

    [Fact]
    public async Task Start_NoSystems_ShowsInfoMessage()
    {
        var vm = CreateVm(out var messageBox, out _);
        messageBox.Setup(m => m.WouldYouLikeToSaveAReportMessageBoxAsync()).ReturnsAsync(CoreMessageBoxResult.No);

        await vm.StartCommand.ExecuteAsync(null);

        Assert.Contains("No systems are configured", vm.InfoText);
        Assert.False(vm.IsProcessing);
        Assert.False(vm.IsBusyOverlayVisible);
        Assert.True(vm.IsStartButtonVisible);
        Assert.False(vm.IsSaveButtonVisible);
    }

    [Fact]
    public async Task Start_WithSystems_ComputesAndDisplaysStats()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "sl-globalstats-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        try
        {
            var fileA = Path.Combine(tempDir, "game-a.nes");
            var fileB = Path.Combine(tempDir, "game-b.nes");
            File.WriteAllText(fileA, new string('a', 100));
            File.WriteAllText(fileB, new string('b', 200));

            var vm = CreateVm(out var messageBox, out var getFiles);
            messageBox.Setup(m => m.WouldYouLikeToSaveAReportMessageBoxAsync()).ReturnsAsync(CoreMessageBoxResult.No);
            getFiles.Setup(f => f.GetFilesAsync(It.IsAny<string>(), It.IsAny<IList<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { fileA, fileB });

            vm.Initialize(new List<SystemManagerConfig> { System("NES", tempDir, fileA, fileB) });

            await vm.StartCommand.ExecuteAsync(null);

            Assert.False(vm.IsProcessing);
            Assert.False(vm.IsBusyOverlayVisible);
            Assert.True(vm.IsSaveButtonVisible);
            var stat = Assert.Single(vm.SystemStats);
            Assert.Equal("NES", stat.SystemName);
            Assert.Equal(2, stat.NumberOfFiles);
            Assert.Equal(300, stat.TotalDiskSize);
            Assert.Contains("Total Systems: 1", vm.InfoText);
            Assert.Contains("Total Games: 2", vm.InfoText);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task SaveReport_WithStats_WritesFileAndConfirms()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "sl-globalstats-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var reportPath = Path.Combine(tempDir, "report.txt");
        try
        {
            var fileA = Path.Combine(tempDir, "game-a.nes");
            File.WriteAllText(fileA, new string('a', 100));

            var filePicker = new Mock<IFilePickerService>();
            filePicker.Setup(f => f.SaveFileAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(reportPath);

            var vm = CreateVm(out var messageBox, out var getFiles, filePicker);
            messageBox.Setup(m => m.WouldYouLikeToSaveAReportMessageBoxAsync()).ReturnsAsync(CoreMessageBoxResult.No);
            getFiles.Setup(f => f.GetFilesAsync(It.IsAny<string>(), It.IsAny<IList<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string> { fileA });

            vm.Initialize(new List<SystemManagerConfig> { System("NES", tempDir, fileA) });
            await vm.StartCommand.ExecuteAsync(null);

            await vm.SaveReportCommand.ExecuteAsync(null);

            Assert.True(File.Exists(reportPath));
            var report = await File.ReadAllTextAsync(reportPath);
            Assert.Contains("NES", report);
            Assert.Contains("Total Systems: 1", report);
            messageBox.Verify(m => m.ReportSavedMessageBoxAsync(), Times.Once);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public async Task RequestClose_NotProcessing_ReturnsTrue()
    {
        var vm = CreateVm(out _, out _);
        Assert.True(await vm.RequestCloseAsync());
    }

    [Fact]
    public async Task RequestClose_DuringProcessing_UserCancels_KeepsWindowOpenAndCancels()
    {
        var gate = new TaskCompletionSource<IList<string>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var vm = CreateVm(out var messageBox, out var getFiles);
        messageBox.Setup(m => m.DoYouWantToCancelAndCloseMessageBoxAsync()).ReturnsAsync(CoreMessageBoxResult.Yes);
        getFiles.Setup(f => f.GetFilesAsync(It.IsAny<string>(), It.IsAny<IList<string>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(gate.Task);

        vm.Initialize(new List<SystemManagerConfig> { System("NES", "C:\\roms", "a.nes") });
        messageBox.Setup(m => m.WouldYouLikeToSaveAReportMessageBoxAsync()).ReturnsAsync(CoreMessageBoxResult.No);

        var closeRequested = false;
        vm.CloseRequested += (_, _) => { closeRequested = true; };

        var startTask = vm.StartCommand.ExecuteAsync(null);
        await HeadlessAvalonia.WaitUntilAsync(() => vm.IsProcessing);

        Assert.True(vm.CancelCommand.CanExecute(null)); // cancel enabled while processing
        Assert.False(vm.StartCommand.CanExecute(null)); // start disabled while processing

        var requestClose = await vm.RequestCloseAsync();

        Assert.False(requestClose); // window stays open, closes when processing ends
        await HeadlessAvalonia.WaitUntilAsync(() => vm.IsCancelOverlayVisible == false);

        gate.SetResult(new List<string> { "a.nes" });
        await startTask;

        await HeadlessAvalonia.WaitUntilAsync(() => closeRequested);
        Assert.False(vm.IsProcessing);
    }

    [Fact]
    public void Initialize_SetsInfoAndBusyTexts()
    {
        var vm = CreateVm(out _, out _);
        vm.Initialize(new List<SystemManagerConfig> { System("NES", "C:\\roms", "a.nes") });

        Assert.NotNull(vm.InfoText);
        Assert.NotNull(vm.BusyOverlayText);
    }
}