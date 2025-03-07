using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace CreateBatchFilesForSegaModel3Games;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        LogMessage("Welcome to the Batch File Creator for Sega Model 3 Games.");
        LogMessage("");
        LogMessage("This program creates batch files to launch your Sega Model 3 games.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the Supermodel emulator executable file (Supermodel.exe)");
        LogMessage("2. Select the folder containing your Sega Model 3 ROM zip files");
        LogMessage("3. Click 'Create Batch Files' to generate the batch files");
        LogMessage("");
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    private void LogMessage(string message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogTextBox.AppendText(message + Environment.NewLine);
            LogTextBox.ScrollToEnd();
        });
    }

    private void BrowseSupermodelButton_Click(object sender, RoutedEventArgs e)
    {
        var supermodelExePath = SelectFile();
        if (!string.IsNullOrEmpty(supermodelExePath))
        {
            SupermodelPathTextBox.Text = supermodelExePath;
            LogMessage($"Supermodel executable selected: {supermodelExePath}");
        }
    }

    private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var romFolder = SelectFolder();
        if (!string.IsNullOrEmpty(romFolder))
        {
            RomFolderTextBox.Text = romFolder;
            LogMessage($"ROM folder selected: {romFolder}");
        }
    }

    private void CreateBatchFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var supermodelExePath = SupermodelPathTextBox.Text;
        var romFolder = RomFolderTextBox.Text;

        if (string.IsNullOrEmpty(supermodelExePath))
        {
            LogMessage("Error: No Supermodel executable selected.");
            ShowError("Please select the Supermodel executable file (Supermodel.exe).");
            return;
        }

        if (string.IsNullOrEmpty(romFolder))
        {
            LogMessage("Error: No ROM folder selected.");
            ShowError("Please select the folder containing your Sega Model 3 ROM zip files.");
            return;
        }

        CreateBatchFilesForModel3Games(romFolder, supermodelExePath);
    }

    private static string? SelectFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Please select the folder where your Sega Model 3 ROM zip files are located."
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private string? SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Please select the Supermodel executable file (Supermodel.exe)",
            Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void CreateBatchFilesForModel3Games(string romFolder, string supermodelExePath)
    {
        var romFiles = Directory.GetFiles(romFolder, "*.zip");
        var filesCreated = 0;

        LogMessage("");
        LogMessage("Starting batch file creation process...");
        LogMessage("");

        foreach (var romFilePath in romFiles)
        {
            var romFileName = Path.GetFileNameWithoutExtension(romFilePath);
            var batchFilePath = Path.Combine(romFolder, romFileName + ".bat");

            using (StreamWriter sw = new(batchFilePath))
            {
                sw.WriteLine($"cd /d \"{Path.GetDirectoryName(supermodelExePath)}\"");
                sw.WriteLine($"\"{supermodelExePath}\" \"{romFilePath}\" -fullscreen -show-fps");

                LogMessage($"Batch file created: {batchFilePath}");
            }

            filesCreated++;
        }

        if (filesCreated > 0)
        {
            LogMessage("");
            LogMessage($"{filesCreated} batch files have been successfully created.");
            LogMessage("They are located in the same folder as your ROM zip files.");

            ShowMessageBox($"{filesCreated} batch files have been successfully created.\n\n" +
                           "They are located in the same folder as your ROM zip files.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            LogMessage("No ROM zip files found. No batch files were created.");
            ShowError("No ROM zip files found. No batch files were created.");
        }
    }

    private void ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
    {
        Dispatcher.Invoke(() =>
            MessageBox.Show(message, title, buttons, icon));
    }

    private void ShowError(string message)
    {
        ShowMessageBox(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}