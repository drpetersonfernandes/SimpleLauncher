using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace CreateBatchFilesForWindowsGames;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        LogMessage("Welcome to the Batch File Creator for Microsoft Windows Games.");
        LogMessage("");
        LogMessage("This program creates batch files to launch your Microsoft Windows games.");
        LogMessage("Please follow these steps:");
        LogMessage("1. Select the game executable file (.exe)");
        LogMessage("2. Choose where to save the batch file");
        LogMessage("3. Click 'Create Batch File' to generate the batch file");
        LogMessage("");

        // Initially disable the Create Batch File button until paths are selected
        CreateBatchFileButton.IsEnabled = false;
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

    private void BrowseGameExeButton_Click(object sender, RoutedEventArgs e)
    {
        var gameExePath = SelectGameExecutable();
        if (!string.IsNullOrEmpty(gameExePath))
        {
            GameExePathTextBox.Text = gameExePath;
            LogMessage($"Game executable selected: {gameExePath}");

            // Update the Save As button status
            UpdateCreateButtonStatus();
        }
    }

    private void SaveBatchFileButton_Click(object sender, RoutedEventArgs e)
    {
        var gameExePath = GameExePathTextBox.Text;
        if (string.IsNullOrEmpty(gameExePath))
        {
            ShowError("Please select a game executable first.");
            return;
        }

        var gameFolderPath = Path.GetDirectoryName(gameExePath) ?? "";
        var folderName = Path.GetFileName(gameFolderPath.TrimEnd(Path.DirectorySeparatorChar));

        var batchFilePath = SaveBatchFile(folderName);
        if (!string.IsNullOrEmpty(batchFilePath))
        {
            BatchFilePathTextBox.Text = batchFilePath;
            LogMessage($"Batch file location selected: {batchFilePath}");

            // Update the Create button status
            UpdateCreateButtonStatus();
        }
    }

    private void CreateBatchFileButton_Click(object sender, RoutedEventArgs e)
    {
        var gameExePath = GameExePathTextBox.Text;
        var batchFilePath = BatchFilePathTextBox.Text;

        if (string.IsNullOrEmpty(gameExePath))
        {
            LogMessage("Error: No game executable selected.");
            ShowError("Please select a game executable file.");
            return;
        }

        if (string.IsNullOrEmpty(batchFilePath))
        {
            LogMessage("Error: No batch file location selected.");
            ShowError("Please select where to save the batch file.");
            return;
        }

        // Create the batch file
        try
        {
            var gameFolderPath = Path.GetDirectoryName(gameExePath) ?? "";
            var gameFileName = Path.GetFileName(gameExePath);

            using (StreamWriter sw = new(batchFilePath))
            {
                sw.WriteLine("@echo off");
                sw.WriteLine($"cd /d \"{gameFolderPath}\"");
                sw.WriteLine($"start {gameFileName}");
            }

            LogMessage("");
            LogMessage($"Batch file '{Path.GetFileName(batchFilePath)}' has been successfully created.");

            ShowMessageBox($"Batch file '{Path.GetFileName(batchFilePath)}' has been successfully created.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Show the "Create Another" button and hide the "Create" button
            CreateBatchFileButton.Visibility = Visibility.Collapsed;
            CreateAnotherButton.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            LogMessage($"Error creating batch file: {ex.Message}");
            ShowError($"Error creating batch file: {ex.Message}");
        }
    }

    private void CreateAnotherButton_Click(object sender, RoutedEventArgs e)
    {
        // Reset the UI for creating another batch file
        GameExePathTextBox.Text = "";
        BatchFilePathTextBox.Text = "";

        // Hide the "Create Another" button and show the "Create" button
        CreateAnotherButton.Visibility = Visibility.Collapsed;
        CreateBatchFileButton.Visibility = Visibility.Visible;
        CreateBatchFileButton.IsEnabled = false;

        LogMessage("");
        LogMessage("Ready to create another batch file.");
        LogMessage("");
    }

    private void UpdateCreateButtonStatus()
    {
        // Enable the Create button only if both paths are selected
        CreateBatchFileButton.IsEnabled = !string.IsNullOrEmpty(GameExePathTextBox.Text) &&
                                          !string.IsNullOrEmpty(BatchFilePathTextBox.Text);
    }

    private string? SelectGameExecutable()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Please select the game executable file (e.g., game.exe)",
            Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*",
            RestoreDirectory = true
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private string? SaveBatchFile(string suggestedName)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save Batch File",
            Filter = "Batch files (*.bat)|*.bat",
            DefaultExt = "bat",
            AddExtension = true,
            FileName = $"{suggestedName}.bat"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
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