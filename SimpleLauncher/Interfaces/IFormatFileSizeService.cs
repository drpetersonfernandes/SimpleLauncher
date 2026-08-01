namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides methods to format byte values into human-readable file size strings.
/// </summary>
public interface IFormatFileSizeService
{
    /// <summary>
    /// Formats a byte value as a megabyte string.
    /// </summary>
    /// <param name="bytes">The number of bytes.</param>
    /// <returns>A string representing the size in megabytes.</returns>
    string FormatToMb(long bytes);

    /// <summary>
    /// Formats a byte value into a human-readable string using the most appropriate unit.
    /// </summary>
    /// <param name="bytes">The number of bytes.</param>
    /// <returns>A human-readable string such as "1.23 MB".</returns>
    string FormatToHumanReadable(long bytes);
}
