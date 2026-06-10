using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public interface IMountIsoFiles
{
    Task MountIsoFileAsync(string resolvedIsoFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, string logPath, ILauncherService gameLauncher, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task<bool> WaitForDirectoryToExistAsync(string directoryPath, int maxWaitTimeMs, int pollIntervalMs, ILogErrors logErrors);
    Task<string> ExecutePowerShellMountCommandAsync(string isoPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task ExecutePowerShellDismountCommandAsync(string isoPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
}
