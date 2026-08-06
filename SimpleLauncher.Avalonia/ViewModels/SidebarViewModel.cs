using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Avalonia.ViewModels;

/// <summary>
/// A single system entry in the sidebar (icon + name + live count badge).
/// </summary>
public partial class SidebarSystemItem : ObservableObject
{
    /// <summary>System name — used as the navigation tag.</summary>
    public string SystemName { get; init; } = "";

    /// <summary>Resolved path to the system icon PNG, or null when no icon exists.</summary>
    public string? IconPath { get; init; }

    /// <summary>Whether a system icon file exists (drives image vs glyph fallback).</summary>
    public bool HasIcon => IconPath is not null;

    [ObservableProperty] private int _count;

    /// <summary>Formatted count badge text (empty when zero, like the original UI).</summary>
    public string CountText => Count > 0 ? $"  {Count}" : "";

    partial void OnCountChanged(int value)
    {
        _ = value; // Parameter is required by the generated partial method signature.
        OnPropertyChanged(nameof(CountText));
    }
}

/// <summary>
/// A manufacturer group (ARCADE, NINTENDO, SEGA, ...) holding its systems.
/// </summary>
public class SidebarManufacturerGroup
{
    public string Header { get; init; } = "";

    public ObservableCollection<SidebarSystemItem> Systems { get; } = [];
}

/// <summary>
/// Builds and updates the sidebar system groups from system.xml data.
/// Extracted from MainWindow.xaml.cs so the view logic (manufacturer mapping,
/// icon resolution, count badges) lives outside the code-behind.
/// </summary>
public class SidebarViewModel
{
    /// <summary>
    /// Manufacturer groups in sidebar display order.
    /// Index order must match the XAML bindings (Groups[0] = ARCADE ... Groups[6] = OTHER).
    /// </summary>
    public ObservableCollection<SidebarManufacturerGroup> Groups { get; } =
    [
        new() { Header = "ARCADE" },
        new() { Header = "NINTENDO" },
        new() { Header = "SEGA" },
        new() { Header = "SONY" },
        new() { Header = "NEC" },
        new() { Header = "SNK" },
        new() { Header = "OTHER" }
    ];

    /// <summary>
    /// Resolves the manufacturer group index for a system name (unknown → OTHER).
    /// </summary>
    private static readonly Dictionary<string, int> ManufacturerGroupIndex = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Atari 2600"] = 6, ["Atari 5200"] = 6, ["Atari 7800"] = 6,
        ["Atari Jaguar"] = 6, ["Atari Jaguar CD"] = 6, ["Atari Lynx"] = 6,
        ["Atari ST"] = 6, ["Atari 8-Bit"] = 6,
        ["NES"] = 1, ["Nintendo NES"] = 1, ["Famicom"] = 1,
        ["SNES"] = 1, ["Nintendo SNES"] = 1, ["Super Famicom"] = 1,
        ["Nintendo 64"] = 1, ["Nintendo 64DD"] = 1,
        ["Nintendo GameCube"] = 1, ["Wii"] = 1, ["Nintendo Wii"] = 1,
        ["Wii U"] = 1, ["Nintendo WiiU"] = 1,
        ["Nintendo Switch"] = 1,
        ["Game Boy"] = 1, ["Nintendo Game Boy"] = 1,
        ["Game Boy Color"] = 1, ["Nintendo Game Boy Color"] = 1,
        ["Game Boy Advance"] = 1, ["Nintendo Game Boy Advance"] = 1,
        ["Nintendo DS"] = 1, ["Nintendo 3DS"] = 1,
        ["Virtual Boy"] = 1,
        ["Sega Genesis"] = 2, ["Sega Mega Drive"] = 2,
        ["Sega Master System"] = 2, ["Sega Saturn"] = 2,
        ["Sega Dreamcast"] = 2, ["Sega Game Gear"] = 2,
        ["Sega CD"] = 2, ["Sega 32X"] = 2, ["Sega Genesis CD"] = 2,
        ["Sega Genesis 32X"] = 2, ["Sega SG-1000"] = 2,
        ["PS1"] = 3, ["Sony PlayStation 1"] = 3,
        ["PS2"] = 3, ["Sony PlayStation 2"] = 3,
        ["PS3"] = 3, ["Sony PlayStation 3"] = 3,
        ["PSP"] = 3, ["Sony PSP"] = 3,
        ["PS Vita"] = 3,
        ["PC Engine"] = 4, ["NEC PC Engine"] = 4,
        ["NEC PC Engine CD"] = 4, ["TurboGrafx-16"] = 4,
        ["NEC PC-FX"] = 4, ["NEC SuperGrafx"] = 4,
        ["Neo Geo"] = 5, ["Neo Geo CD"] = 5,
        ["SNK Neo Geo CD"] = 5, ["Neo Geo Pocket"] = 5,
        ["SNK Neo Geo Pocket"] = 5, ["Neo Geo Pocket Color"] = 5,
        ["SNK Neo Geo Pocket Color"] = 5,
        ["Arcade"] = 0, ["MAME"] = 0
    };

    /// <summary>
    /// Builds the system items from system.xml data. Idempotent — clears and refills the groups.
    /// </summary>
    public void Populate(IEnumerable<SystemManagerConfig> systems)
    {
        foreach (var group in Groups)
        {
            group.Systems.Clear();
        }

        foreach (var system in systems)
        {
            var groupIndex = ManufacturerGroupIndex.GetValueOrDefault(system.SystemName, 6);
            var iconPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "images", "systems", system.SystemName + ".png");

            Groups[groupIndex].Systems.Add(new SidebarSystemItem
            {
                SystemName = system.SystemName,
                IconPath = File.Exists(iconPath) ? iconPath : null
            });
        }
    }

    /// <summary>
    /// Updates the count badges for all systems from the live counts dictionary.
    /// </summary>
    public void RefreshCounts(IReadOnlyDictionary<string, int> counts)
    {
        foreach (var group in Groups)
        {
            foreach (var item in group.Systems)
            {
                item.Count = counts.GetValueOrDefault(item.SystemName, 0);
            }
        }
    }
}
