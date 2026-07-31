using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CleanAndDeleteFiles;

namespace SimpleLauncher.Services.CheckIfDirectoryIsWritable;

public class DirectoryValidationService : IDirectoryValidationService
{
    private readonly ILogger _logger;

    public DirectoryValidationService(ILogger logErrors)
    {
        _logger = logErrors;
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
            _logger.Error(ex, "Failed to check if directory is writable.");
            return false;
        }
    }
}
