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
    private readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_user.log");

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

            // Filter systems that have at least one valid ImagePackDownloadLink
            var systemsWithImagePacks = _manager.Systems
                .Where(static system =>
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink) ||
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink2) ||
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink3) ||
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink4) ||
                    !string.IsNullOrEmpty(system.Emulators?.Emulator?.ImagePackDownloadLink5))
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

        var selectedSystem = GetSelectedSystem(); // Using the new helper method
        if (selectedSystem == null) return;

        // Enable/disable Image Pack buttons based on their respective links
        DownloadImagePackButton1.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink);
        DownloadImagePackButton2.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2);
        DownloadImagePackButton3.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3);
        DownloadImagePackButton4.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4);
        DownloadImagePackButton5.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5);
    }

    // Renamed existing button to DownloadImagePackButton1_Click
    private async void DownloadImagePackButton1_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton1.IsEnabled = false;
            await HandleDownloadAndExtractComponent(DownloadType.ImagePack1, DownloadImagePackButton1);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton1_Click.");
            DownloadImagePackButton1.IsEnabled = true;
        }
    }

    // ADDED: New click handlers for Image Pack 2-5
    private async void DownloadImagePackButton2_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton2.IsEnabled = false;
            await HandleDownloadAndExtractComponent(DownloadType.ImagePack2, DownloadImagePackButton2);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton2_Click.");
            DownloadImagePackButton2.IsEnabled = true;
        }
    }

    private async void DownloadImagePackButton3_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton3.IsEnabled = false;
            await HandleDownloadAndExtractComponent(DownloadType.ImagePack3, DownloadImagePackButton3);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton3_Click.");
            DownloadImagePackButton3.IsEnabled = true;
        }
    }

    private async void DownloadImagePackButton4_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton4.IsEnabled = false;
            await HandleDownloadAndExtractComponent(DownloadType.ImagePack4, DownloadImagePackButton4);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton4_Click.");
            DownloadImagePackButton4.IsEnabled = true;
        }
    }

    private async void DownloadImagePackButton5_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton5.IsEnabled = false;
            await HandleDownloadAndExtractComponent(DownloadType.ImagePack5, DownloadImagePackButton5);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton5_Click.");
            DownloadImagePackButton5.IsEnabled = true;
        }
    }

    // MODIFIED: Changed return type from Task<bool> to Task
    private async Task HandleDownloadAndExtractComponent(DownloadType type, Button buttonToDisable)
    {
        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return; // No return value needed

        string downloadUrl;
        string componentName;
        string easyModeExtractPath;

        switch (type)
        {
            case DownloadType.Emulator: // Not used in this window, but kept for completeness
                downloadUrl = selectedSystem.Emulators?.Emulator?.EmulatorDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.EmulatorDownloadExtractPath;
                componentName = "Emulator";
                break;
            case DownloadType.Core: // Not used in this window, but kept for completeness
                downloadUrl = selectedSystem.Emulators?.Emulator?.CoreDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.CoreDownloadExtractPath;
                componentName = "Core";
                break;
            case DownloadType.ImagePack1:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = "Image Pack 1";
                break;
            case DownloadType.ImagePack2: // ADDED
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath2;
                componentName = "Image Pack 2";
                break;
            case DownloadType.ImagePack3: // ADDED
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath3;
                componentName = "Image Pack 3";
                break;
            case DownloadType.ImagePack4: // ADDED
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath4;
                componentName = "Image Pack 4";
                break;
            case DownloadType.ImagePack5: // ADDED
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath5;
                componentName = "Image Pack 5";
                break;
            default:
                return; // No return value needed
        }

        var destinationPath = PathHelper.ResolveRelativeToAppDirectory(easyModeExtractPath);

        // Ensure valid URL and destination path
        if (string.IsNullOrEmpty(downloadUrl))
        {
            var errorNodownloadUrLfor = (string)Application.Current.TryFindResource("ErrorNodownloadURLfor") ?? "Error: No download URL for";
            UpdateStatus($"{errorNodownloadUrLfor} {componentName}");
            return; // No return value needed
        }

        if (string.IsNullOrEmpty(destinationPath))
        {
            var errorInvalidDestinationPath = (string)Application.Current.TryFindResource("ErrorInvalidDestinationPath") ?? "Error: Invalid destination path for";
            UpdateStatus($"{errorInvalidDestinationPath} {componentName}");

            // Notify developer
            _ = LogErrors.LogErrorAsync(null, $"Invalid destination path for {componentName}: {easyModeExtractPath}");

            return; // No return value needed
        }

        try
        {
            // Initial state for the specific button
            buttonToDisable.IsEnabled = false;

            var preparingtodownload = (string)Application.Current.TryFindResource("Preparingtodownload") ?? "Preparing to download";
            UpdateStatus($"{preparingtodownload} {componentName}...");

            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StopDownloadButton.IsEnabled = true;

            var success = false;

            var downloading = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            UpdateStatus($"{downloading} {componentName}...");

            var downloadedFile = await _downloadManager.DownloadFileAsync(downloadUrl);

            if (downloadedFile != null && _downloadManager.IsDownloadCompleted)
            {
                var extracting = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                UpdateStatus($"{extracting} {componentName}...");
                LoadingMessage.Text = $"{extracting} {componentName}...";
                LoadingOverlay.Visibility = Visibility.Visible;
                success = await _downloadManager.ExtractFileAsync(downloadedFile, destinationPath);
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }

            if (success)
            {
                var hasbeensuccessfullydownloadedandinstalled = (string)Application.Current.TryFindResource("hasbeensuccessfullydownloadedandinstalled") ?? "has been successfully downloaded and installed.";
                UpdateStatus($"{componentName} {hasbeensuccessfullydownloadedandinstalled}");

                // Notify user
                MessageBoxLibrary.DownloadAndExtrationWereSuccessfulMessageBox();

                StopDownloadButton.IsEnabled = false;
                buttonToDisable.IsEnabled = false; // Keep disabled on success
                return; // No return value needed
            }
            else
            {
                if (_downloadManager.IsUserCancellation)
                {
                    var downloadof = (string)Application.Current.TryFindResource("Downloadof") ?? "Download of";
                    var wascanceled = (string)Application.Current.TryFindResource("wascanceled") ?? "was canceled.";
                    UpdateStatus($"{downloadof} {componentName} {wascanceled}");
                }
                else
                {
                    var errorFailedtoextract = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    UpdateStatus($"{errorFailedtoextract} {componentName}.");

                    switch (type)
                    {
                        case DownloadType.ImagePack1:
                        case DownloadType.ImagePack2:
                        case DownloadType.ImagePack3:
                        case DownloadType.ImagePack4:
                        case DownloadType.ImagePack5:
                            await MessageBoxLibrary.ShowImagePackDownloadErrorMessageBoxAsync(selectedSystem);
                            break;
                        default:
                            MessageBoxLibrary.DownloadExtractionFailedMessageBox();
                            break;
                    }
                }

                StopDownloadButton.IsEnabled = false;
                buttonToDisable.IsEnabled = true; // Re-enable on failure/cancellation
                return; // No return value needed
            }
        }
        catch (Exception ex)
        {
            var errorduring2 = (string)Application.Current.TryFindResource("Errorduring") ?? "Error during";
            var downloadprocess2 = (string)Application.Current.TryFindResource("downloadprocess") ?? "download process.";
            UpdateStatus($"{errorduring2} {componentName} {downloadprocess2}");

            // Notify developer
            var contextMessage = $"Error downloading {componentName}.\n" +
                                 $"URL: {downloadUrl}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            switch (type)
            {
                case DownloadType.ImagePack1:
                case DownloadType.ImagePack2:
                case DownloadType.ImagePack3:
                case DownloadType.ImagePack4:
                case DownloadType.ImagePack5:
                    await MessageBoxLibrary.ShowImagePackDownloadErrorMessageBoxAsync(selectedSystem);
                    break;
                default:
                    MessageBoxLibrary.DownloadExtractionFailedMessageBox();
                    break;
            }

            StopDownloadButton.IsEnabled = false;
            buttonToDisable.IsEnabled = true; // Re-enable on exception
            return; // No return value needed
        }
    }

    // ADDED: GetSelectedSystem() helper method
    private EasyModeSystemConfig GetSelectedSystem()
    {
        return SystemNameDropdown.SelectedItem != null
            ? _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString())
            : null;
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
        selectedSystem = GetSelectedSystem(); // Using the new helper method

        if (selectedSystem == null)
        {
            // Notify user
            MessageBoxLibrary.SelectedSystemIsNullMessageBox();
            return false;
        }

        // Validate download URL for at least one image pack (or the one being downloaded)
        // This method is called before HandleDownloadAndExtractComponent, so it should check generally.
        // The specific download button's click handler will check its own URL.
        var hasAnyDownloadLink = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink) ||
                                 !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2) ||
                                 !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3) ||
                                 !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4) ||
                                 !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5);

        if (!hasAnyDownloadLink)
        {
            // Notify user
            MessageBoxLibrary.DownloadUrlIsNullMessageBox();
            return false;
        }

        // Validate all potential extraction paths
        // This is a more robust check, ensuring all possible paths are valid or can be created.
        var pathsToValidate = new List<string>();
        if (!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath))
            pathsToValidate.Add(selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath);
        if (!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath2))
            pathsToValidate.Add(selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath2);
        if (!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath3))
            pathsToValidate.Add(selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath3);
        if (!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath4))
            pathsToValidate.Add(selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath4);
        if (!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath5))
            pathsToValidate.Add(selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath5);

        foreach (var path in pathsToValidate)
        {
            var finalPath = PathHelper.ResolveRelativeToAppDirectory(path);
            if (!CreateExtractionFolder(finalPath)) return false;
        }

        return true;
    }

    private bool CreateExtractionFolder(string extractionFolder)
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
            MessageBoxLibrary.ExtractionFolderCannotBeCreatedMessageBox(_logPath);

            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error creating the extraction folder.");

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

        // Re-enable all image pack download buttons that were active
        var selectedSystem = GetSelectedSystem();
        if (selectedSystem != null)
        {
            DownloadImagePackButton1.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink);
            DownloadImagePackButton2.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2);
            DownloadImagePackButton3.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3);
            DownloadImagePackButton4.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4);
            DownloadImagePackButton5.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5);
        }
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

