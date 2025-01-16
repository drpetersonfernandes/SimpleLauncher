using System;
using System.Collections.Generic;
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
            Margin = new Thickness(10)
        };

        // Create System Info TextBlock with LineBreaks
        var systemInfoTextBlock = new TextBlock();
        systemInfoTextBlock.Inlines.Add(new Run($"{Application.Current.Resources["SystemFolder"]}: {systemFolder}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{Application.Current.Resources["SystemImageFolder"]}: {selectedConfig.SystemImageFolder ?? Application.Current.Resources["DefaultImageFolder"]}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{Application.Current.Resources["SystemIsMame"]}: {selectedConfig.SystemIsMame}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{Application.Current.Resources["FileFormatsToSearch"]}: {string.Join(", ", selectedConfig.FileFormatsToSearch)}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{Application.Current.Resources["ExtractFileBeforeLaunch"]}: {selectedConfig.ExtractFileBeforeLaunch}"));
        systemInfoTextBlock.Inlines.Add(new LineBreak());
        systemInfoTextBlock.Inlines.Add(new Run($"{Application.Current.Resources["FileFormatsToLaunch"]}: {string.Join(", ", selectedConfig.FileFormatsToLaunch)}"));
        verticalStackPanel.Children.Add(systemInfoTextBlock);

        // Add the number of games in the system folder
        var gameCountTextBlock = new TextBlock();
        gameCountTextBlock.Inlines.Add(new LineBreak());
        gameCountTextBlock.Inlines.Add(new Run(string.Format(Application.Current.Resources["TotalGamesCount"] as string ?? throw new InvalidOperationException("TotalGamesCount is null"), gameCount)));
        verticalStackPanel.Children.Add(gameCountTextBlock);

        // Determine the image folder to search
        string imageFolderPath = selectedConfig.SystemImageFolder;
        if (string.IsNullOrWhiteSpace(imageFolderPath))
        {
            imageFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedConfig.SystemName);
        }

        // Add the number of images in the system's image folder
        if (Directory.Exists(imageFolderPath))
        {
            var imageExtensions = new List<string> { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" };
            int imageCount = imageExtensions.Sum(ext => Directory.GetFiles(imageFolderPath, ext).Length);

            var imageCountTextBlock = new TextBlock();
            imageCountTextBlock.Inlines.Add(new Run(string.Format(Application.Current.Resources["NumberOfImages"] as string ?? throw new InvalidOperationException("NumberOfImages is null"), imageCount)));
            verticalStackPanel.Children.Add(imageCountTextBlock);
        }
        else
        {
            var noImageFolderTextBlock = new TextBlock();
            noImageFolderTextBlock.Inlines.Add(new Run(Application.Current.Resources["ImageFolderNotExist"] as string));
            verticalStackPanel.Children.Add(noImageFolderTextBlock);
        }

        // Dynamically create and add a TextBlock for each emulator to the vertical StackPanel
        foreach (var emulator in selectedConfig.Emulators)
        {
            var emulatorInfoTextBlock = new TextBlock();
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{Application.Current.Resources["EmulatorName"]}: {emulator.EmulatorName}"));
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{Application.Current.Resources["EmulatorLocation"]}: {emulator.EmulatorLocation}"));
            emulatorInfoTextBlock.Inlines.Add(new LineBreak());
            emulatorInfoTextBlock.Inlines.Add(new Run($"{Application.Current.Resources["EmulatorParameters"]}: {emulator.EmulatorParameters}"));
            verticalStackPanel.Children.Add(emulatorInfoTextBlock);
        }

        // Add the vertical StackPanel to the horizontal WrapPanel
        gameFileGrid.Children.Add(verticalStackPanel);

        // Validate the System
        ValidateSystemConfiguration(systemFolder, selectedConfig);
    }
    
    private static void ValidateSystemConfiguration(string systemFolder, SystemConfig selectedConfig)
    {
        StringBuilder errorMessages = new StringBuilder();
        bool hasErrors = false;

        // Validate the system folder path
        if (!IsValidPath(systemFolder))
        {
            hasErrors = true;
            errorMessages.AppendLine($"System Folder path is not valid or does not exist: '{systemFolder}'\n\n");
        }

        // Validate the system image folder path if it's provided. Allow null or empty.
        if (!string.IsNullOrWhiteSpace(selectedConfig.SystemImageFolder) && !IsValidPath(selectedConfig.SystemImageFolder))
        {
            hasErrors = true;
            errorMessages.AppendLine($"System Image Folder path is not valid or does not exist: '{selectedConfig.SystemImageFolder}'\n\n");
        }

        // Validate each emulator's location path if it's provided. Allow null or empty.
        foreach (var emulator in selectedConfig.Emulators)
        {
            if (!string.IsNullOrWhiteSpace(emulator.EmulatorLocation) && !IsValidPath(emulator.EmulatorLocation))
            {
                hasErrors = true;
                errorMessages.AppendLine($"Emulator location is not valid for {emulator.EmulatorName}: '{emulator.EmulatorLocation}'\n\n");
            }
        }
            
        // Display all error messages if there are any errors
        if (hasErrors)
        {
            string extraline = "Edit System to fix it.";
            MessageBox.Show(errorMessages + extraline,"Validation Errors", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string fullPath = Path.Combine(basePath, path);

        // Check if the combined path exists
        return Directory.Exists(fullPath) || File.Exists(fullPath);
    }
}