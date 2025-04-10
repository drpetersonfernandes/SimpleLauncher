using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SimpleLauncher;

public static class SystemManager
{
    public static void DisplaySystemInfo(string systemFolder, int gameCount, SystemConfig selectedConfig, WrapPanel gameFileGrid)
    {
        gameFileGrid.Children.Clear();

        // Create a StackPanel to hold TextBlocks vertically
        var verticalStackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(10, 30, 10, 10)
        };

        var systemFolder2 = (string)Application.Current.TryFindResource("SystemFolder") ?? "System Folder";
        var systemImageFolder2 = (string)Application.Current.TryFindResource("SystemImageFolder") ?? "System Image Folder";
        var defaultImageFolder2 = (string)Application.Current.TryFindResource("DefaultImageFolder") ?? "Using default image folder";
        var isthesystemMamEbased2 = (string)Application.Current.TryFindResource("IsthesystemMAMEbased") ?? "Is the system MAME-based?";
        var extensiontoSearchintheSystemFolder2 = (string)Application.Current.TryFindResource("ExtensiontoSearchintheSystemFolder2") ?? "Extension to Search in the System Folder";
        var extractFileBeforeLaunch2 = (string)Application.Current.TryFindResource("ExtractFileBeforeLaunch") ?? "Extract File Before Launch?";
        var extensiontoLaunchAfterExtraction2 = (string)Application.Current.TryFindResource("ExtensiontoLaunchAfterExtraction2") ?? "Extension to Launch After Extraction";

        // Create System Info TextBlock with LineBreaks
        var systemInfoTextBlock = new TextBlock();
        systemInfoTextBlock.Inlines.Add(new Run($"{systemFolder2}: {systemFolder}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{systemImageFolder2}: {selectedConfig.SystemImageFolder ?? defaultImageFolder2}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{isthesystemMamEbased2}: {selectedConfig.SystemIsMame}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{extensiontoSearchintheSystemFolder2}: {string.Join(", ", selectedConfig.FileFormatsToSearch)}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{extractFileBeforeLaunch2}: {selectedConfig.ExtractFileBeforeLaunch}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{extensiontoLaunchAfterExtraction2}: {string.Join(", ", selectedConfig.FileFormatsToLaunch)}"));
        verticalStackPanel.Children.Add(systemInfoTextBlock);

        var totalGamesCount2 = (string)Application.Current.TryFindResource("TotalGamesCount") ?? "Number of games in the System Folder: {0}";

        // Add the number of games in the system folder
        var gameCountTextBlock = new TextBlock();
        gameCountTextBlock.Inlines.Add(new LineBreak());
        gameCountTextBlock.Inlines.Add(new Run(string.Format(totalGamesCount2, gameCount)));
        verticalStackPanel.Children.Add(gameCountTextBlock);

        // Determine the image folder to search
        var imageFolderPath = selectedConfig.SystemImageFolder;
        if (string.IsNullOrWhiteSpace(imageFolderPath))
        {
            imageFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedConfig.SystemName);
        }

        var numberOfImages2 = (string)Application.Current.TryFindResource("NumberOfImages") ?? "Number of images in the System Image Folder: {0}";
        var imageFolderNotExist2 = (string)Application.Current.TryFindResource("ImageFolderNotExist") ?? "System Image Folder does not exist or is not specified.";

        // Add the number of images in the system's image folder
        if (Directory.Exists(imageFolderPath))
        {
            var imageExtensions = new List<string> { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" };
            var imageCount = imageExtensions.Sum(ext => Directory.GetFiles(imageFolderPath, ext).Length);

            var imageCountTextBlock = new TextBlock();
            imageCountTextBlock.Inlines.Add(new Run(string.Format(numberOfImages2, imageCount)));
            verticalStackPanel.Children.Add(imageCountTextBlock);
        }
        else
        {
            var noImageFolderTextBlock = new TextBlock();
            noImageFolderTextBlock.Inlines.Add(new Run(imageFolderNotExist2));
            verticalStackPanel.Children.Add(noImageFolderTextBlock);
        }

        var emulatorName2 = (string)Application.Current.TryFindResource("EmulatorName") ?? "Emulator Name";
        var emulatorLocation2 = (string)Application.Current.TryFindResource("EmulatorLocation") ?? "Emulator Location";
        var emulatorParameters2 = (string)Application.Current.TryFindResource("EmulatorParameters") ?? "Emulator Parameters";
        var receiveNotificationEmulatorError2 = (string)Application.Current.TryFindResource("receiveNotificationEmulatorError") ?? "Receive a Notification on Emulator Error?";

        // Dynamically create and add a TextBlock for each emulator to the vertical StackPanel
        foreach (var emulator in selectedConfig.Emulators)
        {
            var emulatorInfoTextBlock = new TextBlock();
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{emulatorName2}: {emulator.EmulatorName}"));
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{emulatorLocation2}: {emulator.EmulatorLocation}"));
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{emulatorParameters2}: {emulator.EmulatorParameters}"));
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{receiveNotificationEmulatorError2}: {emulator.ReceiveANotificationOnEmulatorError}"));
            verticalStackPanel.Children.Add(emulatorInfoTextBlock);
        }

        // Add the vertical StackPanel to the horizontal WrapPanel
        gameFileGrid.Children.Add(verticalStackPanel);

        // Validate the System
        ValidateSystemConfiguration(systemFolder, selectedConfig);
    }

    private static void ValidateSystemConfiguration(string systemFolder, SystemConfig selectedConfig)
    {
        var errorMessages = new StringBuilder();
        var hasErrors = false;

        // Validate the system folder path
        if (!IsValidPath(systemFolder))
        {
            var systemFolderpathisnotvalid2 = (string)Application.Current.TryFindResource("SystemFolderpathisnotvalid") ?? "System Folder path is not valid or does not exist:";
            hasErrors = true;
            errorMessages.AppendLine(CultureInfo.InvariantCulture, $"{systemFolderpathisnotvalid2} '{systemFolder}'\n\n");
        }

        // Validate the system image folder path if it's provided. Allow null or empty.
        if (!string.IsNullOrWhiteSpace(selectedConfig.SystemImageFolder) && !IsValidPath(selectedConfig.SystemImageFolder))
        {
            var systemImageFolderpathisnotvalid2 = (string)Application.Current.TryFindResource("SystemImageFolderpathisnotvalid") ?? "System Image Folder path is not valid or does not exist:";
            hasErrors = true;
            errorMessages.AppendLine(CultureInfo.InvariantCulture, $"{systemImageFolderpathisnotvalid2} '{selectedConfig.SystemImageFolder}'\n\n");
        }

        // Validate each emulator's location path if it's provided. Allow null or empty.
        foreach (var emulator in selectedConfig.Emulators)
        {
            if (string.IsNullOrWhiteSpace(emulator.EmulatorLocation) ||
                IsValidPath(emulator.EmulatorLocation)) continue;
            var emulatorpathisnotvalidfor2 = (string)Application.Current.TryFindResource("Emulatorpathisnotvalidfor") ?? "Emulator path is not valid for";
            hasErrors = true;
            errorMessages.AppendLine(CultureInfo.InvariantCulture, $"{emulatorpathisnotvalidfor2} {emulator.EmulatorName}: '{emulator.EmulatorLocation}'\n\n");
        }

        // Display all error messages if there are any errors
        if (!hasErrors) return;

        // Notify user
        ListOfErrorsMessageBox(errorMessages);
    }

    private static void ListOfErrorsMessageBox(StringBuilder errorMessages)
    {
        var editSystemtofixit2 = (string)Application.Current.TryFindResource("EditSystemtofixit") ?? "Edit System to fix it.";
        var validationerrors2 = (string)Application.Current.TryFindResource("Validationerrors") ?? "Validation errors";
        MessageBox.Show(errorMessages + editSystemtofixit2,
            validationerrors2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    // Check paths in SystemFolder, SystemImageFolder and EmulatorLocation. Allow relative paths.
    private static bool IsValidPath(string path)
    {
        // Check if the path is not null or whitespace
        if (string.IsNullOrWhiteSpace(path)) return false;

        // Check if the path is an absolute path and exists
        if (Directory.Exists(path) || File.Exists(path)) return true;

        // Assume the path might be relative and combine it with the base directory
        // Allow relative paths
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var fullPath = Path.Combine(basePath, path);

        // Check if the combined path exists
        return Directory.Exists(fullPath) || File.Exists(fullPath);
    }
}