using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class CheckIfDirectoryIsWritable
{
    public static bool IsWritableDirectory(string path)
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
            _ = LogErrors.LogErrorAsync(ex, "Failed to check if directory is writable.");

            return false;
        }
    }
}