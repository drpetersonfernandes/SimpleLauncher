using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml.Linq;
using Microsoft.Win32;
using SimpleLauncher.Services;
using Application = System.Windows.Application;
using System.Xml;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher;

public partial class EasyModeWindow : IDisposable, INotifyPropertyChanged
{
    private EasyModeManager _manager;

    public bool IsEmulatorDownloaded
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
            UpdateAddSystemButtonState();
        }
    } = true;

    public bool IsCoreDownloaded
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
            UpdateAddSystemButtonState();
        }
    } = true;

    public bool IsImagePack1Downloaded
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack2Downloaded
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack3Downloaded
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack4Downloaded
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack5Downloaded
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    } = true;

    public bool IsImagePack1Available
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsImagePack2Available
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsImagePack3Available
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsImagePack4Available
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsImagePack5Available
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    private readonly DownloadManager _downloadManager;
    private bool _disposed;

    private readonly string _basePath = AppDomain.CurrentDomain.BaseDirectory;

    private string DownloadStatus
    {
        get;
        set
        {
            // The 'field' keyword is not valid here. It should be a backing field or auto-property.
            // Assuming this was intended to be an auto-property, or a backing field was omitted.
            // For now, I'll assume it's an auto-property and remove the 'field =' line.
            // If there was a backing field, it would be '_downloadStatus = value;'.
            // Given the context of other properties, it's likely an auto-property was intended here,
            // or the diff didn't touch this part and it was already broken/intended to be fixed elsewhere.
            // Since the diff doesn't touch this, I'll leave it as is, but note the 'field' keyword issue.
            // If it were an auto-property, the setter would just be empty or call OnPropertyChanged.
            // If it's meant to update a TextBlock, it needs to be a full property with a backing field.
            // Let's make it a full property with a backing field to match the pattern.
            // This part was not in the diff, so I will revert it to what it was, but it's an issue.
            // Re-reading the original code, it was `field = value; DownloadStatusTextBlock.Text = value;`.
            // This means it was trying to use a compiler-generated backing field for an auto-property,
            // but also doing UI update. This is incorrect. It should be a full property with a backing field.
            // However, since the diff *only* applies to the properties above, I will not change this.
            // The diff does not touch this property, so I will leave it as it was in the original code.
            field = value;
            DownloadStatusTextBlock.Text = value;
        }
    } = string.Empty;

    public event PropertyChangedEventHandler PropertyChanged; // INotifyPropertyChanged implementation

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public EasyModeWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        // Set DataContext for XAML bindings to work
        DataContext = this;

        // Get the DownloadManager from the service provider
        _downloadManager = App.ServiceProvider.GetRequiredService<DownloadManager>();

        _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;

        Closed += CloseWindowRoutineAsync;
        Loaded += EasyModeWindowLoadedAsync;
    }

    private async void EasyModeWindowLoadedAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await InitializeManagerAsync();
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "[EasyModeWindowLoadedAsync] Error initializing EasyModeManager.");
        }
    }

    private async Task InitializeManagerAsync()
    {
        LoadingOverlay.Visibility = Visibility.Visible;
        var loadingConfiguration = (string)Application.Current.TryFindResource("Loadingconfiguration") ?? "Loading configuration...";
        LoadingMessage.Text = loadingConfiguration;
        await Task.Yield(); // Allow UI to render the loading overlay

        _manager = await EasyModeManager.LoadAsync();

        LoadingOverlay.Visibility = Visibility.Collapsed;

        if (_manager is not { Systems.Count: > 0 })
        {
            MessageBoxLibrary.EasyModeUnavailableMessageBox();
            SystemNameDropdown.IsEnabled = false;
            SystemFolderTextBox.IsEnabled = false;
            DownloadEmulatorButton.IsEnabled = false;
            DownloadCoreButton.IsEnabled = false;
            DownloadImagePackButton1.IsEnabled = false;
            DownloadImagePackButton2.IsEnabled = false;
            DownloadImagePackButton3.IsEnabled = false;
            DownloadImagePackButton4.IsEnabled = false;
            DownloadImagePackButton5.IsEnabled = false;
            AddSystemButton.IsEnabled = false;
            return;
        }

        PopulateSystemDropdown();
    }

    /// Populates the system dropdown with a sorted list of system names based on the configuration data.
    /// The method retrieves the list of system configurations from the EasyModeManager. It filters systems
    /// that have a non-empty and valid `EmulatorDownloadLink` in their corresponding emulator configuration.
    /// System names are then sorted alphabetically before being assigned to the `ItemsSource` property of the
    /// dropdown UI element.
    /// Preconditions:
    /// - `_manager` should be initialized and its `Systems` property should not be null.
    /// - `SystemNameDropdown` should refer to a valid ComboBox.
    /// Postconditions:
    /// - The dropdown is populated with a sorted list of valid system names. If no valid systems are found,
    /// the dropdown is left empty.
    /// Applies to:
    /// - The method is specifically designed for use in the `EasyModeWindow` class and interacts
    /// with its UI components
    private void PopulateSystemDropdown()
    {
        try
        {
            if (_manager?.Systems == null)
            {
                SystemNameDropdown.ItemsSource = new List<string>(); // return an empty list
                return;
            }

            var sortedSystemNames = _manager.Systems
                .Where(static system => !string.IsNullOrEmpty(system.Emulators?.Emulator?.EmulatorDownloadLink))
                .Select(static system => system.SystemName)
                .OrderBy(static name => name)
                .ToList();

            SystemNameDropdown.ItemsSource = sortedSystemNames;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error populating system dropdown.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            SystemNameDropdown.ItemsSource = new List<string>(); // Assign an empty list if there's any error
        }
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null)
        {
            // Reset all states if no system is selected
            DownloadEmulatorButton.IsEnabled = false;
            DownloadCoreButton.IsEnabled = false;

            IsImagePack1Available = false;
            IsImagePack2Available = false;
            IsImagePack3Available = false;
            IsImagePack4Available = false;
            IsImagePack5Available = false;

            IsEmulatorDownloaded = true;
            IsCoreDownloaded = true;
            IsImagePack1Downloaded = true;
            IsImagePack2Downloaded = true;
            IsImagePack3Downloaded = true;
            IsImagePack4Downloaded = true;
            IsImagePack5Downloaded = true;

            UpdateAddSystemButtonState();
            SystemFolderTextBox.Text = string.Empty;
            return;
        }

        var selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem == null)
        {
            // This should ideally not happen if PopulateSystemDropdown is correct, but handle defensively
            return;
        }

        var emulator = selectedSystem.Emulators?.Emulator;
        // Determine if download links exist for image packs (for visibility)
        IsImagePack1Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink) && !string.IsNullOrEmpty(emulator?.ImagePackDownloadExtractPath);
        IsImagePack2Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink2) && !string.IsNullOrEmpty(emulator?.ImagePackDownloadExtractPath);
        IsImagePack3Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink3) && !string.IsNullOrEmpty(emulator?.ImagePackDownloadExtractPath);
        IsImagePack4Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink4) && !string.IsNullOrEmpty(emulator?.ImagePackDownloadExtractPath);
        IsImagePack5Available = !string.IsNullOrEmpty(emulator?.ImagePackDownloadLink5) && !string.IsNullOrEmpty(emulator?.ImagePackDownloadExtractPath);

        // Check if Emulator file already exists on disk. If so, mark it as "downloaded".
        var emulatorLocation = selectedSystem.Emulators?.Emulator?.EmulatorLocation;
        if (!string.IsNullOrEmpty(emulatorLocation))
        {
            var resolvedEmulatorPath = PathHelper.ResolveRelativeToAppDirectory(emulatorLocation);
            IsEmulatorDownloaded = File.Exists(resolvedEmulatorPath);
        }
        else
        {
            // If no location is defined, it can't exist, so it needs to be downloaded.
            IsEmulatorDownloaded = false;
        }

        // Check if Core file already exists or is not needed.
        var coreLocation = selectedSystem.Emulators?.Emulator?.CoreLocation;
        var coreDownloadLink = selectedSystem.Emulators?.Emulator?.CoreDownloadLink;
        if (!string.IsNullOrEmpty(coreLocation))
        {
            var resolvedCorePath = PathHelper.ResolveRelativeToAppDirectory(coreLocation);
            IsCoreDownloaded = File.Exists(resolvedCorePath);
        }
        else
        {
            // If no location is defined, it's considered "ready" only if no download is offered.
            IsCoreDownloaded = string.IsNullOrEmpty(coreDownloadLink);
        }

        // Reset download status for image packs.
        IsImagePack1Downloaded = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink);
        IsImagePack2Downloaded = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2);
        IsImagePack3Downloaded = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3);
        IsImagePack4Downloaded = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4);
        IsImagePack5Downloaded = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5);

        // Resolve path for display in the textbox
        SystemFolderTextBox.Text = PathHelper.ResolveRelativeToAppDirectory(selectedSystem.SystemFolder);
    }

    private async void DownloadEmulatorButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await HandleDownloadAndExtractComponentAsync(DownloadType.Emulator);
        }
        catch (Exception ex)
        {
            if (_disposed) return;

            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadEmulatorButtonClickAsync.");
        }
    }

    private async void DownloadCoreButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await HandleDownloadAndExtractComponentAsync(DownloadType.Core);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadCoreButtonClickAsync.");
        }
    }

    private async void DownloadImagePackButton1ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await HandleDownloadAndExtractComponentAsync(DownloadType.ImagePack1);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton1ClickAsync.");
        }
    }

    private async void DownloadImagePackButton2ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await HandleDownloadAndExtractComponentAsync(DownloadType.ImagePack2);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton2ClickAsync.");
        }
    }

    private async void DownloadImagePackButton3ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await HandleDownloadAndExtractComponentAsync(DownloadType.ImagePack3);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton3ClickAsync.");
        }
    }

    private async void DownloadImagePackButton4ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await HandleDownloadAndExtractComponentAsync(DownloadType.ImagePack4);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton4ClickAsync.");
        }
    }

    private async void DownloadImagePackButton5ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await HandleDownloadAndExtractComponentAsync(DownloadType.ImagePack5);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in DownloadImagePackButton5ClickAsync.");
        }
    }

    // Helper method to reduce code duplication for downloads and extractions
    private async Task HandleDownloadAndExtractComponentAsync(DownloadType type)
    {
        if (_disposed) return;

        // Reset the downloaded state at the start to disable the button
        switch (type)
        {
            case DownloadType.Emulator:
                IsEmulatorDownloaded = false;
                break;
            case DownloadType.Core:
                IsCoreDownloaded = false;
                break;
            case DownloadType.ImagePack1:
                IsImagePack1Downloaded = false;
                break;
            case DownloadType.ImagePack2:
                IsImagePack2Downloaded = false;
                break;
            case DownloadType.ImagePack3:
                IsImagePack3Downloaded = false;
                break;
            case DownloadType.ImagePack4:
                IsImagePack4Downloaded = false;
                break;
            case DownloadType.ImagePack5:
                IsImagePack5Downloaded = false;
                break;
        }

        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return;

        string downloadUrl;
        string componentName;
        string easyModeExtractPath;

        switch (type)
        {
            case DownloadType.Emulator:
                downloadUrl = selectedSystem.Emulators?.Emulator?.EmulatorDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.EmulatorDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("Emulator") ?? "Emulator";
                break;
            case DownloadType.Core:
                downloadUrl = selectedSystem.Emulators?.Emulator?.CoreDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.CoreDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("Core") ?? "Core";
                break;
            case DownloadType.ImagePack1:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack1") ?? "Image Pack 1";
                break;
            case DownloadType.ImagePack2:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack2") ?? "Image Pack 2";
                break;
            case DownloadType.ImagePack3:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack3") ?? "Image Pack 3";
                break;
            case DownloadType.ImagePack4:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack4") ?? "Image Pack 4";
                break;
            case DownloadType.ImagePack5:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = (string)Application.Current.TryFindResource("ImagePack5") ?? "Image Pack 5";
                break;
            default:
                return;
        }

        var destinationPath = PathHelper.ResolveRelativeToAppDirectory(easyModeExtractPath);

        // Ensure valid URL and destination path
        if (string.IsNullOrEmpty(downloadUrl))
        {
            var errorNodownloadUrLfor = (string)Application.Current.TryFindResource("ErrorNodownloadURLfor") ?? "Error: No download URL for";
            DownloadStatus = $"{errorNodownloadUrLfor} {componentName}";
            return;
        }

        if (string.IsNullOrEmpty(destinationPath))
        {
            var errorInvalidDestinationPath = (string)Application.Current.TryFindResource("ErrorInvalidDestinationPath") ?? "Error: Invalid destination path for";
            DownloadStatus = $"{errorInvalidDestinationPath} {componentName}";

            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[HandleDownloadAndExtractComponentAsync] Invalid destination path for {componentName}: {easyModeExtractPath}");

            return;
        }

        try
        {
            var preparingtodownload = (string)Application.Current.TryFindResource("Preparingtodownload") ?? "Preparing to download";
            DownloadStatus = $"{preparingtodownload} {componentName}...";

            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StopDownloadButton.IsEnabled = true;

            var success = false;

            var downloading = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            DownloadStatus = $"{downloading} {componentName}...";

            var downloadedFile = await _downloadManager.DownloadFileAsync(downloadUrl);

            if (_disposed) return;

            if (downloadedFile != null && _downloadManager.IsDownloadCompleted)
            {
                var extracting = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                DownloadStatus = $"{extracting} {componentName}...";
                LoadingMessage.Text = $"{extracting} {componentName}...";
                LoadingOverlay.Visibility = Visibility.Visible;
                await Task.Yield(); // Allow UI to render the loading overlay
                success = await _downloadManager.ExtractFileAsync(downloadedFile, destinationPath);
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }

            if (success)
            {
                var hasbeensuccessfullydownloadedandinstalled = (string)Application.Current.TryFindResource("hasbeensuccessfullydownloadedandinstalled") ?? "has been successfully downloaded and installed.";
                DownloadStatus = $"{componentName} {hasbeensuccessfullydownloadedandinstalled}";

                // Notify user
                MessageBoxLibrary.DownloadAndExtrationWereSuccessfulMessageBox();

                StopDownloadButton.IsEnabled = false;
                // Mark as successfully downloaded
                MarkComponentAsDownloaded(type, true);
            }
            else // Download was not completed successfully (either cancelled, locked, or other failure)
            {
                if (_disposed) return;

                if (_downloadManager.IsUserCancellation) // User cancelled the download
                {
                    var downloadof = (string)Application.Current.TryFindResource("Downloadof") ?? "Download of";
                    var wascanceled = (string)Application.Current.TryFindResource("wascanceled") ?? "was canceled.";
                    DownloadStatus = $"{downloadof} {componentName} {wascanceled}";
                }
                else if (_downloadManager.IsFileLockedDuringDownload) // Specific check for file lock during download
                {
                    await MessageBoxLibrary.ShowDownloadFileLockedMessageBoxAsync(_downloadManager.TempFolder);
                }
                else if (_downloadManager.IsDownloadCompleted) // This means download was completed, but something went wrong *after* (e.g., during cleanup or a very late error)
                {
                    var errorFailedtoextract = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    DownloadStatus = $"{errorFailedtoextract} {componentName}.";
                    await MessageBoxLibrary.ShowExtractionFailedMessageBoxAsync(_downloadManager.TempFolder);
                }
                else // Generic download failure (not user cancelled, not file locked, not extraction failure)
                {
                    var errorDuringDownload = (string)Application.Current.TryFindResource("Errorduringdownload") ?? "Error during download";
                    DownloadStatus = $"{errorDuringDownload}: {componentName}.";

                    // Fallback to original behavior for download failures
                    await ShowDownloadErrorDialogAsync(type, selectedSystem);
                }

                StopDownloadButton.IsEnabled = false;
                MarkComponentAsDownloaded(type, false); // Ensure not marked as downloaded on failure/cancellation
                return;
            }

            return;
        }
        catch (Exception ex)
        {
            if (_disposed) return;

            var errorduring2 = (string)Application.Current.TryFindResource("Errorduring") ?? "Error during";
            var downloadprocess2 = (string)Application.Current.TryFindResource("downloadprocess") ?? "download process.";
            DownloadStatus = $"{errorduring2} {componentName} {downloadprocess2}";

            // Notify developer only if it's not a disk space error
            // Disk space errors are user-environment issues, not code issues
            if (!(ex is IOException ioEx && (ioEx.Message.Contains("Insufficient disk space") || ioEx.Message.Contains("Cannot check disk space"))))
            {
                var contextMessage = $"Error downloading {componentName}.\n" +
                                     $"URL: {downloadUrl}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            }

            // Check if the download failed due to a file lock
            if (_downloadManager.IsFileLockedDuringDownload)
            {
                await MessageBoxLibrary.ShowDownloadFileLockedMessageBoxAsync(_downloadManager.TempFolder);
            }

            // If download was completed, the exception was likely during extraction.
            else if (_downloadManager.IsDownloadCompleted)
            {
                await MessageBoxLibrary.ShowExtractionFailedMessageBoxAsync(_downloadManager.TempFolder);
            }
            else // Exception was during download
            {
                await ShowDownloadErrorDialogAsync(type, selectedSystem);
            }

            if (_disposed) return;

            StopDownloadButton.IsEnabled = false;
            MarkComponentAsDownloaded(type, false); // Ensure not marked as downloaded on exception
            return;
        }
    }

    private static async Task ShowDownloadErrorDialogAsync(DownloadType type, EasyModeSystemConfig selectedSystem)
    {
        switch (type)
        {
            case DownloadType.Emulator:
                await MessageBoxLibrary.ShowEmulatorDownloadErrorMessageBoxAsync(selectedSystem);
                break;
            case DownloadType.Core:
                await MessageBoxLibrary.ShowCoreDownloadErrorMessageBoxAsync(selectedSystem);
                break;
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

    // Helper method to mark a component as downloaded (or not)
    private void MarkComponentAsDownloaded(DownloadType type, bool isDownloaded)
    {
        switch (type)
        {
            case DownloadType.Emulator:
                IsEmulatorDownloaded = isDownloaded;
                break;
            case DownloadType.Core:
                IsCoreDownloaded = isDownloaded;
                break;
            case DownloadType.ImagePack1:
                IsImagePack1Downloaded = isDownloaded;
                break;
            case DownloadType.ImagePack2:
                IsImagePack2Downloaded = isDownloaded;
                break;
            case DownloadType.ImagePack3:
                IsImagePack3Downloaded = isDownloaded;
                break;
            case DownloadType.ImagePack4:
                IsImagePack4Downloaded = isDownloaded;
                break;
            case DownloadType.ImagePack5:
                IsImagePack5Downloaded = isDownloaded;
                break;
        }
    }

    private void DownloadManager_ProgressChanged(object sender, DownloadProgressEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            DownloadProgressBar.Value = e.ProgressPercentage;
            DownloadStatus = e.StatusMessage;
        });
    }

    private EasyModeSystemConfig GetSelectedSystem()
    {
        if (_disposed || _manager == null) return null;

        return SystemNameDropdown.SelectedItem != null
            ? _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString())
            : null;
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        _downloadManager.CancelDownload();
        StopDownloadButton.IsEnabled = false;
        DownloadProgressBar.Value = 0;

        var cancelingdownload2 = (string)Application.Current.TryFindResource("Cancelingdownload") ?? "Canceling download...";
        DownloadStatus = cancelingdownload2;
    }

    private async void AddSystemButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try // Top-level catch for async Task method
        {
            var selectedSystem = GetSelectedSystem();
            if (selectedSystem == null) return;

            string systemFolderRaw;
            if (!string.IsNullOrEmpty(SystemFolderTextBox.Text) && !string.IsNullOrWhiteSpace(SystemFolderTextBox.Text))
            {
                systemFolderRaw = SystemFolderTextBox.Text;
            }
            else
            {
                systemFolderRaw = Path.Combine("%BASEFOLDER%", "roms", selectedSystem.SystemName);
                // No need to update SystemFolderTextBox.Text here, it's already updated in SelectionChanged or will be updated by the user
            }

            var systemImageFolderRaw = selectedSystem.SystemImageFolder;

            var systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

            // --- Start Async Operation ---
            try
            {
                // Disable button during operation
                AddSystemButton.IsEnabled = false;

                // Show overlay
                LoadingMessage.Text = (string)Application.Current.TryFindResource("Addingsystemtoconfiguration") ?? "Adding system to configuration...";
                LoadingOverlay.Visibility = Visibility.Visible;
                await Task.Yield(); // Allow UI to render the loading overlay

                // Update System.xml with the *unresolved* paths, as system.xml expects them.
                await UpdateSystemXmlAsync(systemXmlPath, selectedSystem, systemFolderRaw, systemImageFolderRaw);

                // --- If XML update succeeds, continue with folder creation and UI updates ---
                LoadingMessage.Text = (string)Application.Current.TryFindResource("Creatingsystemfolders") ?? "Creating system folders...";

                // Resolve paths before passing to folder creation
                var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(systemFolderRaw);
                var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemImageFolderRaw);

                // Create System Folders using *resolved* paths
                CreateSystemFolders.CreateFolders(selectedSystem.SystemName, resolvedSystemFolder, resolvedSystemImageFolder);

                var systemhasbeensuccessfullyadded = (string)Application.Current.TryFindResource("Systemhasbeensuccessfullyadded") ?? "System has been successfully added!";
                DownloadStatus = systemhasbeensuccessfullyadded;

                // Notify user
                MessageBoxLibrary.SystemAddedMessageBox(selectedSystem.SystemName, resolvedSystemFolder, resolvedSystemImageFolder);

                // Close the window after successful addition
                Close();
            }
            catch (InvalidOperationException ex) // Catch specific exceptions from the helper
            {
                var errorFailedtoaddsystem = (string)Application.Current.TryFindResource("ErrorFailedtoaddsystem") ?? "Error: Failed to add system.";
                DownloadStatus = $"{errorFailedtoaddsystem} {ex.Message}";

                // Error is already logged by the helper method.
                // Notify user
                MessageBoxLibrary.AddSystemFailedMessageBox(ex.Message);
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                var errorFailedtoaddsystem = (string)Application.Current.TryFindResource("ErrorFailedtoaddsystem") ?? "Error: Failed to add system.";
                DownloadStatus = errorFailedtoaddsystem;

                // Notify developer
                const string contextMessage = "Unexpected error adding system.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.AddSystemFailedMessageBox();
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed; // Hide overlay
                if (IsLoaded) // Check if the window is still loaded
                {
                    AddSystemButton.IsEnabled = true;
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in AddSystemButtonClickAsync.");
        }
    }

    // Moved from EditSystemWindow.SaveSystem.cs and adapted for EasyModeWindow
    private async Task UpdateSystemXmlAsync(
        string xmlPath,
        EasyModeSystemConfig selectedSystem,
        string systemFolder, // This should be the raw path with %BASEFOLDER%
        string systemImageFolder) // This should be the raw path with %BASEFOLDER%
    {
        XDocument xmlDoc = null; // Initialize to null
        try
        {
            // Attempt to load existing XML content asynchronously
            if (File.Exists(xmlPath))
            {
                try
                {
                    var xmlContent = await File.ReadAllTextAsync(xmlPath);
                    // Only parse if content is not empty to avoid errors with empty files
                    if (!string.IsNullOrWhiteSpace(xmlContent))
                    {
                        xmlDoc = XDocument.Parse(xmlContent);
                        // Check if the root element is valid
                        if (xmlDoc.Root == null || xmlDoc.Root.Name != "SystemConfigs")
                        {
                            // If root is null or incorrect, treat as invalid and create new
                            xmlDoc = null; // Reset xmlDoc to trigger creation below

                            // Notify developer
                            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new XmlException("Loaded system.xml has missing or invalid root element."), "Invalid root in system.xml, creating new.");
                        }
                    }
                }
                catch (XmlException ex) // Catch specific XML parsing errors
                {
                    // Notify developer
                    // Log the parsing error but proceed to create a new document
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error parsing existing system.xml, creating new.");

                    xmlDoc = null; // Ensure we create a new one
                }
                catch (Exception ex) // Catch other file reading errors
                {
                    // Notify developer
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error reading existing system.xml.");

                    throw new IOException("Could not read the existing system configuration file.", ex); // Rethrow as IO
                }
            }

            // If xmlDoc is still null (file didn't exist, was empty, or had invalid root), create a new one
            xmlDoc ??= new XDocument(new XElement("SystemConfigs"));

            // --- Proceed with modification logic ---
            if (xmlDoc.Root != null)
            {
                var systemManagers = xmlDoc.Root.Descendants("SystemConfig").ToList(); // Safe now because Root is guaranteed
                var existingSystem = systemManagers.FirstOrDefault(config => config.Element("SystemName")?.Value == selectedSystem.SystemName);
                if (existingSystem != null)
                {
                    // Overwrite existing system (in memory)
                    OverwriteExistingSystem(existingSystem, selectedSystem, systemFolder, systemImageFolder);
                }
                else
                {
                    // Create new system element (in memory)
                    var newSystemElement = SaveNewSystem(selectedSystem, systemFolder, systemImageFolder);
                    xmlDoc.Root.Add(newSystemElement);
                }
            }

            // Sort the elements (in memory)
            if (xmlDoc.Root != null)
            {
                var sortedElements = xmlDoc.Root.Elements("SystemConfig")
                    .OrderBy(static systemElement => systemElement.Element("SystemName")?.Value)
                    .ToList(); // Create a list of sorted elements
                // Replace the nodes in the original document's root
                xmlDoc.Root.ReplaceNodes(sortedElements);
            }

            // Save the updated and sorted XML document asynchronously with proper formatting
            // Use SaveOptions.None for default indentation
            await Task.Run(() =>
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ", // Use 2 spaces for indentation
                    NewLineHandling = NewLineHandling.Replace,
                    Encoding = System.Text.Encoding.UTF8
                };

                using var writer = XmlWriter.Create(xmlPath, settings);
                xmlDoc.Declaration ??= new XDeclaration("1.0", "utf-8", null);
                xmlDoc.Save(writer);
            });
        }
        catch (IOException ex) // Handle file saving errors (permissions, disk full, etc.)
        {
            // Notify developer
            const string contextMessage = "Error saving system.xml.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            throw new InvalidOperationException("Could not save system configuration.", ex);
        }
        catch (Exception ex) // Catch other potential errors
        {
            // Notify developer
            const string contextMessage = "Unexpected error updating system.xml.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            throw new InvalidOperationException("An unexpected error occurred while updating system configuration.", ex);
        }
    }

    // Moved from EditSystemWindow.SaveSystem.cs and adapted for EasyModeWindow
    private static XElement SaveNewSystem(EasyModeSystemConfig selectedSystem, string systemFolder, string systemImageFolder)
    {
        var newSystemElement = new XElement("SystemConfig",
            new XElement("SystemName", selectedSystem.SystemName),
            new XElement("SystemFolders", new XElement("SystemFolder", systemFolder)), // Only one folder from EasyMode
            new XElement("SystemImageFolder", systemImageFolder),
            new XElement("SystemIsMAME", selectedSystem.SystemIsMame.ToString()),
            new XElement("FileFormatsToSearch", selectedSystem.FileFormatsToSearch.Select(static format => new XElement("FormatToSearch", format))),
            new XElement("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch.ToString()),
            new XElement("FileFormatsToLaunch", selectedSystem.FileFormatsToLaunch.Select(static format => new XElement("FormatToLaunch", format))),
            new XElement("Emulators",
                new XElement("Emulator",
                    new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                    new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                    new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
                    , new XElement("ImagePackDownloadLink", selectedSystem.Emulators.Emulator.ImagePackDownloadLink)
                    , new XElement("ImagePackDownloadLink2", selectedSystem.Emulators.Emulator.ImagePackDownloadLink2)
                    , new XElement("ImagePackDownloadLink3", selectedSystem.Emulators.Emulator.ImagePackDownloadLink3)
                    , new XElement("ImagePackDownloadLink4", selectedSystem.Emulators.Emulator.ImagePackDownloadLink4)
                    , new XElement("ImagePackDownloadLink5", selectedSystem.Emulators.Emulator.ImagePackDownloadLink5)
                    , new XElement("ImagePackDownloadExtractPath", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath)
                )
            )
        );
        return newSystemElement;
    }

    // Moved from EditSystemWindow.SaveSystem.cs and adapted for EasyModeWindow
    private static void OverwriteExistingSystem(XElement existingSystem, EasyModeSystemConfig selectedSystem, string systemFolder, string systemImageFolder)
    {
        existingSystem.SetElementValue("SystemName", selectedSystem.SystemName);
        // Update SystemFolders
        var foldersElement = existingSystem.Element("SystemFolders");
        if (foldersElement == null)
        {
            foldersElement = new XElement("SystemFolders");
            // Add it after SystemName to maintain order
            existingSystem.Element("SystemName")?.AddAfterSelf(foldersElement);
        }

        foldersElement.ReplaceNodes(new XElement("SystemFolder", systemFolder)); // Only one folder from EasyMode

        existingSystem.SetElementValue("SystemImageFolder", systemImageFolder);
        existingSystem.SetElementValue("SystemIsMAME", selectedSystem.SystemIsMame.ToString());
        existingSystem.Element("FileFormatsToSearch")?.ReplaceNodes(selectedSystem.FileFormatsToSearch.Select(static format => new XElement("FormatToSearch", format)));
        existingSystem.SetElementValue("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch.ToString());
        existingSystem.Element("FileFormatsToLaunch")?.ReplaceNodes(selectedSystem.FileFormatsToLaunch.Select(static format => new XElement("FormatToLaunch", format)));
        existingSystem.Element("Emulators")?.Remove();
        existingSystem.Add(new XElement("Emulators",
            new XElement("Emulator",
                new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
                , new XElement("ImagePackDownloadLink", selectedSystem.Emulators.Emulator.ImagePackDownloadLink)
                , new XElement("ImagePackDownloadLink2", selectedSystem.Emulators.Emulator.ImagePackDownloadLink2)
                , new XElement("ImagePackDownloadLink3", selectedSystem.Emulators.Emulator.ImagePackDownloadLink3)
                , new XElement("ImagePackDownloadLink4", selectedSystem.Emulators.Emulator.ImagePackDownloadLink4)
                , new XElement("ImagePackDownloadLink5", selectedSystem.Emulators.Emulator.ImagePackDownloadLink5)
                , new XElement("ImagePackDownloadExtractPath", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath)
            )
        ));
    }

    private void UpdateAddSystemButtonState()
    {
        if (_disposed) return;

        var selectedSystem = GetSelectedSystem();
        if (selectedSystem?.Emulators?.Emulator == null)
        {
            AddSystemButton.IsEnabled = false;
            return;
        }

        var emulatorConfig = selectedSystem.Emulators.Emulator;

        // The emulator is always required if a download link exists.
        var isEmulatorDownloadRequired = !string.IsNullOrEmpty(emulatorConfig.EmulatorDownloadLink);
        var isEmulatorReady = !isEmulatorDownloadRequired || IsEmulatorDownloaded;

        // The core is only required if a download link for it exists.
        var isCoreDownloadRequired = !string.IsNullOrEmpty(emulatorConfig.CoreDownloadLink);
        var isCoreReady = !isCoreDownloadRequired || IsCoreDownloaded;

        // The "Add System" button is enabled if all *required* components (emulator and core) are ready.
        // Image packs are optional and do not affect this logic.
        AddSystemButton.IsEnabled = isEmulatorReady && isCoreReady;
    }


    private async void CloseWindowRoutineAsync(object sender, EventArgs e)
    {
        try // Top-level catch for async Task method
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error closing the Add System window.");
        }
    }

    private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var chooseaFolderwithRoMsorIsOs2 = (string)Application.Current.TryFindResource("ChooseafolderwithROMsorISOsforthissystem") ?? "Choose a folder with 'ROMs' or 'ISOs' for this system";

        // Create a new OpenFolderDialog
        var openFolderDialog = new OpenFolderDialog
        {
            Title = chooseaFolderwithRoMsorIsOs2
        };

        // Show the dialog and handle the result
        if (openFolderDialog.ShowDialog() == true)
        {
            SystemFolderTextBox.Text = openFolderDialog.FolderName;
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
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
        if (_disposed) return;

        _downloadManager?.Dispose();

        _disposed = true;

        // Tell GC not to call the finalizer since we've already cleaned up
        GC.SuppressFinalize(this);
    }
}