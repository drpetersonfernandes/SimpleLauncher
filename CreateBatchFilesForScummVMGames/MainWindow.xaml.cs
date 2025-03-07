using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace CreateBatchFilesForScummVMGames;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        LogMessage("Welcome to the Batch File Creator for ScummVM Games.");
        LogMessage("");
        LogMessage("This program creates batch files to launch your ScummVM games.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the ScummVM executable file (scummvm.exe)");
        LogMessage("2. Select the root folder containing your ScummVM game folders");
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

    private void BrowseScummVMButton_Click(object sender, RoutedEventArgs e)
    {
        var scummvmExePath = SelectFile();
        if (!string.IsNullOrEmpty(scummvmExePath))
        {
            ScummVmPathTextBox.Text = scummvmExePath;
            LogMessage($"ScummVM executable selected: {scummvmExePath}");
        }
    }

    private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var rootFolder = SelectFolder();
        if (!string.IsNullOrEmpty(rootFolder))
        {
            GameFolderTextBox.Text = rootFolder;
            LogMessage($"Game folder selected: {rootFolder}");
        }
    }

    private void CreateBatchFilesButton_Click(object sender, RoutedEventArgs e)
    {
        var scummvmExePath = ScummVmPathTextBox.Text;
        var rootFolder = GameFolderTextBox.Text;

        if (string.IsNullOrEmpty(scummvmExePath))
        {
            LogMessage("Error: No ScummVM executable selected.");
            ShowError("Please select the ScummVM executable file (scummvm.exe).");
            return;
        }

        if (string.IsNullOrEmpty(rootFolder))
        {
            LogMessage("Error: No game folder selected.");
            ShowError("Please select the root folder containing your ScummVM game folders.");
            return;
        }

        CreateBatchFilesForScummVmGames(rootFolder, scummvmExePath);
    }

    private static string? SelectFolder()
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Please select the root folder where your ScummVM game folders are located."
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private string? SelectFile()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Please select the ScummVM executable file (scummvm.exe)",
            Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private void CreateBatchFilesForScummVmGames(string rootFolder, string scummvmExePath)
    {
        var gameDirectories = Directory.GetDirectories(rootFolder);
        var filesCreated = 0;

        LogMessage("");
        LogMessage("Starting batch file creation process...");
        LogMessage("");

        foreach (var gameDirectory in gameDirectories)
        {
            var gameFolderName = Path.GetFileName(gameDirectory);
            var batchFilePath = Path.Combine(rootFolder, gameFolderName + ".bat");

            using (StreamWriter sw = new(batchFilePath))
            {
                sw.WriteLine($"\"{scummvmExePath}\" -p \"{gameDirectory}\" --auto-detect --fullscreen");
                LogMessage($"Batch file created: {batchFilePath}");
            }

            filesCreated++;
        }

        if (filesCreated > 0)
        {
            LogMessage("");
            LogMessage($"{filesCreated} batch files have been successfully created.");
            LogMessage("They are located in the root folder of your ScummVM games.");

            ShowMessageBox($"{filesCreated} batch files have been successfully created.\n\n" +
                           "They are located in the root folder of your ScummVM games.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            LogMessage("No game folders found. No batch files were created.");
            ShowError("No game folders found. No batch files were created.");
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