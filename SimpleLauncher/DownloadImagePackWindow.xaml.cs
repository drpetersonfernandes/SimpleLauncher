using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class DownloadImagePackWindow : IDisposable
{
    private EasyModeManager _manager;
    private readonly DownloadManager _downloadManager;
    private bool _disposed;

    public DownloadImagePackWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        // Initialize the DownloadManager
        _downloadManager = new DownloadManager();
        _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;

        // Load Config
        _manager = EasyModeManager.Load();
        PopulateSystemDropdown();

        // Set up event handlers
        Closed += CloseWindowRoutine;
    }

    private void PopulateSystemDropdown()
    {
        if (_manager?.Systems == null) return;

        // Filter systems that have a valid ExtrasDownloadLink
        var systemsWithImagePacks = _manager.Systems
            .Where(system => !string.IsNullOrEmpty(system.Emulators.Emulator.ExtrasDownloadLink))
            .Select(system => system.SystemName)
            .OrderBy(name => name) // Order by system name
            .ToList();

        SystemNameDropdown.ItemsSource = systemsWithImagePacks;
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null) return;

        var selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem != null)
        {
            DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadLink);

            // Automatically populate the extraction path with default if available
            if (!string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath))
            {
                ExtractionFolderTextBox.Text = selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;
            }
        }
    }

    private async void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        // Enable the Stop Button
        StopDownloadButton.IsEnabled = true;

        // Input validation
        if (!ValidateInputs(out var selectedSystem)) return;

        try
        {
            // Get the download URL
            var extrasDownloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;

            // Determine the extraction folder
            var extractionFolder = !string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text)
                ? ExtractionFolderTextBox.Text
                : selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;

            // Update UI elements
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StopDownloadButton.IsEnabled = true;
            DownloadExtrasButton.IsEnabled = false;

            // Show the PleaseWaitExtraction window for extraction
            // We'll use the DownloadManager to handle the download and extraction
            var downloadSuccess = await _downloadManager.DownloadFileAsync(extrasDownloadUrl);

            if (downloadSuccess != null && _downloadManager.IsDownloadCompleted)
            {
                var downloadcompleteStartingextractionto2 = (string)Application.Current.TryFindResource("DownloadcompleteStartingextractionto") ?? "Download complete. Starting extraction to";
                UpdateStatus($"{downloadcompleteStartingextractionto2} {extractionFolder}...");

                // Show the PleaseWaitExtraction window
                var pleaseWaitWindow = new PleaseWaitExtractionWindow();
                pleaseWaitWindow.Show();

                var extractionSuccess = await _downloadManager.ExtractFileAsync(downloadSuccess, extractionFolder);

                // Close the PleaseWaitExtraction window
                pleaseWaitWindow.Close();

                if (extractionSuccess)
                {
                    // Notify user
                    MessageBoxLibrary.DownloadExtractionSuccessfullyMessageBox();

                    var imagepackdownloadedandextractedsuccessfully2 = (string)Application.Current.TryFindResource("Imagepackdownloadedandextractedsuccessfully") ?? "Image pack downloaded and extracted successfully.";
                    UpdateStatus(imagepackdownloadedandextractedsuccessfully2);

                    // Mark as downloaded and disable button
                    DownloadExtrasButton.IsEnabled = false;
                }
                else // Extraction fail
                {
                    // Notify developer
                    var contextMessage = $"Image Pack extraction failed.\n" +
                                         $"File: {extrasDownloadUrl}";
                    var ex = new Exception(contextMessage);
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ExtractionFailedMessageBox();

                    var extractionfailedSeeerrormessagefordetails2 = (string)Application.Current.TryFindResource("ExtractionfailedSeeerrormessagefordetails") ?? "Extraction failed. See error message for details.";
                    UpdateStatus(extractionfailedSeeerrormessagefordetails2);
                        
                    // Re-enable download button
                    DownloadExtrasButton.IsEnabled = true;
                }
            }
            else if (_downloadManager.IsUserCancellation)
            {
                var downloadcanceled2 = (string)Application.Current.TryFindResource("Downloadcanceled") ?? "Download canceled";
                UpdateStatus(downloadcanceled2);
                    
                // Re-enable download button
                DownloadExtrasButton.IsEnabled = true;
            }
            else
            {
                var downloadwasnotcompletedsuccessfully2 = (string)Application.Current.TryFindResource("Downloadwasnotcompletedsuccessfully") ?? "Download was not completed successfully.";
                UpdateStatus(downloadwasnotcompletedsuccessfully2);
                    
                // Notify user
                MessageBoxLibrary.ImagePackDownloadErrorOfferRedirectMessageBox(selectedSystem);
                    
                // Re-enable download button
                DownloadExtrasButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error downloading the Image Pack.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ImagePackDownloadExtractionFailedMessageBox();

            var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            UpdateStatus($"{error2}: {ex.Message}");
            DownloadExtrasButton.IsEnabled = true;
        }
        finally
        {
            StopDownloadButton.IsEnabled = false;
        }
    }

    private bool ValidateInputs(out EasyModeSystemConfig selectedSystem)
    {
        selectedSystem = null;

        // Check if a system is selected
        if (SystemNameDropdown.SelectedItem == null)
        {
            MessageBoxLibrary.SystemNameIsNullMessageBox();
            return false;
        }

        // Get the selected system
        selectedSystem = _manager.Systems.FirstOrDefault(
            system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());

        if (selectedSystem == null)
        {
            MessageBoxLibrary.SelectedSystemIsNullMessageBox();
            return false;
        }

        // Validate download URL
        var downloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;
        if (string.IsNullOrEmpty(downloadUrl))
        {
            MessageBoxLibrary.DownloadUrlIsNullMessageBox();
            return false;
        }

        // Validate extraction folder
        if (string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text) &&
            string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath))
        {
            MessageBoxLibrary.ExtractionFolderIsNullMessageBox();
            return false;
        }

        var extractionFolder = !string.IsNullOrWhiteSpace(ExtractionFolderTextBox.Text)
            ? ExtractionFolderTextBox.Text
            : selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;

        // Verify the extraction folder exists or can be created
        try
        {
            if (!Directory.Exists(extractionFolder))
            {
                Directory.CreateDirectory(extractionFolder);
            }
        }
        catch (Exception ex)
        {
            MessageBoxLibrary.ExtractionFolderCannotBeCreatedMessageBox(ex);
            return false;
        }

        return true;
    }

    private void DownloadManager_ProgressChanged(object sender, DownloadManager.DownloadProgressEventArgs e)
    {
        // Update the progress bar
        DownloadProgressBar.Value = e.ProgressPercentage;
        
        // Update status text
        UpdateStatus(e.StatusMessage);
    }

    private void UpdateStatus(string message)
    {
        // Update the status text block
        StatusTextBlock.Text = message;
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        // Cancel the ongoing download
        _downloadManager.CancelDownload();

        // Disable the stop button
        StopDownloadButton.IsEnabled = false;

        // Reset progress
        DownloadProgressBar.Value = 0;
        var downloadcanceled2 = (string)Application.Current.TryFindResource("Downloadcanceled") ?? "Download canceled";
        UpdateStatus(downloadcanceled2);

        // Enable download button again
        DownloadExtrasButton.IsEnabled = true;
    }

    private void CloseWindowRoutine(object sender, EventArgs e)
    {
        // Empty EasyMode Config
        _manager = null;
        Dispose();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    private void ChooseExtractionFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var selectafoldertoextracttheImagePack2 = (string)Application.Current.TryFindResource("SelectafoldertoextracttheImagePack") ?? "Select a folder to extract the Image Pack";

        // Create a new StorageFolder picker
        var openFolderDialog = new OpenFolderDialog
        {
            Title = selectafoldertoextracttheImagePack2
        };

        // Show the dialog and handle the result
        if (openFolderDialog.ShowDialog() == true)
        {
            ExtractionFolderTextBox.Text = openFolderDialog.FolderName;
        }
    }

    // IDisposable implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _downloadManager?.Dispose();
        }

        _disposed = true;
    }

    ~DownloadImagePackWindow()
    {
        Dispose(false);
    }
}