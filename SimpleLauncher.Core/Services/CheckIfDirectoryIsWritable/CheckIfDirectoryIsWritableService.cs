using SimpleLauncher.Core.Services.CleanAndDeleteFiles;

namespace SimpleLauncher.Core.Services.CheckIfDirectoryIsWritable;

/// <summary>
/// Provides methods to verify whether a directory allows file creation.
/// </summary>
public static class CheckIfDirectoryIsWritableService
{
    /// <summary>
    /// Determines whether the given directory is writable by creating and deleting a temporary test file.
    /// </summary>
    /// <param name="path">The path of the directory to test.</param>
    /// <param name="logErrors">The logger used to record failures.</param>
    /// <returns>True if the directory is writable, false otherwise.</returns>
    public static bool IsWritableDirectory(string path, ILogger logErrors)
    {
        try
        {
            if (!Directory.Exists(path))
                return false;

            var testFile = Path.Combine(path, Guid.NewGuid() + ".tmp");

            // Attempt to create and delete the file
            using (var fs = new FileStream(testFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.Close();
            }

            DeleteFiles.TryDeleteFile(testFile);

            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            logErrors.Error(ex, "Failed to check if directory is writable.");

            return false;
        }
    }
}