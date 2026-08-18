using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the WindowSelectionDialogWindow ViewModel (Phase 4.1 port).
/// </summary>
public class WindowSelectionDialogViewModelTests
{
    [Fact]
    public void Initialize_FiltersBlankTitles()
    {
        var vm = new WindowSelectionDialogViewModel();
        var handle = new IntPtr(1234);

        vm.Initialize([
            (handle, "Emulator Window"),
            (new IntPtr(2), "  "),
            (new IntPtr(3), ""),
            (new IntPtr(4), null!)
        ]);

        Assert.Single(vm.WindowItems);
        Assert.Equal("Emulator Window", vm.WindowItems[0].Title);
        Assert.Equal(handle, vm.WindowItems[0].Handle);
    }

    [Fact]
    public void Initialize_EmptyInput_YieldsEmptyList()
    {
        var vm = new WindowSelectionDialogViewModel();
        vm.Initialize([]);
        Assert.Empty(vm.WindowItems);
    }

    [Fact]
    public void SelectingItem_SetsHandleAndRaisesDialogResult()
    {
        var vm = new WindowSelectionDialogViewModel();
        var handle = new IntPtr(5678);
        vm.Initialize([(handle, "Window A"), (new IntPtr(1), "Window B")]);

        bool? result = null;
        vm.DialogResultRequested += (_, e) => { result = e.Value; };
        vm.SelectedItem = vm.WindowItems[0];

        Assert.True(result);
        Assert.Equal(handle, vm.SelectedWindowHandle);
    }

    [Fact]
    public void SelectingNull_DoesNotRaiseDialogResult()
    {
        var vm = new WindowSelectionDialogViewModel();
        vm.Initialize([(new IntPtr(1), "Window A")]);
        vm.SelectedItem = vm.WindowItems[0];

        var events = 0;
        vm.DialogResultRequested += (_, _) => { events++; };
        vm.SelectedItem = null;

        Assert.Equal(0, events);
        Assert.Equal(new IntPtr(1), vm.SelectedWindowHandle); // unchanged
    }
}