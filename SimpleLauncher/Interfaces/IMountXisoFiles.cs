using SimpleLauncher.Services.GameLauncher.MountFiles;

namespace SimpleLauncher.Interfaces;

public interface IMountXisoFiles
{
    Task<MountXisoDrive> MountAsync(string resolvedIsoFilePath, string? logPath, ILogger logErrors, IMessageBoxLibraryService messageBox);
}
