namespace SimpleLauncher.Services.GameLauncher.MountFiles;

/// <summary>
/// Locates the image.iso file in a given directory.
/// </summary>
public static class FindImageIso
{
    /// <summary>
    /// Searches for image.iso in the specified root directory.
    /// </summary>
    public static string? Find(string rootPath, ILogger logErrors)
    {
        try
        {
            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
            {
                return null;
            }

            var defaultImagePath = Path.Combine(rootPath, "image.iso");
            if (File.Exists(defaultImagePath))
            {
                return defaultImagePath;
            }

            return null;
        }
        catch (Exception ex)
        {
            logErrors.Error(ex, $"Error finding image.iso in path: {rootPath}");
            return null;
        }
    }
}
