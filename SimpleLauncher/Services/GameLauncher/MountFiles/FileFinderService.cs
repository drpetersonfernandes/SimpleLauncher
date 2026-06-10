using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public class FileFinderService : IFileFinderService
{
    private readonly ILogErrors _logErrors;

    public FileFinderService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public string FindDefaultXex(string directory)
    {
        try
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var defaultXexPath = Path.Combine(directory, "default.xex");
            return File.Exists(defaultXexPath) ? defaultXexPath : null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error finding default.xex in path: {directory}");
            return null;
        }
    }

    public string FindDefaultXbe(string directory)
    {
        try
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var defaultXbePath = Path.Combine(directory, "default.xbe");
            return File.Exists(defaultXbePath) ? defaultXbePath : null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error finding default.xbe in path: {directory}");
            return null;
        }
    }

    public string FindCueFile(string directory)
    {
        try
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var cueFiles = Directory.GetFiles(directory, "*.cue");
            return cueFiles.Length > 0 ? cueFiles[0] : null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error finding cue file in path: {directory}");
            return null;
        }
    }

    public string FindBinFile(string directory)
    {
        try
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var binFiles = Directory.GetFiles(directory, "*.bin");
            return binFiles.Length > 0 ? binFiles[0] : null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error finding bin file in path: {directory}");
            return null;
        }
    }

    public string FindEbootBin(string directory)
    {
        try
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var ebootPath = Path.Combine(directory, "EBOOT.BIN");
            if (File.Exists(ebootPath))
                return ebootPath;

            // Search in subdirectories
            foreach (var subDir in Directory.GetDirectories(directory))
            {
                ebootPath = Path.Combine(subDir, "EBOOT.BIN");
                if (File.Exists(ebootPath))
                    return ebootPath;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error finding EBOOT.BIN in path: {directory}");
            return null;
        }
    }

    public string FindImageIso(string directory)
    {
        try
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var isoFiles = Directory.GetFiles(directory, "*.iso");
            if (isoFiles.Length > 0)
                return isoFiles[0];

            var imgFiles = Directory.GetFiles(directory, "*.img");
            if (imgFiles.Length > 0)
                return imgFiles[0];

            return null;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error finding ISO/IMG file in path: {directory}");
            return null;
        }
    }
}
