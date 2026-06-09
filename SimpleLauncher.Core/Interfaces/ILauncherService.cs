using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.LoadingInterface;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Core.Interfaces;

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
