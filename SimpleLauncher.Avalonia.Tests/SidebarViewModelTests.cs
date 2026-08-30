using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Tests for the headerless flat sidebar: the sidebar groups all systems in one
///     flat list (no manufacturer headers), and running Populate again after a system
///     is added via Easy/Expert Mode makes it appear immediately.
/// </summary>
public class SidebarViewModelTests
{
    private static SystemManagerConfig System(string name)
    {
        return new SystemManagerConfig
        {
            SystemName = name,
            SystemFolders = [Path.Combine("roms", name)],
            SystemImageFolder = Path.Combine("images", name),
            FileFormatsToSearch = [".zip"],
            FileFormatsToLaunch = [".zip"]
        };
    }

    private static SidebarViewModel CreateSidebar(List<SystemManagerConfig> systems)
    {
        var sidebar = new SidebarViewModel();
        sidebar.Populate(systems);
        return sidebar;
    }

    [Fact]
    public void Populate_ListsAllSystemsFlatInConfigurationOrder()
    {
        var sidebar = CreateSidebar([System("NES"), System("Atari 2600"), System("Sega Genesis")]);

        Assert.Equal(3, sidebar.Systems.Count);
        Assert.Equal(["NES", "Atari 2600", "Sega Genesis"], sidebar.Systems.Select(static s => s.SystemName).ToList());
    }

    [Fact]
    public void Populate_RepopulateIncludesNewlyAddedSystem()
    {
        var sidebar = CreateSidebar([System("NES")]);

        // Simulate Easy/Expert Mode adding Atari 2600 → Populate runs again.
        sidebar.Populate([System("NES"), System("Atari 2600")]);

        Assert.Equal(2, sidebar.Systems.Count);
        Assert.Contains(sidebar.Systems, static s => string.Equals(s.SystemName, "Atari 2600", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void RefreshCounts_UpdatesBadgesForAllSystems()
    {
        var sidebar = CreateSidebar([System("NES"), System("Atari 2600")]);

        sidebar.RefreshCounts(new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["NES"] = 5,
            ["Atari 2600"] = 0
        });

        Assert.Equal(5, sidebar.Systems[0].Count);
        Assert.Equal("  5", sidebar.Systems[0].CountText);
        Assert.Equal(0, sidebar.Systems[1].Count);
        Assert.Equal("", sidebar.Systems[1].CountText);
    }
}