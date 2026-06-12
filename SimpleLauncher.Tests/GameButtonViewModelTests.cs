using SimpleLauncher.Services.GameItemFactory;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="GameButtonViewModel"/> class.
/// </summary>
public class GameButtonViewModelTests
{
    /// <summary>
    /// Verifies that the default value of IsFavorite is false.
    /// </summary>
    [Fact]
    public void DefaultIsFavoriteIsFalse()
    {
        var vm = new GameButtonViewModel();
        Assert.False(vm.IsFavorite);
    }

    /// <summary>
    /// Verifies that the default value of HasAchievements is false.
    /// </summary>
    [Fact]
    public void DefaultHasAchievementsIsFalse()
    {
        var vm = new GameButtonViewModel();
        Assert.False(vm.HasAchievements);
    }

    /// <summary>
    /// Verifies that IsFavorite can be set to true.
    /// </summary>
    [Fact]
    public void IsFavoriteCanBeSetToTrue()
    {
        var vm = new GameButtonViewModel { IsFavorite = true };
        Assert.True(vm.IsFavorite);
    }

    /// <summary>
    /// Verifies that HasAchievements can be set to true.
    /// </summary>
    [Fact]
    public void HasAchievementsCanBeSetToTrue()
    {
        var vm = new GameButtonViewModel { HasAchievements = true };
        Assert.True(vm.HasAchievements);
    }

    /// <summary>
    /// Verifies that setting IsFavorite raises PropertyChanged.
    /// </summary>
    [Fact]
    public void IsFavoriteRaisesPropertyChanged()
    {
        var vm = new GameButtonViewModel();
        var raised = false;
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameButtonViewModel.IsFavorite))
            {
                raised = true;
            }
        };

        vm.IsFavorite = true;
        Assert.True(raised);
    }

    /// <summary>
    /// Verifies that setting HasAchievements raises PropertyChanged.
    /// </summary>
    [Fact]
    public void HasAchievementsRaisesPropertyChanged()
    {
        var vm = new GameButtonViewModel();
        var raised = false;
        vm.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(GameButtonViewModel.HasAchievements))
            {
                raised = true;
            }
        };

        vm.HasAchievements = true;
        Assert.True(raised);
    }

    /// <summary>
    /// Verifies that setting IsFavorite to the same value does not raise PropertyChanged.
    /// </summary>
    [Fact]
    public void IsFavoriteSameValueDoesNotRaisePropertyChanged()
    {
        var vm = new GameButtonViewModel { IsFavorite = true };
        var raised = false;
        vm.PropertyChanged += (_, _) => { raised = true; };

        vm.IsFavorite = true;
        Assert.False(raised);
    }

    /// <summary>
    /// Verifies that setting HasAchievements to the same value does not raise PropertyChanged.
    /// </summary>
    [Fact]
    public void HasAchievementsSameValueDoesNotRaisePropertyChanged()
    {
        var vm = new GameButtonViewModel { HasAchievements = true };
        var raised = false;
        vm.PropertyChanged += (_, _) => { raised = true; };

        vm.HasAchievements = true;
        Assert.False(raised);
    }

    /// <summary>
    /// Verifies that IsFavorite can be toggled between true and false.
    /// </summary>
    [Fact]
    public void IsFavoriteToggleBackAndForth()
    {
        var vm = new GameButtonViewModel
        {
            IsFavorite = true
        };
        Assert.True(vm.IsFavorite);
        vm.IsFavorite = false;
        Assert.False(vm.IsFavorite);
    }

    /// <summary>
    /// Verifies that multiple PropertyChanged subscriptions are all invoked.
    /// </summary>
    [Fact]
    public void MultiplePropertyChangedSubscriptions()
    {
        var vm = new GameButtonViewModel();
        var count = 0;
        vm.PropertyChanged += (_, _) => { count++; };
        vm.PropertyChanged += (_, _) => { count++; };

        vm.IsFavorite = true;
        Assert.Equal(2, count);
    }
}
