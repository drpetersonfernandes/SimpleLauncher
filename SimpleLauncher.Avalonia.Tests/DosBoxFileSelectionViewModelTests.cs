using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the DosBoxFileSelectionWindow ViewModel (Phase 4.1 port).
/// </summary>
public class DosBoxFileSelectionViewModelTests
{
    [Fact]
    public void Initialize_PopulatesFileItems()
    {
        var vm = new DosBoxFileSelectionViewModel();
        var baseDir = Path.Combine("C", "dosgames");
        var files = new[] { Path.Combine(baseDir, "game.bat"), Path.Combine(baseDir, "sub", "level.bat") };

        vm.Initialize(files, baseDir);

        Assert.Equal(2, vm.FileItems.Count);
        Assert.Equal("game.bat", vm.FileItems[0].DisplayName);
        Assert.Equal("", vm.FileItems[0].RelativePath); // same folder as base
        Assert.Equal("sub", vm.FileItems[1].RelativePath); // relative subfolder
        Assert.False(vm.IsLaunchEnabled);
    }

    [Fact]
    public void SelectingItem_EnablesLaunch()
    {
        var vm = new DosBoxFileSelectionViewModel();
        vm.Initialize([Path.Combine("C", "dosgames", "game.bat")], Path.Combine("C", "dosgames"));

        vm.SelectedItem = vm.FileItems[0];

        Assert.True(vm.IsLaunchEnabled);
    }

    [Fact]
    public void Launch_SetsSelectedFilePathAndRaisesDialogResult()
    {
        var vm = new DosBoxFileSelectionViewModel();
        vm.Initialize([Path.Combine("C", "dosgames", "game.bat")], Path.Combine("C", "dosgames"));
        vm.SelectedItem = vm.FileItems[0];

        bool? result = null;
        vm.DialogResultRequested += (_, e) => { result = e.Value; };
        vm.LaunchCommand.Execute(null);

        Assert.True(result);
        Assert.Equal(Path.Combine("C", "dosgames", "game.bat"), vm.SelectedFilePath);
    }

    [Fact]
    public void Launch_WithoutSelection_DoesNothing()
    {
        var vm = new DosBoxFileSelectionViewModel();
        vm.Initialize([Path.Combine("C", "dosgames", "game.bat")], Path.Combine("C", "dosgames"));

        var events = 0;
        vm.DialogResultRequested += (_, _) => { events++; };

        vm.LaunchCommand.Execute(null);

        Assert.Equal(0, events);
        Assert.Equal("", vm.SelectedFilePath);
    }

    [Fact]
    public void Cancel_RaisesDialogResultFalse()
    {
        var vm = new DosBoxFileSelectionViewModel();
        vm.Initialize([Path.Combine("C", "dosgames", "game.bat")], Path.Combine("C", "dosgames"));

        bool? result = null;
        vm.DialogResultRequested += (_, e) => { result = e.Value; };
        vm.CancelCommand.Execute(null);

        Assert.False(result);
    }

    [Fact]
    public void DoubleClick_BehavesLikeLaunch()
    {
        var vm = new DosBoxFileSelectionViewModel();
        vm.Initialize([Path.Combine("C", "dosgames", "game.bat")], Path.Combine("C", "dosgames"));
        vm.SelectedItem = vm.FileItems[0];

        bool? result = null;
        vm.DialogResultRequested += (_, e) => { result = e.Value; };
        vm.OnItemDoubleClicked();

        Assert.True(result);
    }
}