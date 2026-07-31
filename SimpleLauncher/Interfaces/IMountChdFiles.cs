using SimpleLauncher.Services.GameLauncher.MountFiles;
using SimpleLauncher.Models;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface IMountChdFiles
{
    Task<MountChdDrive> MountAsync(string resolvedChdFilePath, int? consoleIndex, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task MountChdFileAndLoadAsync(string resolvedChdFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, ILauncherService gameLauncher, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task MountChdFileAndLoadWithConsoleIndexAsync(string resolvedChdFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, ILauncherService gameLauncher, int? consoleIndex, ILogger logErrors, IMessageBoxLibraryService messageBox);
    int? GetConsoleIndexFromSystemName(string systemName, string emulatorName, ILogger logErrors);
    void KillAllChdMounterProcesses(ILogger logErrors);
}
