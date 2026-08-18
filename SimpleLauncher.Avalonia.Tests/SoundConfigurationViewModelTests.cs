using Moq;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the SoundConfigurationWindow ViewModel (Phase 4.1 port). Audio playback
/// is disabled (EnableNotificationSound = false) so no device is touched.
/// </summary>
public class SoundConfigurationViewModelTests
{
    private static SoundConfigurationViewModel CreateVm(out Mock<IMessageBoxLibraryService> messageBox, out SettingsManagerService settings)
    {
        messageBox = TestDependencies.MessageBox();
        settings = TestDependencies.Settings(messageBox: messageBox);
        settings.EnableNotificationSound = false;
        var playSound = TestDependencies.PlaySound(settings);
        var vm = new SoundConfigurationViewModel(settings, playSound, TestDependencies.Logger().Object, messageBox.Object, TestDependencies.ResourceProvider().Object);
        return vm;
    }

    [Fact]
    public void Ctor_LoadsSettingsAndSyncsControls()
    {
        var messageBox = TestDependencies.MessageBox();
        var settings = TestDependencies.Settings(messageBox: messageBox);
        settings.EnableNotificationSound = true;
        settings.CustomNotificationSoundFile = "click.mp3";

        var vm = new SoundConfigurationViewModel(settings, TestDependencies.PlaySound(settings),
            TestDependencies.Logger().Object, messageBox.Object, TestDependencies.ResourceProvider().Object);

        Assert.True(vm.EnableNotificationSound);
        Assert.Equal("click.mp3", vm.NotificationSoundFile);
        Assert.True(vm.IsSoundControlsEnabled);
    }

    [Fact]
    public void TogglingEnableSound_SyncsSoundControlsEnabled()
    {
        var vm = CreateVm(out _, out _);
        vm.EnableNotificationSound = true;
        Assert.True(vm.IsSoundControlsEnabled);

        vm.EnableNotificationSound = false;
        Assert.False(vm.IsSoundControlsEnabled);
    }

    [Fact]
    public async Task PlayCurrentSound_Disabled_ShowsNotificationDisabledMessage()
    {
        var vm = CreateVm(out var messageBox, out _);

        await vm.PlayCurrentSoundCommand.ExecuteAsync(null);

        messageBox.Verify(m => m.NotificationSoundIsDisableMessageBoxAsync(), Times.Once);
        messageBox.Verify(m => m.NoSoundFileIsSelectedMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task PlayCurrentSound_EnabledWithoutFile_ShowsNoSoundSelectedMessage()
    {
        var vm = CreateVm(out var messageBox, out _);
        vm.EnableNotificationSound = true;
        vm.NotificationSoundFile = "";

        await vm.PlayCurrentSoundCommand.ExecuteAsync(null);

        messageBox.Verify(m => m.NoSoundFileIsSelectedMessageBoxAsync(), Times.Once);
        messageBox.Verify(m => m.NotificationSoundIsDisableMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public void ResetToDefault_EnablesAndRestoresDefaultSound()
    {
        var vm = CreateVm(out _, out _);
        vm.EnableNotificationSound = false;
        vm.NotificationSoundFile = "";

        vm.ResetToDefaultCommand.Execute(null);

        Assert.True(vm.EnableNotificationSound);
        Assert.Equal("click.mp3", vm.NotificationSoundFile);
    }

    [Fact]
    public async Task Save_PersistsSettingsAndRaisesSaveCompleted()
    {
        var vm = CreateVm(out var messageBox, out var settings);
        vm.EnableNotificationSound = true;
        vm.NotificationSoundFile = "custom.mp3";
        var saved = false;
        vm.SaveCompleted += (_, _) => { saved = true; };

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.True(settings.EnableNotificationSound);
        Assert.Equal("custom.mp3", settings.CustomNotificationSoundFile);
        Assert.True(saved);
        messageBox.Verify(m => m.SettingsSavedSuccessfullyMessageBoxAsync(), Times.Once);
    }

    [Fact]
    public async Task ChooseSoundFile_NullPick_DoesNothing()
    {
        var vm = CreateVm(out _, out _);
        vm.NotificationSoundFile = "before.mp3";

        await vm.ChooseSoundFileCommand.ExecuteAsync(null);

        Assert.Equal("before.mp3", vm.NotificationSoundFile);
    }

    [Fact]
    public async Task ChooseSoundFile_CopiesFileIntoAudioFolder()
    {
        var vm = CreateVm(out _, out _);
        var sourceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-audio-src-file.mp3");
        File.WriteAllText(sourceFile, "mp3-bytes");

        var audioDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio");
        var copiedFile = Path.Combine(audioDir, "test-audio-src-file.mp3");
        File.Delete(copiedFile);
        try
        {
            vm.RequestSoundFilePath = () => Task.FromResult<string?>(sourceFile);

            await vm.ChooseSoundFileCommand.ExecuteAsync(null);

            Assert.Equal("test-audio-src-file.mp3", vm.NotificationSoundFile);
            Assert.True(File.Exists(copiedFile));
        }
        finally
        {
            TryDeleteFile(sourceFile);
            TryDeleteFile(copiedFile);
        }
    }

    private static void TryDeleteFile(string path)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                File.Delete(path);
                return;
            }
            catch (IOException) when (attempt < 9)
            {
                Thread.Sleep(50); // transient handle (AV scanner etc.)
            }
        }
    }

    [Fact]
    public void Cancel_RaisesCloseRequested()
    {
        var vm = CreateVm(out _, out _);
        var closed = false;
        vm.CloseRequested += (_, _) => { closed = true; };

        vm.CancelCommand.Execute(null);

        Assert.True(closed);
    }
}