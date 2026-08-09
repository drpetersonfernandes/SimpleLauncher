using System.Text;

namespace SimpleLauncher.Core;

/// <summary>
/// Application-wide string constants to prevent typos and centralize magic values.
/// </summary>
internal static class AppConstants
{
    // Search modes passed to LoadGameFilesAsync as searchQuery
    /// <summary>
    /// Search mode constant for displaying favorites.
    /// </summary>
    internal const string Favorites = "FAVORITES";

    /// <summary>
    /// Search mode constant for displaying a random selection of games.
    /// </summary>
    internal const string RandomSelection = "RANDOM_SELECTION";

    /// <summary>
    /// Search mode constant for displaying RetroAchievements games.
    /// </summary>
    internal const string RetroAchievements = "RETRO_ACHIEVEMENTS";

    // ShowGames filter values
    /// <summary>
    /// Filter value to show all games regardless of cover image.
    /// </summary>
    internal const string ShowAll = "ShowAll";

    /// <summary>
    /// Filter value to show only games that have a cover image.
    /// </summary>
    internal const string ShowWithCover = "ShowWithCover";

    /// <summary>
    /// Filter value to show only games that do not have a cover image.
    /// </summary>
    internal const string ShowWithoutCover = "ShowWithoutCover";

    // View modes
    /// <summary>
    /// View mode constant for list view display.
    /// </summary>
    internal const string ListView = "ListView";

    /// <summary>
    /// View mode constant for grid view display.
    /// </summary>
    internal const string GridView = "GridView";

    // Filename display modes
    /// <summary>
    /// Filename display mode that shows the original filename.
    /// </summary>
    internal const string FilenameOriginal = "Original";

    /// <summary>
    /// Filename display mode that shows a cleaned-up filename.
    /// </summary>
    internal const string FilenameCleanUp = "CleanUp";

    /// <summary>
    /// Filename display mode that hides the filename.
    /// </summary>
    internal const string FilenameNoFilename = "NoFilename";

    // Font sizes
    /// <summary>
    /// Small font size setting.
    /// </summary>
    internal const string FontSizeSmall = "Small";

    /// <summary>
    /// Normal font size setting.
    /// </summary>
    internal const string FontSizeNormal = "Normal";

    /// <summary>
    /// Big font size setting.
    /// </summary>
    internal const string FontSizeBig = "Big";

    // Aspect ratios
    /// <summary>
    /// Square aspect ratio for button thumbnails.
    /// </summary>
    internal const string AspectSquare = "Square";

    /// <summary>
    /// Wider aspect ratio for button thumbnails.
    /// </summary>
    internal const string AspectWider = "Wider";

    /// <summary>
    /// Super wider aspect ratio for button thumbnails.
    /// </summary>
    internal const string AspectSuperWider = "SuperWider";

    /// <summary>
    /// Second super wider aspect ratio variant for button thumbnails.
    /// </summary>
    internal const string AspectSuperWider2 = "SuperWider2";

    /// <summary>
    /// Taller aspect ratio for button thumbnails.
    /// </summary>
    internal const string AspectTaller = "Taller";

    /// <summary>
    /// Super taller aspect ratio for button thumbnails.
    /// </summary>
    internal const string AspectSuperTaller = "SuperTaller";

    /// <summary>
    /// Second super taller aspect ratio variant for button thumbnails.
    /// </summary>
    internal const string AspectSuperTaller2 = "SuperTaller2";

    // MAME sort order values
    /// <summary>
    /// Sort order value that keeps games sorted by file name.
    /// </summary>
    internal const string MameSortOrderFileName = "FileName";

    /// <summary>
    /// Sort order value that sorts MAME games by machine description.
    /// </summary>
    internal const string MameSortOrderMachineDescription = "MachineDescription";

    // API key
    /// <summary>
    /// The runtime-decoded application API key. The encrypted value is read from appsettings.json
    /// and decoded once at application launch via <see cref="InitializeApiKey"/>.
    /// </summary>
    private static string? _apiKey;

    /// <summary>
    /// Reads the encrypted API key from configuration, decodes it, and caches the plaintext
    /// for the rest of the application lifetime. Called once at application launch.
    /// </summary>
    /// <param name="encodedApiKey">The double Base64-encoded API key from appsettings.json ("ApiKey" entry).</param>
    public static void InitializeApiKey(string? encodedApiKey)
    {
        _apiKey = DecodeApiKey(encodedApiKey);
    }

    /// <summary>
    /// Returns the application API key (decrypted at launch), or an empty string if it was
    /// not initialized or could not be decoded.
    /// </summary>
    public static string GetApiKey()
    {
        return _apiKey ?? "";
    }

    private static string DecodeApiKey(string? encodedApiKey)
    {
        if (string.IsNullOrEmpty(encodedApiKey)) return "";

        try
        {
            var decodedOnce = Encoding.UTF8.GetString(Convert.FromBase64String(encodedApiKey));
            return Encoding.UTF8.GetString(Convert.FromBase64String(decodedOnce));
        }
        catch (FormatException)
        {
            return "";
        }
    }
}