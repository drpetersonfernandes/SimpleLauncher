using System.IO;
using System.Linq;
using System.Windows;

namespace SimpleLauncher;

public static class CheckSystem
{
    public static void ValidateSystemConfiguration(string systemFolderTextBox, string systemImageFolderTextBox, string emulator1LocationTextBox, string emulator2LocationTextBox, string emulator3LocationTextBox, string emulator4LocationTextBox, string emulator5LocationTextBox)
    {
        
        // Check if System Folder exists
        if (!Directory.Exists(systemFolderTextBox))
        {
            MessageBox.Show($"The system folder '{systemFolderTextBox}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    
        // Check if System Image Folder exists (only if it's not empty, as it's optional)
        if (!string.IsNullOrWhiteSpace(systemImageFolderTextBox) && !Directory.Exists(systemImageFolderTextBox))
        {
            MessageBox.Show($"The system image folder '{systemImageFolderTextBox}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // Check Emulator 1 File exists (example for Emulator 1, repeat for others as necessary)
        if (!string.IsNullOrWhiteSpace(emulator1LocationTextBox) && !File.Exists(emulator1LocationTextBox))
        {
            MessageBox.Show($"The emulator file '{emulator1LocationTextBox}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        
        // Check Emulator 2 File exists
        if (!string.IsNullOrWhiteSpace(emulator2LocationTextBox) && !File.Exists(emulator2LocationTextBox))
        {
            MessageBox.Show($"The emulator file '{emulator2LocationTextBox}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        
        // Check Emulator 3 File exists
        if (!string.IsNullOrWhiteSpace(emulator3LocationTextBox) && !File.Exists(emulator3LocationTextBox))
        {
            MessageBox.Show($"The emulator file '{emulator3LocationTextBox}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // Check Emulator 4 File exists
        if (!string.IsNullOrWhiteSpace(emulator4LocationTextBox) && !File.Exists(emulator4LocationTextBox))
        {
            MessageBox.Show($"The emulator file '{emulator4LocationTextBox}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        
        // Check Emulator 5 File exists
        if (!string.IsNullOrWhiteSpace(emulator5LocationTextBox) && !File.Exists(emulator5LocationTextBox))
        {
            MessageBox.Show($"The emulator file '{emulator5LocationTextBox}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

    }
    
    public static void ValidateSystemConfiguration2(string systemFolder, string systemImageFolder, params string[] emulatorLocations)
    {
        // Check if System Folder exists
        if (!Directory.Exists(systemFolder))
        {
            MessageBox.Show($"The system folder '{systemFolder}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // Check if System Image Folder exists (only if it's not empty, as it's optional)
        if (!string.IsNullOrWhiteSpace(systemImageFolder) && !Directory.Exists(systemImageFolder))
        {
            MessageBox.Show($"The system image folder '{systemImageFolder}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // Check each emulator's file existence
        foreach (var emulatorLocation in emulatorLocations.Where(location => !string.IsNullOrWhiteSpace(location)))
        {
            if (!File.Exists(emulatorLocation))
            {
                MessageBox.Show($"The emulator file '{emulatorLocation}' does not exist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}