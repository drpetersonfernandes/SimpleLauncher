using SimpleLauncher.Avalonia.Models;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.CheckPaths;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Avalonia.Services.DisplaySystemInfo;

/// <summary>
///     Validates system configuration and produces human-readable system information.
///     Extracted from the WPF DisplaySystemInformation service — adapted for Avalonia
///     by returning data models instead of manipulating WPF UI elements directly.
/// </summary>
public class AvaloniaDisplaySystemInformation
{
    private readonly LocalizationService? _localization;

    public AvaloniaDisplaySystemInformation(LocalizationService? localization = null)
    {
        _localization = localization;
    }

    /// <summary>
    ///     Validates a system configuration (folders, image folder, emulator paths).
    /// </summary>
    public SystemValidationResult ValidateSystemConfiguration(SystemManagerConfig config)
    {
        var result = new SystemValidationResult();

        var allFoldersValid = config.SystemFolders.All(static folder =>
        {
            var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(folder);
            return resolvedSystemFolder != null && CheckPath.IsValidPath(resolvedSystemFolder);
        });

        if (!allFoldersValid)
        {
            result.IsValid = false;
            result.AreSystemFoldersValid = false;
            // WPF parity: use localized strings with trailing newlines
            var systemFolderMsg = _localization?.GetString("SystemFolderpathisnotvalid") ??
                                  "System Folder path is not valid or does not exist:";
            result.ErrorMessages.Add($"{systemFolderMsg} '{string.Join(";", config.SystemFolders)}'\n\n");
        }

        if (!string.IsNullOrWhiteSpace(config.SystemImageFolder))
        {
            var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(config.SystemImageFolder);
            if (resolvedSystemImageFolder == null || !CheckPath.IsValidPath(resolvedSystemImageFolder))
            {
                result.IsValid = false;
                result.IsSystemImageFolderValid = false;
                // WPF parity: use localized strings with trailing newlines
                var imageFolderMsg = _localization?.GetString("SystemImageFolderpathisnotvalid") ??
                                     "System Image Folder path is not valid or does not exist:";
                result.ErrorMessages.Add($"{imageFolderMsg} '{config.SystemImageFolder}'\n\n");
            }
        }

        foreach (var emulator in config.Emulators)
        {
            if (string.IsNullOrWhiteSpace(emulator.EmulatorLocation) ||
                CheckPath.IsValidEmulatorExecutablePath(emulator.EmulatorLocation))
            {
                continue;
            }

            result.IsValid = false;
            result.InvalidEmulatorLocations.Add(emulator.EmulatorLocation);
            // WPF parity: use localized strings with trailing newlines
            var emulatorMsg = _localization?.GetString("Emulatorpathisnotvalidfor") ?? "Emulator path is not valid for";
            result.ErrorMessages.Add($"{emulatorMsg} {emulator.EmulatorName}: '{emulator.EmulatorLocation}'\n\n");
        }

        return result;
    }

    /// <summary>
    ///     Builds a structured system information model for display in any Avalonia UI.
    /// </summary>
    public SystemInfoModel BuildSystemInfo(SystemManagerConfig config)
    {
        var validation = ValidateSystemConfiguration(config);

        var emulators = config.Emulators.Select(e => new EmulatorInfoModel
        {
            Name = e.EmulatorName,
            Location = e.EmulatorLocation,
            Parameters = e.EmulatorParameters,
            ReceiveErrorNotification = e.ReceiveANotificationOnEmulatorError,
            IsLocationValid = !validation.InvalidEmulatorLocations.Contains(e.EmulatorLocation)
        }).ToList();

        return new SystemInfoModel
        {
            SystemName = config.SystemName,
            SystemFolders = config.SystemFolders.ToList(),
            SystemImageFolder = config.SystemImageFolder,
            FileFormatsToSearch = config.FileFormatsToSearch.ToList(),
            ExtractFileBeforeLaunch = config.ExtractFileBeforeLaunch,
            FileFormatsToLaunch = config.FileFormatsToLaunch.ToList(),
            GroupByFolder = config.GroupByFolder,
            DisableRecursiveSearch = config.DisableRecursiveSearch,
            Emulators = emulators,
            AreSystemFoldersValid = validation.AreSystemFoldersValid,
            IsSystemImageFolderValid = validation.IsSystemImageFolderValid,
            ValidationResult = validation
        };
    }
}