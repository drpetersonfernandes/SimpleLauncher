using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class DisplaySystemInformation
{
    public static async Task DisplaySystemInfo(string systemFolder, int gameCount, SystemManager selectedManager, WrapPanel gameFileGrid)
{
    gameFileGrid.Children.Clear();

    var verticalStackPanel = new StackPanel
    {
        Orientation = Orientation.Vertical,
        Margin = new Thickness(10, 30, 10, 10)
    };

    var clickontheletterbuttonsabove2 = (string)Application.Current.TryFindResource("Clickontheletterbuttonsabove") ?? "Click on the letter buttons above to see the games";
    var systemFolder2 = (string)Application.Current.TryFindResource("SystemFolder") ?? "System Folder";
    var systemImageFolder2 = (string)Application.Current.TryFindResource("SystemImageFolder") ?? "System Image Folder";
    var defaultImageFolder2 = (string)Application.Current.TryFindResource("DefaultImageFolder") ?? "Using default image folder";
    var isthesystemMamEbased2 = (string)Application.Current.TryFindResource("IsthesystemMAMEbased") ?? "Is the system MAME-based?";
    var extensiontoSearchintheSystemFolder2 = (string)Application.Current.TryFindResource("ExtensiontoSearchintheSystemFolder2") ?? "Extension to Search in the System Folder";
    var extractFileBeforeLaunch2 = (string)Application.Current.TryFindResource("ExtractFileBeforeLaunch") ?? "Extract File Before Launch?";
    var extensiontoLaunchAfterExtraction2 = (string)Application.Current.TryFindResource("ExtensiontoLaunchAfterExtraction2") ?? "Extension to Launch After Extraction";

    var systemInfoTextBlock = new TextBlock();
    systemInfoTextBlock.Inlines.Add(new Run($"{clickontheletterbuttonsabove2}"));
    systemInfoTextBlock.Inlines.Add(new LineBreak());
    systemInfoTextBlock.Inlines.Add(new LineBreak());
    // Display the raw string from config
    systemInfoTextBlock.Inlines.Add(new Run($"{systemFolder2}: {selectedManager.SystemFolder}"));
    systemInfoTextBlock.Inlines.Add(new LineBreak());
    // Display the raw string from config, or default message if null/empty
    systemInfoTextBlock.Inlines.Add(new Run($"{systemImageFolder2}: {selectedManager.SystemImageFolder ?? defaultImageFolder2}"));
    systemInfoTextBlock.Inlines.Add(new LineBreak());
    systemInfoTextBlock.Inlines.Add(new Run($"{isthesystemMamEbased2}: {selectedManager.SystemIsMame}"));
    systemInfoTextBlock.Inlines.Add(new LineBreak());
    systemInfoTextBlock.Inlines.Add(new Run($"{extensiontoSearchintheSystemFolder2}: {string.Join(", ", selectedManager.FileFormatsToSearch)}"));
    systemInfoTextBlock.Inlines.Add(new LineBreak());
    systemInfoTextBlock.Inlines.Add(new Run($"{extractFileBeforeLaunch2}: {selectedManager.ExtractFileBeforeLaunch}"));
    systemInfoTextBlock.Inlines.Add(new LineBreak());
    systemInfoTextBlock.Inlines.Add(new Run($"{extensiontoLaunchAfterExtraction2}: {string.Join(", ", selectedManager.FileFormatsToLaunch)}"));
    verticalStackPanel.Children.Add(systemInfoTextBlock);

    var totalGamesCount2 = (string)Application.Current.TryFindResource("TotalGamesCount") ?? "Number of games in the System Folder: {0}";

    var gameCountTextBlock = new TextBlock();
    gameCountTextBlock.Inlines.Add(new LineBreak());
    gameCountTextBlock.Inlines.Add(new Run(string.Format(CultureInfo.InvariantCulture, totalGamesCount2, gameCount)));
    verticalStackPanel.Children.Add(gameCountTextBlock);

    var imageFolderPath = selectedManager.SystemImageFolder;
    // Resolve the system image path using PathHelper for checking existence and counting
    var resolvedImageFolderPath = string.IsNullOrWhiteSpace(imageFolderPath)
        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedManager.SystemName)
        : PathHelper.ResolveRelativeToAppDirectory(imageFolderPath);


    var numberOfImages2 = (string)Application.Current.TryFindResource("NumberOfImages") ?? "Number of images in the System Image Folder: {0}";
    var imageFolderNotExist2 = (string)Application.Current.TryFindResource("ImageFolderNotExist") ?? "System Image Folder does not exist or is not specified.";

    // Check if the resolved image path is valid before proceeding
    if (!string.IsNullOrEmpty(resolvedImageFolderPath) && Directory.Exists(resolvedImageFolderPath))
    {
        var imageExtensions = GetImageExtensions.GetExtensions();
        var imageCount = await Task.Run(() => Directory.EnumerateFiles(resolvedImageFolderPath, "*.*").Count(file => imageExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))));

        var imageCountTextBlock = new TextBlock();
        imageCountTextBlock.Inlines.Add(new Run(string.Format(CultureInfo.InvariantCulture, numberOfImages2, imageCount)));
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

    foreach (var emulator in selectedManager.Emulators)
    {
        var emulatorInfoTextBlock = new TextBlock();
        emulatorInfoTextBlock.Inlines.Add(new LineBreak());
        emulatorInfoTextBlock.Inlines.Add(new Run($"{emulatorName2}: {emulator.EmulatorName}"));
        emulatorInfoTextBlock.Inlines.Add(new LineBreak());
        // Display the raw string from config
        emulatorInfoTextBlock.Inlines.Add(new Run($"{emulatorLocation2}: {emulator.EmulatorLocation}"));
        emulatorInfoTextBlock.Inlines.Add(new LineBreak());
        // Display the raw string from config
        emulatorInfoTextBlock.Inlines.Add(new Run($"{emulatorParameters2}: {emulator.EmulatorParameters}"));
        emulatorInfoTextBlock.Inlines.Add(new LineBreak());
        emulatorInfoTextBlock.Inlines.Add(new Run($"{receiveNotificationEmulatorError2}: {emulator.ReceiveANotificationOnEmulatorError}"));
        verticalStackPanel.Children.Add(emulatorInfoTextBlock);
    }

    gameFileGrid.Children.Add(verticalStackPanel);

    // Validate the System (pass the raw systemFolder string, validation uses resolved path internally)
    ValidateSystemConfiguration(selectedManager.SystemFolder, selectedManager);
}

    private static void ValidateSystemConfiguration(string systemFolder, SystemManager selectedManager)
{
    var errorMessages = new StringBuilder();
    var hasErrors = false;

    // Resolve paths using PathHelper for validation
    var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(systemFolder);
    var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(selectedManager.SystemImageFolder);

    if (!CheckPath.IsValidPath(resolvedSystemFolder)) // CheckPath uses PathHelper internally
    {
        var systemFolderpathisnotvalid2 = (string)Application.Current.TryFindResource("SystemFolderpathisnotvalid") ?? "System Folder path is not valid or does not exist:";
        hasErrors = true;
        errorMessages.AppendLine(CultureInfo.InvariantCulture, $"{systemFolderpathisnotvalid2} '{systemFolder}'\n\n"); // Display the raw string
    }

    if (!string.IsNullOrWhiteSpace(selectedManager.SystemImageFolder) && !CheckPath.IsValidPath(resolvedSystemImageFolder)) // CheckPath uses PathHelper internally
    {
        var systemImageFolderpathisnotvalid2 = (string)Application.Current.TryFindResource("SystemImageFolderpathisnotvalid") ?? "System Image Folder path is not valid or does not exist:";
        hasErrors = true;
        errorMessages.AppendLine(CultureInfo.InvariantCulture, $"{systemImageFolderpathisnotvalid2} '{selectedManager.SystemImageFolder}'\n\n"); // Display the raw string
    }

    foreach (var emulator in selectedManager.Emulators)
    {
        // Resolve emulator location using PathHelper for validation
        var resolvedEmulatorLocation = PathHelper.ResolveRelativeToAppDirectory(emulator.EmulatorLocation);
        if (!string.IsNullOrWhiteSpace(emulator.EmulatorLocation) && !CheckPath.IsValidPath(resolvedEmulatorLocation)) // CheckPath uses PathHelper internally
        {
            var emulatorpathisnotvalidfor2 = (string)Application.Current.TryFindResource("Emulatorpathisnotvalidfor") ?? "Emulator path is not valid for";
            hasErrors = true;
            errorMessages.AppendLine(CultureInfo.InvariantCulture, $"{emulatorpathisnotvalidfor2} {emulator.EmulatorName}: '{emulator.EmulatorLocation}'\n\n"); // Display the raw string
        }
    }

    if (!hasErrors) return;

    MessageBoxLibrary.ListOfErrorsMessageBox(errorMessages);
}
}
