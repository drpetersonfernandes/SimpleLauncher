using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services;

public static class DisplaySystemInformation
{
    public static async Task<SystemValidationResult> DisplaySystemInfoAsync(SystemManager selectedManager, WrapPanel gameFileGrid, CancellationToken cancellationToken = default)
    {
        gameFileGrid.Children.Clear();

        var verticalStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 30, 10, 10)
        };

        // --- UI Text Resources ---
        var clickontheletterbuttonsabove2 = (string)Application.Current.TryFindResource("Clickontheletterbuttonsabove") ?? "Click on the letter buttons above to see the games";
        var systemFolder2 = (string)Application.Current.TryFindResource("SystemFolder") ?? "System Folder";
        var systemImageFolder2 = (string)Application.Current.TryFindResource("SystemImageFolder") ?? "System Image Folder";
        var defaultImageFolder2 = (string)Application.Current.TryFindResource("DefaultImageFolder") ?? "Using default image folder";
        var isthesystemMamEbased2 = (string)Application.Current.TryFindResource("IsthesystemMAMEbased") ?? "Is the system MAME-based?";
        var extensiontoSearchintheSystemFolder2 = (string)Application.Current.TryFindResource("ExtensiontoSearchintheSystemFolder2") ?? "Extension to Search in the System Folder";
        var groupFilesByFolder2 = (string)Application.Current.TryFindResource("GroupFilesByFolder") ?? "Group Files by Folder?";
        var extractFileBeforeLaunch2 = (string)Application.Current.TryFindResource("ExtractFileBeforeLaunch") ?? "Extract File Before Launch?";
        var extensiontoLaunchAfterExtraction2 = (string)Application.Current.TryFindResource("ExtensiontoLaunchAfterExtraction2") ?? "Extension to Launch After Extraction";
        // var totalGamesCount2 = (string)Application.Current.TryFindResource("TotalGamesCount") ?? "Number of games in the System Folder: {0}";
        // var numberOfImages2 = (string)Application.Current.TryFindResource("NumberOfImages") ?? "Number of images in the System Image Folder: {0}";
        // var imageFolderNotExist2 = (string)Application.Current.TryFindResource("ImageFolderNotExist") ?? "System Image Folder does not exist or is not specified.";
        var emulatorName2 = (string)Application.Current.TryFindResource("EmulatorName") ?? "Emulator Name";
        var emulatorLocation2 = (string)Application.Current.TryFindResource("EmulatorPath") ?? "Emulator Path";
        var emulatorParameters2 = (string)Application.Current.TryFindResource("EmulatorParameters") ?? "Emulator Parameters";
        var receiveNotificationEmulatorError2 = (string)Application.Current.TryFindResource("receiveNotificationEmulatorError") ?? "Receive a Notification on Emulator Error?";

        // --- Validate Configuration ---
        // Offload path validation to a background thread to prevent UI freezing
        var validationResult = await Task.Run(() => ValidateSystemConfiguration(selectedManager), cancellationToken);

        // --- Create UI Elements and Apply Validation Styling ---
        var systemInfoTextBlock = new TextBlock();
        systemInfoTextBlock.Inlines.Add(new Run($"{clickontheletterbuttonsabove2}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new LineBreak());

        var systemFoldersRun = new Run($"{systemFolder2}: {string.Join("; ", selectedManager.SystemFolders)}");
        if (!validationResult.AreSystemFoldersValid)
        {
            systemFoldersRun.Foreground = Brushes.Red;
        }

        systemInfoTextBlock.Inlines.Add(systemFoldersRun);
        systemInfoTextBlock.Inlines.Add(new LineBreak());

        var systemImageFolderRun = new Run($"{systemImageFolder2}: {selectedManager.SystemImageFolder ?? defaultImageFolder2}");
        if (!validationResult.IsSystemImageFolderValid)
        {
            systemImageFolderRun.Foreground = Brushes.Red;
        }

        systemInfoTextBlock.Inlines.Add(systemImageFolderRun);
        systemInfoTextBlock.Inlines.Add(new LineBreak());

        systemInfoTextBlock.Inlines.Add(new Run($"{isthesystemMamEbased2}: {selectedManager.SystemIsMame}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{extensiontoSearchintheSystemFolder2}: {string.Join(", ", selectedManager.FileFormatsToSearch)}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{groupFilesByFolder2}: {selectedManager.GroupByFolder}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{extractFileBeforeLaunch2}: {selectedManager.ExtractFileBeforeLaunch}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{extensiontoLaunchAfterExtraction2}: {string.Join(", ", selectedManager.FileFormatsToLaunch)}"));
        verticalStackPanel.Children.Add(systemInfoTextBlock);

        // // --- Game and Image Count ---
        // var allFiles = new List<string>();
        // foreach (var folder in selectedManager.SystemFolders)
        // {
        //     cancellationToken.ThrowIfCancellationRequested();
        //     var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(folder);
        //     if (!string.IsNullOrEmpty(resolvedPath) && Directory.Exists(resolvedPath))
        //     {
        //         allFiles.AddRange(await GetListOfFiles.GetFilesAsync(resolvedPath, selectedManager.FileFormatsToSearch, cancellationToken));
        //     }
        // }
        //
        // var gameCount = allFiles.Count;
        //
        // var gameCountTextBlock = new TextBlock();
        // gameCountTextBlock.Inlines.Add(new LineBreak());
        // gameCountTextBlock.Inlines.Add(new Run(string.Format(CultureInfo.InvariantCulture, totalGamesCount2, gameCount)));
        // verticalStackPanel.Children.Add(gameCountTextBlock);
        //
        // var imageFolderPath = selectedManager.SystemImageFolder;
        // var resolvedImageFolderPath = string.IsNullOrWhiteSpace(imageFolderPath)
        //     ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedManager.SystemName)
        //     : PathHelper.ResolveRelativeToAppDirectory(imageFolderPath);
        //
        // if (!string.IsNullOrEmpty(resolvedImageFolderPath) && Directory.Exists(resolvedImageFolderPath))
        // {
        //     var imageExtensions = GetImageExtensions.GetExtensions();
        //     var imageCount = await Task.Run(() => Directory.EnumerateFiles(resolvedImageFolderPath, "*.*").Count(file => imageExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))), cancellationToken);
        //     var imageCountTextBlock = new TextBlock();
        //     imageCountTextBlock.Inlines.Add(new Run(string.Format(CultureInfo.InvariantCulture, numberOfImages2, imageCount)));
        //     verticalStackPanel.Children.Add(imageCountTextBlock);
        // }
        // else
        // {
        //     var noImageFolderTextBlock = new TextBlock();
        //     noImageFolderTextBlock.Inlines.Add(new Run(imageFolderNotExist2));
        //     verticalStackPanel.Children.Add(noImageFolderTextBlock);
        // }

        // --- Emulator Info ---
        foreach (var emulator in selectedManager.Emulators)
        {
            var emulatorInfoTextBlock = new TextBlock();
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{emulatorName2}: {emulator.EmulatorName}"));
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());

            var emulatorLocationRun = new Run($"{emulatorLocation2}: {emulator.EmulatorLocation}");
            if (validationResult.InvalidEmulatorLocations.Contains(emulator.EmulatorLocation))
            {
                emulatorLocationRun.Foreground = Brushes.Red;
            }

            emulatorInfoTextBlock.Inlines.Add(emulatorLocationRun);

            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{emulatorParameters2}: {emulator.EmulatorParameters}"));
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{receiveNotificationEmulatorError2}: {emulator.ReceiveANotificationOnEmulatorError}"));
            verticalStackPanel.Children.Add(emulatorInfoTextBlock);
        }

        gameFileGrid.Children.Add(verticalStackPanel);

        return validationResult;
    }

    private static SystemValidationResult ValidateSystemConfiguration(SystemManager selectedManager)
    {
        var result = new SystemValidationResult();

        // Validate all system folders
        var allFoldersValid = selectedManager.SystemFolders.All(folder =>
        {
            var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(folder);
            return CheckPath.IsValidPath(resolvedSystemFolder);
        });

        if (!allFoldersValid)
        {
            result.IsValid = false;
            result.AreSystemFoldersValid = false;
            var systemFolderpathisnotvalid2 = (string)Application.Current.TryFindResource("SystemFolderpathisnotvalid") ?? "System Folder path is not valid or does not exist:";
            result.ErrorMessages.Add($"{systemFolderpathisnotvalid2} '{string.Join(";", selectedManager.SystemFolders)}'\n\n");
        }

        // Validate image folder
        if (!string.IsNullOrWhiteSpace(selectedManager.SystemImageFolder))
        {
            var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(selectedManager.SystemImageFolder);
            if (!CheckPath.IsValidPath(resolvedSystemImageFolder))
            {
                result.IsValid = false;
                result.IsSystemImageFolderValid = false;
                var systemImageFolderpathisnotvalid2 = (string)Application.Current.TryFindResource("SystemImageFolderpathisnotvalid") ?? "System Image Folder path is not valid or does not exist:";
                result.ErrorMessages.Add($"{systemImageFolderpathisnotvalid2} '{selectedManager.SystemImageFolder}'\n\n");
            }
        }

        // Validate emulators
        foreach (var emulator in selectedManager.Emulators)
        {
            var resolvedEmulatorLocation = PathHelper.ResolveRelativeToAppDirectory(emulator.EmulatorLocation);
            if (string.IsNullOrWhiteSpace(emulator.EmulatorLocation) || CheckPath.IsValidPath(resolvedEmulatorLocation)) continue;

            result.IsValid = false;
            result.InvalidEmulatorLocations.Add(emulator.EmulatorLocation);
            var emulatorpathisnotvalidfor2 = (string)Application.Current.TryFindResource("Emulatorpathisnotvalidfor") ?? "Emulator path is not valid for";
            result.ErrorMessages.Add($"{emulatorpathisnotvalidfor2} {emulator.EmulatorName}: '{emulator.EmulatorLocation}'\n\n");
        }

        return result;
    }
}