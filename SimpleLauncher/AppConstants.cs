namespace SimpleLauncher;

/// <summary>
/// Application-wide string constants to prevent typos and centralize magic values.
/// </summary>
internal static class AppConstants
{
    // Search modes passed to LoadGameFilesAsync as searchQuery
    internal const string Favorites = "FAVORITES";
    internal const string RandomSelection = "RANDOM_SELECTION";

    // ShowGames filter values
    internal const string ShowAll = "ShowAll";
    internal const string ShowWithCover = "ShowWithCover";
    internal const string ShowWithoutCover = "ShowWithoutCover";

    // View modes
    internal const string ListView = "ListView";
    internal const string GridView = "GridView";

    // Filename display modes
    internal const string FilenameOriginal = "Original";
    internal const string FilenameCleanUp = "CleanUp";
    internal const string FilenameNoFilename = "NoFilename";

    // Font sizes
    internal const string FontSizeSmall = "Small";
    internal const string FontSizeNormal = "Normal";
    internal const string FontSizeBig = "Big";

    // Aspect ratios
    internal const string AspectSquare = "Square";
    internal const string AspectWider = "Wider";
    internal const string AspectSuperWider = "SuperWider";
    internal const string AspectSuperWider2 = "SuperWider2";
    internal const string AspectTaller = "Taller";
    internal const string AspectSuperTaller = "SuperTaller";
    internal const string AspectSuperTaller2 = "SuperTaller2";
}
