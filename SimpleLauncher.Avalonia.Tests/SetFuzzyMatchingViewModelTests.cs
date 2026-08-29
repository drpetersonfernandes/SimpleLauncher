using Moq;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Tests for the SetFuzzyMatchingWindow ViewModel (Phase 4.1 port).
/// </summary>
public class SetFuzzyMatchingViewModelTests
{
    private static SetFuzzyMatchingViewModel CreateVm(out Mock<IMessageBoxLibraryService> messageBox,
        out SettingsManagerService settings)
    {
        messageBox = TestDependencies.MessageBox();
        settings = TestDependencies.Settings(messageBox: messageBox);
        var vm = new SetFuzzyMatchingViewModel(settings, TestDependencies.Logger().Object, messageBox.Object,
            TestDependencies.ResourceProvider().Object);
        return vm;
    }

    [Fact]
    public void Ctor_ClampsThresholdToSliderRange()
    {
        var messageBox = TestDependencies.MessageBox();
        var settings = TestDependencies.Settings(messageBox: messageBox);
        settings.FuzzyMatchingThreshold = 0.99;

        var high = new SetFuzzyMatchingViewModel(settings, TestDependencies.Logger().Object, messageBox.Object,
            TestDependencies.ResourceProvider().Object);
        Assert.Equal(SetFuzzyMatchingViewModel.MaximumThreshold, high.ThresholdValue);

        settings.FuzzyMatchingThreshold = 0.5;
        var low = new SetFuzzyMatchingViewModel(settings, TestDependencies.Logger().Object, messageBox.Object,
            TestDependencies.ResourceProvider().Object);
        Assert.Equal(SetFuzzyMatchingViewModel.MinimumThreshold, low.ThresholdValue);
    }

    [Fact]
    public void ThresholdValue_UpdateRaisesThresholdPercentage()
    {
        var vm = CreateVm(out _, out _);
        Assert.Equal("80 %", vm.ThresholdPercentage); // default 0.80 → P0

        vm.ThresholdValue = 0.9;

        Assert.Equal("90 %", vm.ThresholdPercentage);
    }

    [Fact]
    public void CanSave_IsTrue()
    {
        var vm = CreateVm(out _, out _);
        Assert.True(vm.CanSave);
        Assert.True(vm.SaveCommand.CanExecute(null));
    }

    [Fact]
    public async Task Save_ClampsAndPersistsThreshold()
    {
        var vm = CreateVm(out var messageBox, out var settings);
        vm.ThresholdValue = 1.5; // way above max
        var saved = false;
        vm.SaveCompleted += (_, _) => { saved = true; };

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(SetFuzzyMatchingViewModel.MaximumThreshold, settings.FuzzyMatchingThreshold);
        Assert.True(saved);
        messageBox.Verify(m => m.FuzzyMatchingErrorFailToSetThresholdMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task Save_PersistsInRangeThreshold()
    {
        var vm = CreateVm(out var messageBox, out var settings);
        vm.ThresholdValue = 0.85;
        var saved = false;
        vm.SaveCompleted += (_, _) => { saved = true; };

        await vm.SaveCommand.ExecuteAsync(null);

        Assert.Equal(0.85, settings.FuzzyMatchingThreshold);
        Assert.True(saved);
        messageBox.Verify(m => m.FuzzyMatchingErrorFailToSetThresholdMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public void Cancel_RaisesCancelRequested()
    {
        var vm = CreateVm(out _, out _);
        var cancelled = false;
        vm.CancelRequested += (_, _) => { cancelled = true; };

        vm.CancelCommand.Execute(null);

        Assert.True(cancelled);
    }
}