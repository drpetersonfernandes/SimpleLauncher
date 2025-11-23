using System.IO;
using System.Windows;
using Mame.DatCreator.Services;
using Microsoft.Win32;

namespace Mame.DatCreator;

public partial class MainWindow
{
    private readonly WpfLogger _logger;

    public MainWindow()
    {
        InitializeComponent();
        _logger = new WpfLogger(LogTextBox, LogScrollViewer);

        _logger.Info("Welcome to MAME DAT Creator Utility.");
        _logger.Info("This tool will merge the MAME full driver list with software lists to create a unified DAT file.");
        _logger.Info("Please select the required files and folder to begin.\n");
    }

    private void BrowseFullXml_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select MAME Full Driver XML",
            Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            FullXmlPathTextBox.Text = openFileDialog.FileName;
            _logger.Info($"Selected MAME full driver XML: {openFileDialog.FileName}");
        }
    }

    private void BrowseHashFolder_Click(object sender, RoutedEventArgs e)
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Title = "Select MAME Hash Folder"
        };

        if (openFolderDialog.ShowDialog() == true)
        {
            HashFolderPathTextBox.Text = openFolderDialog.FolderName;
            _logger.Info($"Selected hash folder: {openFolderDialog.FolderName}");
        }
    }

    private void BrowseOutputFile_Click(object sender, RoutedEventArgs e)
    {
        var saveFileDialog = new SaveFileDialog
        {
            Title = "Save Merged MAME File",
            Filter = "XML File (*.xml)|*.xml|All Files (*.*)|*.*",
            FileName = "mame.xml"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            OutputPathTextBox.Text = saveFileDialog.FileName;
            _logger.Info($"Output will be saved as: {saveFileDialog.FileName} (and .dat)");
        }
    }

    private async void Process_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(FullXmlPathTextBox.Text))
            {
                _logger.Warning("Please select a MAME full driver XML file.");
                MessageBox.Show("Please select a MAME full driver XML file.", "Missing Input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(HashFolderPathTextBox.Text))
            {
                _logger.Warning("Please select the MAME hash folder.");
                MessageBox.Show("Please select the MAME hash folder.", "Missing Input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(OutputPathTextBox.Text))
            {
                _logger.Warning("Please select an output file location.");
                MessageBox.Show("Please select an output file location.", "Missing Input",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verify files exist
            if (!File.Exists(FullXmlPathTextBox.Text))
            {
                _logger.Error("The selected MAME full driver XML file does not exist.");
                MessageBox.Show("The selected MAME full driver XML file does not exist.", "File Not Found",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(HashFolderPathTextBox.Text))
            {
                _logger.Error("The selected hash folder does not exist.");
                MessageBox.Show("The selected hash folder does not exist.", "Folder Not Found",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Disable UI during processing
            ProcessButton.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;

            try
            {
                _logger.Info("\n========================================");
                _logger.Info("Starting DAT creation process...");
                _logger.Info("========================================\n");

                var logic = new DatCreatorLogic(_logger);
                await logic.CreateMergedDatAsync(
                    FullXmlPathTextBox.Text,
                    HashFolderPathTextBox.Text,
                    OutputPathTextBox.Text);

                _logger.Info("\n========================================");
                _logger.Info("Process completed successfully!");
                _logger.Info("========================================\n");

                MessageBox.Show(
                    $"DAT files created successfully!\n\nXML: {OutputPathTextBox.Text}\nDAT: {Path.ChangeExtension(OutputPathTextBox.Text, ".dat")}",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.Error("A critical error occurred during the process.", ex);
                MessageBox.Show(
                    $"An error occurred during processing:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable UI
                ProcessButton.IsEnabled = true;
                ProgressBar.IsIndeterminate = false;
                ProgressBar.Visibility = Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("An unexpected error occurred.", ex);
        }
    }
}
