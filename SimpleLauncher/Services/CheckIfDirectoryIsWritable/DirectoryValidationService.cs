using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CleanAndDeleteFiles;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.CheckIfDirectoryIsWritable;

public class DirectoryValidationService : IDirectoryValidationService
{
    private readonly ILogErrors _logErrors;

    public DirectoryValidationService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public bool IsWritableDirectory(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return false;

            var testFile = Path.Combine(path, Guid.NewGuid() + ".tmp");

            using (var fs = new FileStream(testFile, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                fs.Close();
            }

            DeleteFiles.TryDeleteFile(testFile);

            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Failed to check if directory is writable.");
            return false;
        }
    }
}
