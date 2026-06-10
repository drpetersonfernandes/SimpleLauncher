using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public interface IMountXisoFiles
{
    Task<MountXisoDrive> MountAsync(string resolvedIsoFilePath, string logPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
}
