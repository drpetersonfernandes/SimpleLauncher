using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Tests for the FlashOverlayWindow ViewModel (Phase 4.1 port).
/// </summary>
public class FlashOverlayViewModelTests
{
    [Fact]
    public void Opacity_DefaultsToZero()
    {
        var vm = new FlashOverlayViewModel();
        Assert.Equal(0, vm.Opacity);
    }

    [Fact]
    public void Opacity_SetAndRaisesPropertyChanged()
    {
        var vm = new FlashOverlayViewModel();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.Opacity = 0.85;

        Assert.Equal(0.85, vm.Opacity);
        Assert.Contains(nameof(FlashOverlayViewModel.Opacity), changed);
    }

    [Fact]
    public void OnAnimationCompleted_RaisesCloseRequested()
    {
        var vm = new FlashOverlayViewModel();
        var raised = false;
        vm.CloseRequested += (_, _) => { raised = true; };

        vm.OnAnimationCompleted();

        Assert.True(raised);
    }

    [Fact]
    public void OnAnimationCompleted_WithoutSubscriber_DoesNotThrow()
    {
        var vm = new FlashOverlayViewModel();
        vm.OnAnimationCompleted();
    }
}