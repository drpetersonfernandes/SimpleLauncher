namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Provides methods to locate specific game files (executables, disc images) within a directory.
/// </summary>
public interface IFileFinderService
{
    /// <summary>
    ///     Finds the default Xbox 360 XEX executable in the specified directory.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <returns>The path to the default XEX file, or null if not found.</returns>
    string? FindDefaultXex(string directory);

    /// <summary>
    ///     Finds the default Xbox XBE executable in the specified directory.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <returns>The path to the default XBE file, or null if not found.</returns>
    string? FindDefaultXbe(string directory);

    /// <summary>
    ///     Finds a CUE file in the specified directory.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <returns>The path to the CUE file, or null if not found.</returns>
    string? FindCueFile(string directory);

    /// <summary>
    ///     Finds a BIN file in the specified directory.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <returns>The path to the BIN file, or null if not found.</returns>
    string? FindBinFile(string directory);

    /// <summary>
    ///     Finds the EBOOT.BIN file in the specified directory (used for PS3 games).
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <returns>The path to the EBOOT.BIN file, or null if not found.</returns>
    string? FindEbootBin(string directory);

    /// <summary>
    ///     Finds an ISO image file in the specified directory.
    /// </summary>
    /// <param name="directory">The directory to search.</param>
    /// <returns>The path to the ISO file, or null if not found.</returns>
    string? FindImageIso(string directory);
}