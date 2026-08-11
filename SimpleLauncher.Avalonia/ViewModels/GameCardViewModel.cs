using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleLauncher.Avalonia.ViewModels;

/// <summary>
/// Represents a single game card in the grid.
/// Mapped from scan results + cache + favorites + play history.
/// </summary>
public partial class GameCardViewModel : ObservableObject
{
    [ObservableProperty] private string _displayTitle = "";

    [ObservableProperty] private string _coverPath = "";

    [ObservableProperty] private string _systemName = "";

    [ObservableProperty] private string _filePath = "";

    [ObservableProperty] private bool _isFavorite;

    [ObservableProperty] private int _playCount;

    [ObservableProperty] private int? _rating; // 0-5, null when no rating is known (hides the star row)

    [ObservableProperty] private string? _lastPlayed;

    [ObservableProperty] private bool _hasCover;

    [ObservableProperty] private string _placeholderColor = "#15FFFFFF";

    /// <summary>
    /// Whether this game's system is supported by RetroAchievements.
    /// </summary>
    [ObservableProperty] private bool _isRaSupported;

    /// <summary>
    /// Known RetroAchievements-supported systems (by system.xml name).
    /// </summary>
    private static readonly HashSet<string> RaSupportedSystems = new(StringComparer.OrdinalIgnoreCase)
    {
        "NES", "Nintendo NES", "Famicom", "Nintendo Famicom",
        "SNES", "Nintendo SNES", "Super Famicom", "Nintendo Super Famicom",
        "Nintendo 64", "N64",
        "Nintendo GameCube", "GameCube",
        "Wii", "Nintendo Wii", "Wii U", "Nintendo WiiU",
        "Game Boy", "Nintendo Game Boy",
        "Game Boy Color", "Nintendo Game Boy Color",
        "Game Boy Advance", "Nintendo Game Boy Advance", "GBA",
        "Nintendo DS", "DS", "Nintendo 3DS", "3DS",
        "Sega Genesis", "Sega Mega Drive", "Genesis",
        "Sega Master System", "Master System",
        "Sega Saturn", "Saturn",
        "Sega Dreamcast", "Dreamcast",
        "Sega Game Gear", "Game Gear",
        "Sega CD", "Sega Genesis CD",
        "Sega 32X", "Sega Genesis 32X",
        "PS1", "Sony PlayStation 1", "PlayStation",
        "PS2", "Sony PlayStation 2",
        "PSP", "Sony PSP",
        "Arcade", "MAME",
        "PC Engine", "NEC PC Engine", "TurboGrafx-16",
        "NEC PC Engine CD", "NEC PC-FX",
        "Neo Geo", "Neo Geo CD", "SNK Neo Geo CD",
        "Neo Geo Pocket", "SNK Neo Geo Pocket",
        "Atari 2600", "Atari 5200", "Atari 7800",
        "Atari Jaguar", "Atari Lynx",
        "Commodore 64", "ColecoVision",
        "MSX", "Microsoft MSX", "Microsoft MSX2",
        "WonderSwan", "Bandai WonderSwan", "Bandai WonderSwan Color",
        "Panasonic 3DO", "3DO",
        "Philips CD-i"
    };

    public static bool IsSystemRaSupported(string systemName)
    {
        return !string.IsNullOrEmpty(systemName) && RaSupportedSystems.Contains(systemName);
    }
}
