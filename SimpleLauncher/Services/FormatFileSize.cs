namespace SimpleLauncher.Services;

public static class FormatFileSize
{
    /// <summary>
    /// Formats a byte size into a human-readable format (KB, MB, GB, etc.).
    /// </summary>
    /// <param name="bytes">The size in bytes.</param>
    /// <returns>A formatted string representation of the size.</returns>
    public static string Format(long bytes)
    {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB"];
        var counter = 0;
        double size = bytes;

        while (size >= 1024 && counter < suffixes.Length - 1)
        {
            size /= 1024;
            counter++;
        }

        return $"{size:F2} {suffixes[counter]}";
    }
}