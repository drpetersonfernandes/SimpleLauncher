using SimpleLauncher.SharedModels;
using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameVerificationViewModelTests
{
    [Fact]
    public void Constructor_WithEmptyList_CreatesEmptyGameItems()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        List<SelectableGameItem> games = [];

        var viewModel = new GameVerificationViewModel(games);

        Assert.Empty(viewModel.GameItems);
        Assert.Empty(viewModel.ConfirmedGames);
    }

    [Fact]
    public void Constructor_WithGames_PopulatesGameItems()
    {
        var games = new List<SelectableGameItem>
        {
            new() { Name = "Game 1", AppId = "app1", IsSelected = false },
            new() { Name = "Game 2", AppId = "app2", IsSelected = true },
            new() { Name = "Game 3", AppId = "app3", IsSelected = false }
        };

        var viewModel = new GameVerificationViewModel(games);

        Assert.Equal(3, viewModel.GameItems.Count);
        Assert.Equal("Game 1", viewModel.GameItems[0].Name);
        Assert.Equal("Game 2", viewModel.GameItems[1].Name);
        Assert.Equal("Game 3", viewModel.GameItems[2].Name);
    }

    [Fact]
    public void Constructor_PreservesOriginalIsSelectedState()
    {
        var games = new List<SelectableGameItem>
        {
            new() { Name = "Game 1", IsSelected = false },
            new() { Name = "Game 2", IsSelected = true }
        };

        var viewModel = new GameVerificationViewModel(games);

        Assert.False(viewModel.GameItems[0].IsSelected);
        Assert.True(viewModel.GameItems[1].IsSelected);
    }

    [Fact]
    public void CanConfirm_ReturnsFalse_WhenNoGamesSelected()
    {
        var games = new List<SelectableGameItem>
        {
            new() { Name = "Game 1", IsSelected = false },
            new() { Name = "Game 2", IsSelected = false }
        };

        var viewModel = new GameVerificationViewModel(games);

        Assert.False(viewModel.CanConfirm);
    }

    [Fact]
    public void CanConfirm_ReturnsTrue_WhenAtLeastOneGameSelected()
    {
        var games = new List<SelectableGameItem>
        {
            new() { Name = "Game 1", IsSelected = false },
            new() { Name = "Game 2", IsSelected = true }
        };

        var viewModel = new GameVerificationViewModel(games);

        Assert.True(viewModel.CanConfirm);
    }

    [Fact]
    public void CanConfirm_ReturnsTrue_WhenAllGamesSelected()
    {
        var games = new List<SelectableGameItem>
        {
            new() { Name = "Game 1", IsSelected = true },
            new() { Name = "Game 2", IsSelected = true }
        };

        var viewModel = new GameVerificationViewModel(games);

        Assert.True(viewModel.CanConfirm);
    }

    [Fact]
    public void ConfirmCommand_RaisesConfirmRequestedEvent()
    {
        var games = new List<SelectableGameItem>
        {
            new() { Name = "Game 1", IsSelected = true, AppId = "app1" }
        };
        var viewModel = new GameVerificationViewModel(games);
        var eventRaised = false;
        viewModel.ConfirmRequested += () => { eventRaised = true; };

        viewModel.ConfirmCommand.Execute(null);

        Assert.True(eventRaised);
    }

    [Fact]
    public void ConfirmCommand_PopulatesConfirmedGames_WithSelectedItems()
    {
        var games = new List<SelectableGameItem>
        {
            new() { Name = "Game 1", AppId = "app1", IsSelected = true },
            new() { Name = "Game 2", AppId = "app2", IsSelected = false },
            new() { Name = "Game 3", AppId = "app3", IsSelected = true }
        };
        var viewModel = new GameVerificationViewModel(games);

        viewModel.ConfirmCommand.Execute(null);

        Assert.Equal(2, viewModel.ConfirmedGames.Count);
        Assert.Contains(viewModel.ConfirmedGames, static g => g.Name == "Game 1");
        Assert.Contains(viewModel.ConfirmedGames, static g => g.Name == "Game 3");
        Assert.DoesNotContain(viewModel.ConfirmedGames, static g => g.Name == "Game 2");
    }

    [Fact]
    public void ConfirmCommand_PreservesOriginalItemProperties()
    {
        var originalItem = new SelectableGameItem
        {
            Name = "Game 1",
            AppId = "app1",
            InstallLocation = "C:\\Games\\Game1",
            PackageFamilyName = "Publisher.Game1",
            LogoRelativePath = "Assets\\Logo.png",
            IsSelected = true
        };
        var games = new List<SelectableGameItem> { originalItem };
        var viewModel = new GameVerificationViewModel(games);

        viewModel.ConfirmCommand.Execute(null);

        var confirmed = viewModel.ConfirmedGames[0];
        Assert.Equal(originalItem.Name, confirmed.Name);
        Assert.Equal(originalItem.AppId, confirmed.AppId);
        Assert.Equal(originalItem.InstallLocation, confirmed.InstallLocation);
        Assert.Equal(originalItem.PackageFamilyName, confirmed.PackageFamilyName);
        Assert.Equal(originalItem.LogoRelativePath, confirmed.LogoRelativePath);
    }

    [Fact]
    public void CancelCommand_RaisesCancelRequestedEvent()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        List<SelectableGameItem> games = [];
        var viewModel = new GameVerificationViewModel(games);
        var eventRaised = false;
        viewModel.CancelRequested += () => { eventRaised = true; };

        viewModel.CancelCommand.Execute(null);

        Assert.True(eventRaised);
    }

    [Fact]
    public void SelectableGameItemViewModel_ExposesAllOriginalProperties()
    {
        var original = new SelectableGameItem
        {
            Name = "Test Game",
            AppId = "test123",
            InstallLocation = "C:\\Test",
            PackageFamilyName = "Test.Family",
            LogoRelativePath = "Assets\\Logo.png",
            IsSelected = true
        };

        var viewModel = new SelectableGameItemViewModel(original);

        Assert.Equal(original.Name, viewModel.Name);
        Assert.Equal(original.AppId, viewModel.AppId);
        Assert.Equal(original.InstallLocation, viewModel.InstallLocation);
        Assert.Equal(original.PackageFamilyName, viewModel.PackageFamilyName);
        Assert.Equal(original.LogoRelativePath, viewModel.LogoRelativePath);
        Assert.Equal(original.IsSelected, viewModel.IsSelected);
        Assert.Equal(original, viewModel.OriginalItem);
    }

    [Fact]
    public void SelectableGameItemViewModel_IsSelected_ChangesUpdatesOriginal()
    {
        var original = new SelectableGameItem { Name = "Test", IsSelected = false };
        var viewModel = new SelectableGameItemViewModel(original)
        {
            IsSelected = true
        };

        Assert.True(original.IsSelected);
        Assert.True(viewModel.IsSelected);
    }

    [Fact]
    public void SelectableGameItemViewModel_IsSelected_RaisesPropertyChanged()
    {
        var original = new SelectableGameItem { Name = "Test", IsSelected = false };
        var viewModel = new SelectableGameItemViewModel(original);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectableGameItemViewModel.IsSelected))
            {
                propertyChangedRaised = true;
            }
        };

        viewModel.IsSelected = true;

        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public void SelectableGameItemViewModel_Constructor_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(static () => new SelectableGameItemViewModel(null!));
    }

    [Fact]
    public void ConfirmCommand_DoesNotModifyConfirmedGames_UntilExecuted()
    {
        var games = new List<SelectableGameItem>
        {
            new() { Name = "Game 1", IsSelected = true }
        };
        var viewModel = new GameVerificationViewModel(games);

        Assert.Empty(viewModel.ConfirmedGames);

        viewModel.ConfirmCommand.Execute(null);

        Assert.Single(viewModel.ConfirmedGames);
    }
}
