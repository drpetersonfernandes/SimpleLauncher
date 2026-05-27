using System.IO;
using SimpleLauncher.Services.CleanAndDeleteFiles;

namespace SimpleLauncher.Services.CheckIfDirectoryIsWritable;

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
            App.LogErrorAsync(ex, "Failed to check if directory is writable.");

            return false;
        }
    }
}