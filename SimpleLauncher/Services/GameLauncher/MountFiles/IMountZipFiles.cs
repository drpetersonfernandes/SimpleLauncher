using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public interface IMountZipFiles
{
    string ConfiguredMountDriveRoot { get; }
    Task MountZipFileAndLoadEbootBinAsync(string resolvedZipFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, string logPath, ILauncherService gameLauncher, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task MountZipFileAndSearchForFileToLoadAsync(string resolvedZipFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string rawEmulatorParameters, IWindowContext windowContext, string logPath, ILauncherService gameLauncher, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    Task MountZipFileAndLoadWithScummVmAsync(string resolvedZipFilePath, string selectedSystemName, string selectedEmulatorName, ISystemManager selectedSystemManager, Emulator selectedEmulatorManager, string selectedEmulatorParameters, string logPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox);
    void KillAllSimpleZipDriveProcesses(ILogErrors logErrors);
}
