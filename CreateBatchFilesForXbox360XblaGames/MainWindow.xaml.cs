using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace CreateBatchFilesForXbox360XBLAGames;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        LogMessage("Welcome to the Batch File Creator for Xbox 360 XBLA Games.");
        LogMessage("");
        LogMessage("This program creates batch files to launch your Xbox 360 XBLA games.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the Xenia executable file (xenia.exe)");
        LogMessage("2. Select the root folder containing your Xbox 360 XBLA game folders");
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

    private void BrowseXeniaButton_Click(object sender, RoutedEventArgs e)
    {
        var xeniaExePath = SelectFile();
        if (!string.IsNullOrEmpty(xeniaExePath))
        {
            XeniaPathTextBox.Text = xeniaExePath;
            LogMessage($"Xenia executable selected: {xeniaExePath}");
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
        var xeniaExePath = XeniaPathTextBox.Text;
        var rootFolder = GameFolderTextBox.Text;

        if (string.IsNullOrEmpty(xeniaExePath))
        {
            LogMessage("Error: No Xenia executable selected.");
            ShowError("Please select the Xenia executable file (xenia.exe).");
            return;
        }

        if (string.IsNullOrEmpty(rootFolder))
        {
            LogMessage("Error: No game folder selected.");
            ShowError("Please select the root folder containing your Xbox 360 XBLA game folders.");
            return;
        }

        CreateBatchFilesForXboxXblaGames(rootFolder, xeniaExePath);
    }

    private static string? SelectFolder()
    {
        var fbd = new OpenFolderDialog
        {
            Title = "Please select the root folder where your Xbox 360 XBLA game folders are located."
        };

        return fbd.ShowDialog() == true ? fbd.FolderName : null;
    }

    private string? SelectFile()
    {
        var ofd = new OpenFileDialog
        {
            Title = "Please select the Xenia executable file (xenia.exe)",
            Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        return ofd.ShowDialog() == true ? ofd.FileName : null;
    }

    private void CreateBatchFilesForXboxXblaGames(string rootFolder, string xeniaExePath)
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

            var gameFilePath = FindGameFile(gameDirectory);

            if (string.IsNullOrEmpty(gameFilePath))
            {
                LogMessage($"No game file found in {gameFolderName}. Skipping...");
                continue;
            }

            using (StreamWriter sw = new(batchFilePath))
            {
                sw.WriteLine($"\"{xeniaExePath}\" \"{gameFilePath}\"");
                LogMessage($"Batch file created: {batchFilePath}");
            }

            filesCreated++;
        }

        if (filesCreated > 0)
        {
            LogMessage("");
            LogMessage($"{filesCreated} batch files have been successfully created.");
            LogMessage("They are located in the root folder of your Xbox 360 XBLA games.");

            ShowMessageBox($"{filesCreated} batch files have been successfully created.\n\n" +
                           "They are located in the root folder of your Xbox 360 XBLA games.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            LogMessage("No valid game folders found. No batch files were created.");
            ShowError("No valid game folders found. No batch files were created.");
        }
    }

    private string? FindGameFile(string gameDirectory)
    {
        try
        {
            var directories = Directory.GetDirectories(gameDirectory, "000D0000", SearchOption.AllDirectories);

            if (directories.Length > 0)
            {
                var files = Directory.GetFiles(directories[0]);

                if (files.Length > 0)
                {
                    return files[0];
                }
            }
        }
        catch (Exception ex)
        {
            LogMessage($"Error finding game file in {gameDirectory}: {ex.Message}");
        }

        return null;
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