using SimpleLauncher.Core.Services.GameItemFactory;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameButtonViewModelTests
{
    [Fact]
    public void DefaultIsFavoriteIsFalse()
    {
        var vm = new GameButtonViewModel();
        Assert.False(vm.IsFavorite);
    }

    [Fact]
    public void DefaultHasAchievementsIsFalse()
    {
        var vm = new GameButtonViewModel();
        Assert.False(vm.HasAchievements);
    }

    [Fact]
    public void IsFavoriteCanBeSetToTrue()
    {
        var vm = new GameButtonViewModel { IsFavorite = true };
        Assert.True(vm.IsFavorite);
    }

    [Fact]
    public void HasAchievementsCanBeSetToTrue()
    {
        var vm = new GameButtonViewModel { HasAchievements = true };
        Assert.True(vm.HasAchievements);
    }

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

    [Fact]
    public void IsFavoriteSameValueDoesNotRaisePropertyChanged()
    {
        var vm = new GameButtonViewModel { IsFavorite = true };
        var raised = false;
        vm.PropertyChanged += (_, _) => { raised = true; };

        vm.IsFavorite = true;
        Assert.False(raised);
    }

    [Fact]
    public void HasAchievementsSameValueDoesNotRaisePropertyChanged()
    {
        var vm = new GameButtonViewModel { HasAchievements = true };
        var raised = false;
        vm.PropertyChanged += (_, _) => { raised = true; };

        vm.HasAchievements = true;
        Assert.False(raised);
    }

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
