using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public interface IMountChdFiles
{
    Task<MountChdDrive> MountAsync(string resolvedChdFilePath, int? consoleIndex, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task MountChdFileAndLoadAsync(string resolvedChdFilePath, string selectedSystemName, string selectedEmulatorName, SystemManager.SystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, GameLauncher gameLauncher, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task MountChdFileAndLoadWithConsoleIndexAsync(string resolvedChdFilePath, string selectedSystemName, string selectedEmulatorName, SystemManager.SystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, GameLauncher gameLauncher, int? consoleIndex, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    int? GetConsoleIndexFromSystemName(string systemName, string emulatorName, ILogErrors logErrors);
    void KillAllChdMounterProcesses(ILogErrors logErrors);
}
