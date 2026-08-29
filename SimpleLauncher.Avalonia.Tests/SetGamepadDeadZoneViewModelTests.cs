using Moq;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Tests for the SetGamepadDeadZoneWindow ViewModel (Phase 4.1 port).
/// </summary>
public class SetGamepadDeadZoneViewModelTests
{
    private static SetGamepadDeadZoneViewModel CreateVm(out Mock<IMessageBoxLibraryService> messageBox,
        out SettingsManagerService settings)
    {
        messageBox = TestDependencies.MessageBox();
        settings = TestDependencies.Settings(messageBox: messageBox);
        var vm = new SetGamepadDeadZoneViewModel(settings, messageBox.Object,
            TestDependencies.ResourceProvider().Object, TestDependencies.Logger().Object);
        return vm;
    }

    [Fact]
    public void Ctor_LoadsDeadZonesFromSettings()
    {
        var messageBox = TestDependencies.MessageBox();
        var settings = TestDependencies.Settings(messageBox: messageBox);
        settings.DeadZoneX = 0.11f;
        settings.DeadZoneY = 0.07f;

        var vm = new SetGamepadDeadZoneViewModel(settings, messageBox.Object,
            TestDependencies.ResourceProvider().Object, TestDependencies.Logger().Object);

        Assert.Equal(0.11f, vm.DeadZoneX);
        Assert.Equal(0.07f, vm.DeadZoneY);
        Assert.Equal("0.11", vm.DeadZoneXText);
        Assert.Equal("0.07", vm.DeadZoneYText);
    }

    [Fact]
    public async Task Save_PersistsValuesAndRaisesSaveCompleted()
    {
        var vm = CreateVm(out var messageBox, out var settings);
        vm.DeadZoneX = 0.2;
        vm.DeadZoneY = 0.1;
        var saved = false;
        vm.SaveCompleted += (_, _) => { saved = true; };

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(0.2f, settings.DeadZoneX);
        Assert.Equal(0.1f, settings.DeadZoneY);
        Assert.True(saved);
        messageBox.Verify(m => m.DeadZonesSavedMessageBoxAsync(), Times.Once);
    }

    [Fact]
    public async Task Revert_ResetsToDefaultsAndCloses()
    {
        var vm = CreateVm(out var messageBox, out var settings);
        settings.DeadZoneX = 0.9f;
        settings.DeadZoneY = 0.9f;
        vm.DeadZoneX = 0.9;
        vm.DeadZoneY = 0.9;
        var closed = false;
        vm.CloseRequested += (_, _) => { closed = true; };

        await vm.RevertCommand.ExecuteAsync(null);

        Assert.Equal(SettingsManagerService.DefaultDeadZoneX, vm.DeadZoneX);
        Assert.Equal(SettingsManagerService.DefaultDeadZoneY, vm.DeadZoneY);
        Assert.Equal(SettingsManagerService.DefaultDeadZoneX, settings.DeadZoneX);
        Assert.Equal(SettingsManagerService.DefaultDeadZoneY, settings.DeadZoneY);
        messageBox.Verify(m => m.DeadZonesRevertedMessageBoxAsync(), Times.Once);
        Assert.True(closed);
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