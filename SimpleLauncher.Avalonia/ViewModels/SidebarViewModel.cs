using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SimpleLauncher.Avalonia.Services.SystemImageResolver;
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
/// Builds and updates the sidebar system list from system.xml data.
/// All configured systems are listed flat (no manufacturer headers), so a system
/// added through Easy/Expert Mode appears as soon as Populate is called again.
/// </summary>
public class SidebarViewModel
{
    /// <summary>
    /// The flat list of all systems for the sidebar (system.xml order).
    /// </summary>
    public ObservableCollection<SidebarSystemItem> Systems { get; } = [];

    /// <summary>
    /// Builds the system items from system.xml data. Idempotent — clears and refills the list.
    /// When an image resolver is supplied, icons are resolved with annotation-stripped
    /// and fuzzy matching (WPF SystemImageResolverService parity); otherwise only the
    /// exact "images/systems/{name}.png" file is used.
    /// </summary>
    public void Populate(IEnumerable<SystemManagerConfig> systems, ISystemImageResolverService? imageResolver = null)
    {
        Systems.Clear();

        foreach (var system in systems)
        {
            var iconPath = imageResolver is not null
                ? imageResolver.ResolveSystemIconAsync(system.SystemName).GetAwaiter().GetResult()
                : ExactPngIcon(system.SystemName);

            Systems.Add(new SidebarSystemItem
            {
                SystemName = system.SystemName,
                IconPath = iconPath
            });
        }
    }

    private static string? ExactPngIcon(string systemName)
    {
        var iconPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "images", "systems", systemName + ".png");
        return File.Exists(iconPath) ? iconPath : null;
    }

    /// <summary>
    /// Updates the count badges for all systems from the live counts dictionary.
    /// </summary>
    public void RefreshCounts(IReadOnlyDictionary<string, int> counts)
    {
        foreach (var item in Systems)
        {
            item.Count = counts.GetValueOrDefault(item.SystemName, 0);
        }
    }
}
