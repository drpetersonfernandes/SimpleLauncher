using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface IMountZipFiles
{
    string ConfiguredMountDriveRoot { get; }
    Task MountZipFileAndLoadEbootBinAsync(string resolvedZipFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, string? logPath, ILauncherService gameLauncher, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task MountZipFileAndSearchForFileToLoadAsync(string resolvedZipFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, string? logPath, ILauncherService gameLauncher, ILogger logErrors, IMessageBoxLibraryService messageBox);
    Task MountZipFileAndLoadWithScummVmAsync(string resolvedZipFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string selectedEmulatorParameters, string? logPath, ILogger logErrors, IMessageBoxLibraryService messageBox);
    void KillAllSimpleZipDriveProcesses(ILogger logErrors);
}
