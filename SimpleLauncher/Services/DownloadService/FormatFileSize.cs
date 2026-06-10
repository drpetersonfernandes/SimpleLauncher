using System.Globalization;

namespace SimpleLauncher.Services.DownloadService;

public static class FormatFileSize
{
    /// <summary>
    /// Formats a byte size into MB.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>A formatted string representation of the size in MB.</returns>
    public static string FormatToMb(long bytes)
    {
        var sizeMb = bytes / (1024.0 * 1024.0);
        return string.Format(CultureInfo.InvariantCulture, "{0:F2} MB", sizeMb);
    }

    /// <summary>
    /// Formats a byte size into a human-readable format.
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>A formatted string representation of the size.</returns>
    public static string FormatToHumanReadable(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        var counter = 0;
        double size = bytes;

        while (size > 1024 && counter < suffixes.Length - 1)
        {
            size /= 1024;
            counter++;
        }

        return string.Format(CultureInfo.InvariantCulture, "{0:F2} {1}", size, suffixes[counter]);
    }
}
