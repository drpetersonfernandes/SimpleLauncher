using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public interface IMountXisoFiles
{
    Task<MountXisoDrive> MountAsync(string resolvedIsoFilePath, string logPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
}
