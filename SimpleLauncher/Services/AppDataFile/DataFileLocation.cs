using System.IO;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.CheckPaths;

namespace SimpleLauncher.Services.AppDataFile;

public sealed class DataFileLocation
{
    private readonly string _fileName;
    public string FilePath { get; private set; }
    public string TempFilePath => FilePath + ".tmp";
    public bool IsPortableMode { get; private set; }

    public DataFileLocation(string fileName)
    {
        _fileName = fileName;
        var portablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _fileName);
        Initialize(portablePath);
    }

    public DataFileLocation(IConfiguration configuration, string configKey, string defaultFileName)
    {
        _fileName = defaultFileName;
        var configuredPath = configuration.GetValue<string>(configKey) ?? defaultFileName;
        var portablePath = PathHelper.ResolveRelativeToAppDirectory(configuredPath) ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, defaultFileName);
        Initialize(portablePath);
    }

    private void Initialize(string portablePath)
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDataFolder = Path.Combine(localAppData, "SimpleLauncher");
        var localAppDataPath = Path.Combine(appDataFolder, _fileName);
        var portableExists = File.Exists(portablePath);
        var localExists = File.Exists(localAppDataPath);

        switch (portableExists)
        {
            case true when !localExists:
                FilePath = portablePath;
                IsPortableMode = true;
                break;
            case false when localExists:
                FilePath = localAppDataPath;
                IsPortableMode = false;
                break;
            case true when localExists:
            {
                var portableInfo = new FileInfo(portablePath);
                var localInfo = new FileInfo(localAppDataPath);
                if (portableInfo.LastWriteTimeUtc > localInfo.LastWriteTimeUtc)
                {
                    FilePath = portablePath;
                    IsPortableMode = true;
                }
                else
                {
                    FilePath = localAppDataPath;
                    IsPortableMode = false;
                }

                break;
            }
            default:
            {
                if (IsDirectoryWritable(AppDomain.CurrentDomain.BaseDirectory))
                {
                    FilePath = portablePath;
                    IsPortableMode = true;
                }
                else
                {
                    EnsureDirectoryExists(appDataFolder);
                    FilePath = localAppDataPath;
                    IsPortableMode = false;
                }

                break;
            }
        }
    }

    public string GetLocalAppDataPath()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDataFolder = Path.Combine(localAppData, "SimpleLauncher");
        return Path.Combine(appDataFolder, _fileName);
    }

    public bool TryFallbackToLocalAppData()
    {
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDataFolder = Path.Combine(localAppData, "SimpleLauncher");
            var newFilePath = Path.Combine(appDataFolder, _fileName);

            EnsureDirectoryExists(appDataFolder);

            FilePath = newFilePath;
            IsPortableMode = false;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsDirectoryWritable(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
                return false;

            var testFilePath = Path.Combine(directoryPath, $".write_test_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFilePath, "test");
            File.Delete(testFilePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void EnsureDirectoryExists(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        catch
        {
            // ignored
        }
    }
}