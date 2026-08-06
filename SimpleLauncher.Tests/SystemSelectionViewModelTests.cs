using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="SystemSelectionViewModel"/> system list initialization,
/// case-insensitive pre-selection, and dialog result events.
/// </summary>
public class SystemSelectionViewModelTests
{
    private static SystemSelectionViewModel CreateViewModel()
    {
        var matcher = new Mock<IRetroAchievementsSystemMatcher>();
        matcher.Setup(x => x.GetSupportedSystemNames()).Returns(new List<string>
        {
            "Nintendo 64",
            "Sony PlayStation",
            "Sega Genesis"
        });
        return new SystemSelectionViewModel(matcher.Object);
    }

    [Fact]
    public void Initialize_PopulatesSystemsFromMatcher()
    {
        var viewModel = CreateViewModel();

        viewModel.Initialize("");

        Assert.Equal(3, viewModel.Systems.Count);
        Assert.Equal("Nintendo 64", viewModel.Systems[0]);
    }

    [Fact]
    public void Initialize_CurrentGuess_PreselectsCaseInsensitiveMatch()
    {
        var viewModel = CreateViewModel();

        viewModel.Initialize("nintendo 64");

        Assert.Equal("Nintendo 64", viewModel.SelectedSystem);
    }

    [Fact]
    public void Initialize_CurrentGuess_WithNoMatch_LeavesSelectionNull()
    {
        var viewModel = CreateViewModel();

        viewModel.Initialize("Unknown Console");

        Assert.Null(viewModel.SelectedSystem);
    }

    [Fact]
    public void Confirm_WithSelection_RaisesDialogResultTrue()
    {
        var viewModel = CreateViewModel();
        viewModel.Initialize("Sony PlayStation");
        bool? result = null;
        var raised = 0;
        viewModel.DialogResultRequested += (_, e) =>
        {
            result = e.Value;
            raised++;
        };

        viewModel.ConfirmCommand.Execute(null);

        Assert.Equal(1, raised);
        Assert.True(result);
    }

    [Fact]
    public void Confirm_WithoutSelection_DoesNotRaiseDialogResult()
    {
        var viewModel = CreateViewModel();
        viewModel.Initialize("Unknown Console");
        var raised = 0;
        viewModel.DialogResultRequested += (_, _) => { raised++; };

        viewModel.ConfirmCommand.Execute(null);

        Assert.Equal(0, raised);
    }

    [Fact]
    public void Cancel_RaisesDialogResultFalse()
    {
        var viewModel = CreateViewModel();
        bool? result = null;
        viewModel.DialogResultRequested += (_, e) => { result = e.Value; };

        viewModel.CancelCommand.Execute(null);

        Assert.False(result);
    }

    [Fact]
    public void SelectedSystem_Setter_RaisesPropertyChanged()
    {
        var viewModel = CreateViewModel();
        var changedProperties = new List<string?>();
        viewModel.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        viewModel.SelectedSystem = "Sega Genesis";

        Assert.Contains("SelectedSystem", changedProperties, StringComparer.Ordinal);
    }
}
