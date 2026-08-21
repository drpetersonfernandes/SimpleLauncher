using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.CheckPaths;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Avalonia.Services.DisplaySystemInfo;

/// <summary>
/// Validates system configuration and produces human-readable system information.
/// Extracted from the WPF DisplaySystemInformation service — adapted for Avalonia
/// by returning data models instead of manipulating WPF UI elements directly.
/// </summary>
public class AvaloniaDisplaySystemInformation
{
    /// <summary>
    /// Validates a system configuration (folders, image folder, emulator paths).
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
            result.ErrorMessages.Add($"System Folder path is not valid or does not exist: '{string.Join(";", config.SystemFolders)}'");
        }

        if (!string.IsNullOrWhiteSpace(config.SystemImageFolder))
        {
            var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(config.SystemImageFolder);
            if (resolvedSystemImageFolder == null || !CheckPath.IsValidPath(resolvedSystemImageFolder))
            {
                result.IsValid = false;
                result.IsSystemImageFolderValid = false;
                result.ErrorMessages.Add($"System Image Folder path is not valid or does not exist: '{config.SystemImageFolder}'");
            }
        }

        foreach (var emulator in config.Emulators)
        {
            if (string.IsNullOrWhiteSpace(emulator.EmulatorLocation) || CheckPath.IsValidEmulatorExecutablePath(emulator.EmulatorLocation)) continue;

            result.IsValid = false;
            result.InvalidEmulatorLocations.Add(emulator.EmulatorLocation);
            result.ErrorMessages.Add($"Emulator path is not valid for {emulator.EmulatorName}: '{emulator.EmulatorLocation}'");
        }

        return result;
    }

    /// <summary>
    /// Builds a structured system information model for display in any Avalonia UI.
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

/// <summary>
/// Structured system information for display in Avalonia UI.
/// </summary>
public class SystemInfoModel
{
    public string SystemName { get; init; } = "";
    public List<string> SystemFolders { get; init; } = [];
    public string? SystemImageFolder { get; init; }
    public List<string> FileFormatsToSearch { get; init; } = [];
    public bool ExtractFileBeforeLaunch { get; init; }
    public List<string> FileFormatsToLaunch { get; init; } = [];
    public bool GroupByFolder { get; init; }
    public bool DisableRecursiveSearch { get; init; }
    public List<EmulatorInfoModel> Emulators { get; init; } = [];
    public bool AreSystemFoldersValid { get; init; }
    public bool IsSystemImageFolderValid { get; init; }
    public SystemValidationResult ValidationResult { get; init; } = new();
}

/// <summary>
/// Emulator information for display.
/// </summary>
public class EmulatorInfoModel
{
    public string Name { get; init; } = "";
    public string Location { get; init; } = "";
    public string Parameters { get; init; } = "";
    public bool ReceiveErrorNotification { get; init; }
    public bool IsLocationValid { get; init; }
}
