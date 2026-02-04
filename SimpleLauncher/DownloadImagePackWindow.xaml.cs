using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Threading.Tasks;
using Application = System.Windows.Application;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.DownloadService;
using SimpleLauncher.Services.DownloadService.Models;
using SimpleLauncher.Services.EasyMode;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.SharedModels;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher;

internal partial class DownloadImagePackWindow : IDisposable, System.ComponentModel.INotifyPropertyChanged, ILoadingState
{
    private EasyModeManager _manager;
    private readonly DownloadManager _downloadManager;
    private bool _disposed;

    public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }

    public bool IsOperationInProgress
    {
        get;
        private set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    // hold dynamic image pack buttons
    private ObservableCollection<ImagePackDownloadItem> ImagePacksToDisplay { get; }

    internal DownloadImagePackWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        DataContext = this;

        // Get the DownloadManager from the service provider
        _downloadManager = App.ServiceProvider.GetRequiredService<DownloadManager>();

        _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;

        // Initialize the new collection
        ImagePacksToDisplay = new ObservableCollection<ImagePackDownloadItem>();
        ImagePacksItemsControl.ItemsSource = ImagePacksToDisplay; // Bind ItemsControl to the collection

        // Set up event handlers
        Closed += CloseWindowRoutineAsync;
        Loaded += DownloadImagePackWindowLoadedAsync;
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;

            // Ensure the main content area is disabled to prevent Tab-key navigation
            MainContentGrid?.IsEnabled = !isLoading;

            if (isLoading)
            {
                LoadingOverlay.Content = message ?? (string)Application.Current.TryFindResource("Loading") ?? "Loading...";
            }
        });
    }

    private async void DownloadImagePackWindowLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await InitializeManagerAsync();
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "[DownloadImagePackWindowLoadedAsync] Error initializing EasyModeManager.");
        }
    }

    private async Task InitializeManagerAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        var loadingConfiguration = (string)Application.Current.TryFindResource("Loadingconfiguration") ?? "Loading configuration...";
        LoadingOverlay.Content = loadingConfiguration;
        await Task.Yield(); // Allow UI to render the loading overlay

        _manager = await EasyModeManager.LoadAsync();

        LoadingOverlay.Visibility = Visibility.Collapsed;

        if (_manager is not { Systems.Count: > 0 })
        {
            MessageBoxLibrary.ImagePackDownloaderUnavailableMessageBox();
            SystemNameDropdown.IsEnabled = false;
            return;
        }

        PopulateSystemDropdown();
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Assign an empty list if there's any error
            SystemNameDropdown.ItemsSource = new List<string>();
        }
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ImagePacksToDisplay.Clear(); // Clear previous buttons

        if (SystemNameDropdown.SelectedItem == null) return;

        var selectedSystem = GetSelectedSystem(); // Using the new helper method
        if (selectedSystem == null) return;

        // Dynamically add image pack items to the ObservableCollection
        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack1") ?? "Image Pack 1");
        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack2") ?? "Image Pack 2");
        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack3") ?? "Image Pack 3");
        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack4") ?? "Image Pack 4");
        AddImagePackItemIfValid(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5, selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath,
            (string)Application.Current.TryFindResource("ImagePack5") ?? "Image Pack 5");
    }

    private void AddImagePackItemIfValid(string downloadLink, string extractPath, string displayName)
    {
        if (!string.IsNullOrEmpty(downloadLink) && !string.IsNullOrEmpty(extractPath))
        {
            ImagePacksToDisplay.Add(new ImagePackDownloadItem
            {
                DisplayName = displayName,
                DownloadUrl = downloadLink,
                ExtractPath = extractPath,
                IsDownloaded = false // Initially not downloaded
            });
        }
    }

    // Single click handler for all dynamic image pack buttons
    private async void DownloadImagePackButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_disposed || IsOperationInProgress) return; // Early exit if window is already disposed

            try
            {
                if (sender is not Button { DataContext: ImagePackDownloadItem item } clickedButton) return;

                IsOperationInProgress = true;
                try
                {
                    clickedButton.IsEnabled = false; // Disable this specific button
                    item.IsDownloaded = false; // Mark as not downloaded while in progress

                    await HandleDownloadAndExtractComponentAsync(item);
                }
                catch (Exception ex)
                {
                    if (_disposed) return; // Check again after catch

                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error in DownloadImagePackButtonClickAsync for {item.DisplayName}.");
                    clickedButton.IsEnabled = true; // Re-enable on error
                    item.IsDownloaded = false;
                }
                finally
                {
                    IsOperationInProgress = false;
                }
            }
            catch (Exception ex)
            {
                if (_disposed) return; // Check again after catch

                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButtonClickAsync.");
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButtonClickAsync.");
        }
    }

    // Changed signature to accept ImagePackDownloadItem
    private async Task HandleDownloadAndExtractComponentAsync(ImagePackDownloadItem item)
    {
        if (_disposed) return; // Early exit if window is already disposed

        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return;

        var downloadUrl = item.DownloadUrl;
        var componentName = item.DisplayName;
        var easyModeExtractPath = item.ExtractPath;

        var destinationPath = PathHelper.ResolveRelativeToAppDirectory(easyModeExtractPath);

        // Ensure valid URL and destination path
        if (string.IsNullOrEmpty(downloadUrl))
        {
            var errorNodownloadUrLfor = (string)Application.Current.TryFindResource("ErrorNodownloadURLfor") ?? "Error: No download URL for";
            if (_disposed) return; // Check before UI update

            UpdateStatus($"{errorNodownloadUrLfor} {componentName}");
            return;
        }

        if (string.IsNullOrEmpty(destinationPath))
        {
            var errorInvalidDestinationPath = (string)Application.Current.TryFindResource("ErrorInvalidDestinationPath") ?? "Error: Invalid destination path for";
            if (_disposed) return; // Check before UI update

            UpdateStatus($"{errorInvalidDestinationPath} {componentName}");

            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[HandleDownloadAndExtractComponentAsync] Invalid destination path for {componentName}: {easyModeExtractPath}");

            return;
        }

        try
        {
            var preparingtodownload = (string)Application.Current.TryFindResource("Preparingtodownload") ?? "Preparing to download";
            if (_disposed) return; // Check before UI update

            UpdateStatus($"{preparingtodownload} {componentName}...");

            if (_disposed) return; // Check before UI update

            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StopDownloadButton.IsEnabled = true;

            var success = false;

            var downloading = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            if (_disposed) return; // Check before UI update

            UpdateStatus($"{downloading} {componentName}...");

            var downloadedFile = await _downloadManager.DownloadFileAsync(downloadUrl);

            if (_disposed) return; // Crucial check after first await

            if (downloadedFile != null && _downloadManager.IsDownloadCompleted)
            {
                var extracting = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                if (_disposed) return; // Check before UI update

                UpdateStatus($"{extracting} {componentName}...");
                LoadingOverlay.Content = $"{extracting} {componentName}...";
                LoadingOverlay.Visibility = Visibility.Visible;
                await Task.Yield(); // Allow UI to render the loading overlay
                success = await _downloadManager.ExtractFileAsync(downloadedFile, destinationPath);
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }

            if (_disposed) return; // Check before final UI updates

            if (success)
            {
                var hasbeensuccessfullydownloadedandinstalled = (string)Application.Current.TryFindResource("hasbeensuccessfullydownloadedandinstalled") ?? "has been successfully downloaded and installed.";
                UpdateStatus($"{componentName} {hasbeensuccessfullydownloadedandinstalled}");

                // Notify user
                MessageBoxLibrary.DownloadAndExtrationWereSuccessfulMessageBox();

                StopDownloadButton.IsEnabled = false;
                item.IsDownloaded = true; // Mark as downloaded
            }
            else
            {
                if (_disposed) return; // Check before UI updates in else branch

                if (_downloadManager.IsUserCancellation) // User cancelled the download
                {
                    var downloadof = (string)Application.Current.TryFindResource("Downloadof") ?? "Download of";
                    var wascanceled = (string)Application.Current.TryFindResource("wascanceled") ?? "was canceled.";
                    UpdateStatus($"{downloadof} {componentName} {wascanceled}");
                }
                else if (_downloadManager.IsDownloadCompleted) // Download OK, but extraction failed
                {
                    var errorFailedtoextract = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    UpdateStatus($"{errorFailedtoextract} {componentName}.");
                    await MessageBoxLibrary.ShowExtractionFailedMessageBoxAsync(_downloadManager.TempFolder);
                }
                else // Download failed for other reasons
                {
                    var errorFailedtoextract = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    UpdateStatus($"{errorFailedtoextract} {componentName}.");

                    // Since this is an image pack, use the specific message box
                    await MessageBoxLibrary.ShowImagePackDownloadErrorMessageBoxAsync(selectedSystem);
                    if (_disposed) return; // Check after await in MessageBox
                }

                StopDownloadButton.IsEnabled = false;
                item.IsDownloaded = false; // Ensure not marked as downloaded on failure/cancellation
            }
        }
        catch (Exception ex)
        {
            if (_disposed) return; // Check after catch

            var errorduring2 = (string)Application.Current.TryFindResource("Errorduring") ?? "Error during";
            var downloadprocess2 = (string)Application.Current.TryFindResource("downloadprocess") ?? "download process.";
            UpdateStatus($"{errorduring2} {componentName} {downloadprocess2}");

            // Notify developer only if it's not a disk space error
            // Disk space errors are user-environment issues, not code issues
            if (!(ex is IOException ioEx && (ioEx.Message.Contains("Insufficient disk space") || ioEx.Message.Contains("Cannot check disk space"))))
            {
                var contextMessage = $"Error downloading {componentName}.\n" +
                                     $"URL: {downloadUrl}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            }

            // If download was completed, the exception was likely during extraction.
            if (_downloadManager.IsDownloadCompleted)
            {
                await MessageBoxLibrary.ShowExtractionFailedMessageBoxAsync(_downloadManager.TempFolder);
            }
            else // Exception was during download
            {
                // Since this is an image pack, use the specific message box for download failures
                await MessageBoxLibrary.ShowImagePackDownloadErrorMessageBoxAsync(selectedSystem);
            }

            if (_disposed) return; // Check after await in MessageBox

            StopDownloadButton.IsEnabled = false;
            item.IsDownloaded = false; // Ensure not marked as downloaded on exception
        }
    }

    // GetSelectedSystem() helper method
    private EasyModeSystemConfig GetSelectedSystem()
    {
        return SystemNameDropdown.SelectedItem != null
            ? _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString())
            : null;
    }

    private void DownloadManager_ProgressChanged(object sender, DownloadProgressEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
            {
                if (_disposed) return; // Added for explicit safety, though InvokeAsync usually handles this.

                DownloadProgressBar.Value = e.ProgressPercentage;
                UpdateStatus(e.StatusMessage);
            }
        );
    }


    private void UpdateStatus(string message)
    {
        if (_disposed) return; // Check before UI update
        // Update the status text block
        StatusTextBlock.Text = message;
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if (_disposed) return; // Early exit if window is already disposed

        _downloadManager.CancelDownload();
        StopDownloadButton.IsEnabled = false;
        DownloadProgressBar.Value = 0;

        var downloadcanceled2 = (string)Application.Current.TryFindResource("Downloadcanceled") ?? "Download canceled";
        UpdateStatus(downloadcanceled2);

        // Re-enable all image pack download buttons
        foreach (var item in ImagePacksToDisplay)
        {
            if (_disposed) break; // If disposed during loop, stop updating

            item.IsDownloaded = false; // Mark as not downloaded to re-enable button
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error opening the download link.");

            // Notify user
            MessageBoxLibrary.CouldNotOpenTheDownloadLink();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private async void CloseWindowRoutineAsync(object sender, EventArgs e)
    {
        try
        {
            if (StopDownloadButton.IsEnabled)
            {
                StopDownloadButton_Click(null, null);
                await Task.Delay(200); // A small delay to allow cancellation to start
            }

            _manager = null;
            Dispose();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error closing the Add System window.");
        }
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _downloadManager?.Dispose();
            _downloadManager?.DownloadProgressChanged -= DownloadManager_ProgressChanged; // Unsubscribe from event
        }

        _disposed = true;
    }

    ~DownloadImagePackWindow()
    {
        Dispose(false);
    }
}