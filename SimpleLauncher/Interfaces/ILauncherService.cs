using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Interfaces;

public interface ILauncherService
{
    Task LaunchRegularEmulatorAsync(
        string resolvedFilePath,
        string selectedEmulatorName,
        ISystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        IWindowContext windowContext,
        ILoadingState loadingStateProvider,
        string originalFilePathForDisplay = null);

    Task RunBatchFileAsync(
        string resolvedFilePath,
        Emulator selectedEmulatorManager,
        IWindowContext windowContext);

    Task LaunchShortcutFileAsync(
        string resolvedFilePath,
        Emulator selectedEmulatorManager,
        IWindowContext windowContext);

    Task LaunchExecutableAsync(
        string resolvedFilePath,
        Emulator selectedEmulatorManager,
        IWindowContext windowContext);
}
