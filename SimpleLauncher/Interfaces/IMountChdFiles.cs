using SimpleLauncher.Services.GameLauncher.MountFiles;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface IMountChdFiles
{
    Task<MountChdDrive> MountAsync(string resolvedChdFilePath, int? consoleIndex, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task MountChdFileAndLoadAsync(string resolvedChdFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, ILauncherService gameLauncher, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task MountChdFileAndLoadWithConsoleIndexAsync(string resolvedChdFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, ILauncherService gameLauncher, int? consoleIndex, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    int? GetConsoleIndexFromSystemName(string systemName, string emulatorName, ILogErrors logErrors);
    void KillAllChdMounterProcesses(ILogErrors logErrors);
}
