using System.IO;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.CleanAndDeleteFiles;

namespace SimpleLauncher.Services.CheckIfDirectoryIsWritable;

public static class CheckIfDirectoryIsWritable
{
    public static bool IsWritableDirectory(string path, ILogErrors logErrors)
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
            logErrors.LogAndForget(ex, "Failed to check if directory is writable.");

            return false;
        }
    }
}