using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using Application = System.Windows.Application;
using Microsoft.Extensions.DependencyInjection;

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

        // Get the factory from the service provider
        var httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        // Initialize the DownloadManager, passing the factory
        _downloadManager = new DownloadManager(httpClientFactory);
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
    /// ImagePackDownloadLink, and sorted alphabetically.
    /// The method retrieves the list of systems from the EasyModeManager instance.
    /// If the list is null or not present, no action is performed. Otherwise, the
    /// systems are filtered to include only those with a non-empty ImagePackDownloadLink
    /// property. The resulting names are then set as the items source for the
    /// SystemNameDropdown control, which allows the user to select a system.
    /// This method indirectly relies on the initialization of EasyModeManager to
    /// ensure the systems are correctly loaded before it is called.
    private void PopulateSystemDropdown()
    {
        try
        {
            if (_manager?.Systems == null)
            {
                SystemNameDropdown.ItemsSource = new List<string>(); // return an empty list
                return;
            }

            // Filter systems that have a valid ImagePackDownloadLink
            var systemsWithImagePacks = _manager.Systems
                .Where(static system => !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink))
                .Select(static system => system.SystemName)
                .OrderBy(static name => name) // Order by system name
                .ToList();

            SystemNameDropdown.ItemsSource = systemsWithImagePacks;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error populating system dropdown.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Assign an empty list if there's any error
            SystemNameDropdown.ItemsSource = new List<string>();
        }
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null) return;

        var selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem == null) return;

        DownloadImagePackButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink);
    }

    private async void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Enable the Stop Button
            StopDownloadButton.IsEnabled = true;

            // Input validation
            if (!ValidateInputsAndCreateExtractionFolder(out var selectedSystem)) return;

            try
            {
                // Get the download URL
                var imagePackDownloadUrl = selectedSystem.Emulators.Emulator.ImagePackDownloadLink;

                // Determine the extraction folder
                var imagePackDownloadExtractPath = selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath;
                // var fixedImagePackDownloadExtractPath = imagePackDownloadExtractPath.Replace("%BASEFOLDER%", _basePath);
                // var finalImagePackDownloadExtractPath = Path.GetFullPath(fixedImagePackDownloadExtractPath);

                var finalImagePackDownloadExtractPath = PathHelper.ResolveRelativeToAppDirectory(imagePackDownloadExtractPath);

                // Update UI elements
                DownloadProgressBar.Visibility = Visibility.Visible;
                DownloadProgressBar.Value = 0;
                StopDownloadButton.IsEnabled = true;
                DownloadImagePackButton.IsEnabled = false;

                // We'll use the DownloadManager to handle the download and extraction
                var downloadSuccess = await _downloadManager.DownloadFileAsync(imagePackDownloadUrl);

                if (downloadSuccess != null && _downloadManager.IsDownloadCompleted)
                {
                    var downloadcompleteStartingextractionto2 = (string)Application.Current.TryFindResource("DownloadcompleteStartingextractionto") ?? "Download complete. Starting extraction to";
                    UpdateStatus($"{downloadcompleteStartingextractionto2} {finalImagePackDownloadExtractPath}...");

                    var extracting2 = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                    LoadingMessage.Text = $"{extracting2}...";
                    LoadingOverlay.Visibility = Visibility.Visible;

                    var extractionSuccess = await _downloadManager.ExtractFileAsync(downloadSuccess, finalImagePackDownloadExtractPath);

                    LoadingOverlay.Visibility = Visibility.Collapsed;

                    if (extractionSuccess)
                    {
                        // Notify user
                        MessageBoxLibrary.DownloadExtractionSuccessfullyMessageBox(finalImagePackDownloadExtractPath);

                        var imagepackdownloadedandextractedsuccessfully2 = (string)Application.Current.TryFindResource("Imagepackdownloadedandextractedsuccessfully") ?? "Image pack downloaded and extracted successfully.";
                        UpdateStatus(imagepackdownloadedandextractedsuccessfully2);

                        // Mark as downloaded and disable button
                        DownloadImagePackButton.IsEnabled = false;
                    }
                    else // Extraction fail
                    {
                        // Notify developer
                        var contextMessage = $"Image Pack extraction failed.\n" +
                                             $"File: {imagePackDownloadUrl}";
                        _ = LogErrors.LogErrorAsync(null, contextMessage);

                        // Notify user
                        MessageBoxLibrary.ExtractionFailedMessageBox();

                        var extractionfailedSeeerrormessagefordetails2 = (string)Application.Current.TryFindResource("ExtractionfailedSeeerrormessagefordetails") ?? "Extraction failed. See error message for details.";
                        UpdateStatus(extractionfailedSeeerrormessagefordetails2);

                        // Re-enable download button
                        DownloadImagePackButton.IsEnabled = true;
                    }
                }
                else if (_downloadManager.IsUserCancellation)
                {
                    var downloadcanceled2 = (string)Application.Current.TryFindResource("Downloadcanceled") ?? "Download canceled";
                    UpdateStatus(downloadcanceled2);

                    // Re-enable download button
                    DownloadImagePackButton.IsEnabled = true;
                }
                else
                {
                    var downloadwasnotcompletedsuccessfully2 = (string)Application.Current.TryFindResource("Downloadwasnotcompletedsuccessfully") ?? "Download was not completed successfully.";
                    UpdateStatus(downloadwasnotcompletedsuccessfully2);

                    // Notify user
                    MessageBoxLibrary.ImagePackDownloadErrorOfferRedirectMessageBox(selectedSystem);

                    // Re-enable download button
                    DownloadImagePackButton.IsEnabled = true;
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
                DownloadImagePackButton.IsEnabled = true;
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

    private bool ValidateInputsAndCreateExtractionFolder(out EasyModeSystemConfig selectedSystem)
    {
        selectedSystem = null;

        // Check if a system is selected
        if (SystemNameDropdown.SelectedItem == null)
        {
            // Notify user
            MessageBoxLibrary.SystemNameIsNullMessageBox();
            return false;
        }

        // Get the selected system
        selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());

        if (selectedSystem == null)
        {
            // Notify user
            MessageBoxLibrary.SelectedSystemIsNullMessageBox();
            return false;
        }

        // Validate download URL
        var downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink;
        if (string.IsNullOrEmpty(downloadUrl))
        {
            // Notify user
            MessageBoxLibrary.DownloadUrlIsNullMessageBox();
            return false;
        }

        if (selectedSystem.Emulators is { Emulator: not null })
        {
            var imagePackDownloadExtractPath = selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath;
            // var fixedImagePackDownloadExtractPath = imagePackDownloadExtractPath.Replace("%BASEFOLDER%", _basePath);
            // var finalImagePackDownloadExtractPath = Path.GetFullPath(fixedImagePackDownloadExtractPath);

            var finalImagePackDownloadExtractPath = PathHelper.ResolveRelativeToAppDirectory(imagePackDownloadExtractPath);

            // Verify the extraction folder exists or can be created
            if (!CreateExtractionFolder(finalImagePackDownloadExtractPath)) return false;
        }

        return true;
    }

    private static bool CreateExtractionFolder(string extractionFolder)
    {
        try
        {
            if (!Directory.Exists(extractionFolder))
            {
                try
                {
                    if (extractionFolder != null) Directory.CreateDirectory(extractionFolder);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, "Error creating the extraction folder.");
                }
            }
        }
        catch (Exception ex)
        {
            // Notify user
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
        _downloadManager.CancelDownload();
        StopDownloadButton.IsEnabled = false;
        DownloadProgressBar.Value = 0;

        var downloadcanceled2 = (string)Application.Current.TryFindResource("Downloadcanceled") ?? "Download canceled";
        UpdateStatus(downloadcanceled2);

        DownloadImagePackButton.IsEnabled = true;
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error opening the download link.");

            // Notify user
            MessageBoxLibrary.CouldNotOpenTheDownloadLink();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async void CloseWindowRoutine(object sender, EventArgs e)
    {
        try
        {
            if (StopDownloadButton.IsEnabled)
            {
                StopDownloadButton_Click(null, null);
                await Task.Delay(200);
            }

            _manager = null;
            Dispose();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error closing the Add System window.");
        }
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
