using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

/// <summary>
/// Service that locates common game files (default.xex, default.xbe, .cue, .bin, EBOOT.BIN, ISO/IMG) within a directory.
/// </summary>
public class FileFinderService : IFileFinderService
{
    private readonly ILogErrors _logErrors;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileFinderService"/> class.
    /// </summary>
    public FileFinderService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    /// <summary>
    /// Finds the default.xex file (Xbox 360 executable) in the specified directory.
    /// </summary>
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

    /// <summary>
    /// Finds the default.xbe file (Xbox executable) in the specified directory.
    /// </summary>
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

    /// <summary>
    /// Finds the first .cue file in the specified directory.
    /// </summary>
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

    /// <summary>
    /// Finds the first .bin file in the specified directory.
    /// </summary>
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

    /// <summary>
    /// Finds the EBOOT.BIN file (PS3 executable) in the specified directory or its immediate subdirectories.
    /// </summary>
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

    /// <summary>
    /// Finds the first .iso or .img file in the specified directory.
    /// </summary>
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
