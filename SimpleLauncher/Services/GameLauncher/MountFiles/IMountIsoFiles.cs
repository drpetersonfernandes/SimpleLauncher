using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public interface IMountIsoFiles
{
    Task MountIsoFileAsync(string resolvedIsoFilePath, string selectedSystemName, string selectedEmulatorName, SystemManager.SystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, string logPath, GameLauncher gameLauncher, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task<bool> WaitForDirectoryToExistAsync(string directoryPath, int maxWaitTimeMs, int pollIntervalMs, ILogErrors logErrors);
    Task<string> ExecutePowerShellMountCommandAsync(string isoPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task ExecutePowerShellDismountCommandAsync(string isoPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
}
