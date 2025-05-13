using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
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

    /// Populates the system dropdown menu with a list of system names for which
    /// image packs are available. The system names are sourced from the associated
    /// EasyModeManager instance, filtered to include only systems with a valid
    /// ExtrasDownloadLink, and sorted alphabetically.
    /// The method retrieves the list of systems from the EasyModeManager instance.
    /// If the list is null or not present, no action is performed. Otherwise, the
    /// systems are filtered to include only those with a non-empty ExtrasDownloadLink
    /// property. The resulting names are then set as the items source for the
    /// SystemNameDropdown control, which allows the user to select a system.
    /// This method indirectly relies on the initialization of EasyModeManager to
    /// ensure the systems are correctly loaded before it is called.
    private void PopulateSystemDropdown()
    {
        // if (_manager?.Systems == null) return;

        // Filter systems that have a valid ExtrasDownloadLink
        var systemsWithImagePacks = _manager.Systems
            .Where(static system => !string.IsNullOrEmpty(system.Emulators.Emulator.ExtrasDownloadLink))
            .Select(static system => system.SystemName)
            .OrderBy(static name => name) // Order by system name
            .ToList();

        SystemNameDropdown.ItemsSource = systemsWithImagePacks;
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null) return;

        var selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem == null) return;

        DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadLink);
    }

    private async void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        try
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
                var extractionFolder = selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;

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
                    var extracting2 = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                    var pleaseWaitWindow = new PleaseWaitWindow($"{extracting2}...")
                    {
                        Owner = this
                    };
                    pleaseWaitWindow.Show();

                    var extractionSuccess = await _downloadManager.ExtractFileAsync(downloadSuccess, extractionFolder);

                    // Close the PleaseWaitExtraction window
                    pleaseWaitWindow.Close();

                    if (extractionSuccess)
                    {
                        // Notify user
                        MessageBoxLibrary.DownloadExtractionSuccessfullyMessageBox(extractionFolder);

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
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error downloading the Image Pack.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
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
        selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());

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

        string extractionFolder;

        if (string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath))
        {
            // Automatically populate the extraction path with a default path
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var systemName = selectedSystem.SystemName;
            extractionFolder = Path.Combine(appPath, "images", systemName);
        }
        else
        {
            extractionFolder = selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;
        }

        // Verify the extraction folder exists or can be created
        try
        {
            if (!Directory.Exists(extractionFolder))
            {
                IoOperations.CreateDirectory(extractionFolder);
            }
        }
        catch (Exception ex)
        {
            MessageBoxLibrary.ExtractionFolderCannotBeCreatedMessageBox(ex);
            return false;
        }

        return true;
    }

    private void DownloadManager_ProgressChanged(object sender, DownloadProgressEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
            {
                DownloadProgressBar.Value = e.ProgressPercentage;
                UpdateStatus(e.StatusMessage);
            }
        );
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

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    // IDisposable implementation
    public void Dispose()
    {
        Dispose(true);

        // Tell GC not to call the finalizer since we've already cleaned up
        GC.SuppressFinalize(this);
    }

    private void CloseWindowRoutine(object sender, EventArgs e)
    {
        // Empty EasyMode Config
        _manager = null;
        Dispose();
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