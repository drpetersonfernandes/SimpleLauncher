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
        var sizeMb = bytes / (1024.0 * 1024.0);
        return $"{sizeMb:F2} MB";
    }
}