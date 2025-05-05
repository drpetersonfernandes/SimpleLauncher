using System;
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

namespace SimpleLauncher;

public partial class EasyModeWindow : IDisposable
{
    private EasyModeManager _manager;
    private bool _isEmulatorDownloaded;
    private bool _isCoreDownloaded;
    private readonly DownloadManager _downloadManager;
    private bool _disposed;

    // Download component types
    private enum DownloadType
    {
        Emulator,
        Core,
        ImagePack
    }

    // Download status tracking
    private string _downloadStatus = string.Empty;

    private string DownloadStatus
    {
        get => _downloadStatus;
        set
        {
            _downloadStatus = value;
            DownloadStatusTextBlock.Text = value;
        }
    }

    public EasyModeWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        // Initialize the DownloadManager
        _downloadManager = new DownloadManager();
        _downloadManager.DownloadProgressChanged += DownloadManager_ProgressChanged;

        // Load Config
        _manager = EasyModeManager.Load();
        PopulateSystemDropdown();

        Closed += CloseWindowRoutine;
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
        if (_manager?.Systems == null) return;

        var sortedSystemNames = _manager.Systems
            .Where(static system => !string.IsNullOrEmpty(system.Emulators?.Emulator?.EmulatorDownloadLink))
            .Select(static system => system.SystemName)
            .OrderBy(static name => name)
            .ToList();

        SystemNameDropdown.ItemsSource = sortedSystemNames;
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null) return;

        var selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem == null) return;

        DownloadEmulatorButton.IsEnabled = true;
        DownloadCoreButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.CoreDownloadLink);
        DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadLink);

        // Reset download status
        _isEmulatorDownloaded = false;
        _isCoreDownloaded = !DownloadCoreButton.IsEnabled;

        UpdateAddSystemButtonState();

        // Automatically populate the SystemFolder by the default path
        var applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemName = SystemNameDropdown.SelectedItem.ToString();
        if (systemName == null) return;

        var systemFolderPath = Path.Combine(applicationDirectory, "roms", systemName);
        SystemFolderTextBox.Text = systemFolderPath;
    }

    private async void DownloadEmulatorButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadEmulatorButton.IsEnabled = false;
            _isEmulatorDownloaded = false;
            UpdateAddSystemButtonState();

            var success = await DownloadAndExtractAsync(DownloadType.Emulator);

            if (success)
            {
                _isEmulatorDownloaded = true;
            }
            else
            {
                // Re-enable the button unless it was a successful download
                DownloadEmulatorButton.IsEnabled = true;
            }

            UpdateAddSystemButtonState();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error downloading emulator.");
        }
    }

    private async void DownloadCoreButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadCoreButton.IsEnabled = false;
            _isCoreDownloaded = false;
            UpdateAddSystemButtonState();

            var success = await DownloadAndExtractAsync(DownloadType.Core);

            if (success)
            {
                _isCoreDownloaded = true;
            }
            else
            {
                // Re-enable the button unless it was a successful download
                DownloadCoreButton.IsEnabled = true;
            }

            UpdateAddSystemButtonState();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error downloading core.");
        }
    }

    private async void DownloadImagePackButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadExtrasButton.IsEnabled = false;

            var success = await DownloadAndExtractAsync(DownloadType.ImagePack);

            // Re-enable the button if not successful
            if (!success)
            {
                DownloadExtrasButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error downloading image pack.");
        }
    }

    private async Task<bool> DownloadAndExtractAsync(DownloadType downloadType)
    {
        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return false;

        string downloadUrl;
        string destinationPath;
        string componentName;

        // Configure based on the download type
        switch (downloadType)
        {
            case DownloadType.Emulator:
                downloadUrl = selectedSystem.Emulators.Emulator.EmulatorDownloadLink;
                destinationPath = selectedSystem.Emulators.Emulator.EmulatorDownloadExtractPath;
                componentName = "Emulator";
                break;
            case DownloadType.Core:
                downloadUrl = selectedSystem.Emulators.Emulator.CoreDownloadLink;
                destinationPath = selectedSystem.Emulators.Emulator.CoreDownloadExtractPath;
                componentName = "Core";
                break;
            case DownloadType.ImagePack:
                downloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;

                // Determine the extraction folder
                var imagePath = selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;
                destinationPath = PathHelper.ResolveRelativeToAppDirectory(imagePath);
                componentName = "Image Pack";

                break;
            default:
                return false;
        }

        // Ensure valid URL
        if (string.IsNullOrEmpty(downloadUrl))
        {
            var errorNodownloadUrLfor2 = (string)Application.Current.TryFindResource("ErrorNodownloadURLfor") ?? "Error: No download URL for";
            DownloadStatus = $"{errorNodownloadUrLfor2} {componentName}";
            return false;
        }

        try
        {
            // Reset status
            var preparingtodownload2 = (string)Application.Current.TryFindResource("Preparingtodownload") ?? "Preparing to download";
            DownloadStatus = $"{preparingtodownload2} {componentName}...";

            // Display progress bar
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StopDownloadButton.IsEnabled = true;

            // Download and extract
            var success = false; // Initialize variable

            var pleaseWaitWindow = new PleaseWaitExtractionWindow();

            // Use the DownloadAndExtractAsync method in DownloadManager
            var downloading2 = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            DownloadStatus = $"{downloading2} {componentName}...";

            // First download
            var downloadedFile = await _downloadManager.DownloadFileAsync(downloadUrl);

            if (downloadedFile != null && _downloadManager.IsDownloadCompleted)
            {
                // Then extract
                var extracting2 = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                DownloadStatus = $"{extracting2} {componentName}...";

                pleaseWaitWindow.Show();
                success = await _downloadManager.ExtractFileAsync(downloadedFile, destinationPath);
                pleaseWaitWindow.Close();
            }

            // Update UI based on the result
            if (success)
            {
                // Notify user
                var hasbeensuccessfullydownloadedandinstalled2 = (string)Application.Current.TryFindResource("hasbeensuccessfullydownloadedandinstalled") ?? "has been successfully downloaded and installed.";
                DownloadStatus = $"{componentName} {hasbeensuccessfullydownloadedandinstalled2}";
                MessageBoxLibrary.DownloadAndExtrationWereSuccessfulMessageBox();

                StopDownloadButton.IsEnabled = false;
                return true;
            }
            else
            {
                if (_downloadManager.IsUserCancellation)
                {
                    var downloadof2 = (string)Application.Current.TryFindResource("Downloadof") ?? "Download of";
                    var wascanceled2 = (string)Application.Current.TryFindResource("wascanceled") ?? "was canceled.";
                    DownloadStatus = $"{downloadof2} {componentName} {wascanceled2}";
                }
                else
                {
                    // Log extraction failure
                    var errorFailedtoextract2 = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    DownloadStatus = $"{errorFailedtoextract2} {componentName}.";

                    // Show an error message based on component type
                    switch (downloadType)
                    {
                        case DownloadType.Emulator:
                            await MessageBoxLibrary.EmulatorDownloadErrorMessageBox(selectedSystem);
                            break;
                        case DownloadType.Core:
                            await MessageBoxLibrary.CoreDownloadErrorMessageBox(selectedSystem);
                            break;
                        case DownloadType.ImagePack:
                            await MessageBoxLibrary.ImagePackDownloadErrorMessageBox(selectedSystem);
                            break;
                        default:
                            MessageBoxLibrary.DownloadExtractionFailedMessageBox();
                            break;
                    }
                }

                StopDownloadButton.IsEnabled = false;
                return false;
            }
        }
        catch (Exception ex)
        {
            var errorduring2 = (string)Application.Current.TryFindResource("Errorduring") ?? "Error during";
            var downloadprocess2 = (string)Application.Current.TryFindResource("downloadprocess") ?? "download process.";
            DownloadStatus = $"{errorduring2} {componentName} {downloadprocess2}";

            // Notify developer
            var contextMessage = $"Error downloading {componentName}.\n" +
                                 $"URL: {downloadUrl}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            switch (downloadType)
            {
                case DownloadType.Emulator:
                    await MessageBoxLibrary.EmulatorDownloadErrorMessageBox(selectedSystem);
                    break;
                case DownloadType.Core:
                    await MessageBoxLibrary.CoreDownloadErrorMessageBox(selectedSystem);
                    break;
                case DownloadType.ImagePack:
                    await MessageBoxLibrary.ImagePackDownloadErrorMessageBox(selectedSystem);
                    break;
                default:
                    MessageBoxLibrary.DownloadExtractionFailedMessageBox();
                    break;
            }

            StopDownloadButton.IsEnabled = false;
            return false;
        }
    }

    private void DownloadManager_ProgressChanged(object sender, DownloadManager.DownloadProgressEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            DownloadProgressBar.Value = e.ProgressPercentage;
            DownloadStatus = e.StatusMessage;
        });
    }

    private EasyModeSystemConfig GetSelectedSystem()
    {
        return SystemNameDropdown.SelectedItem != null
            ? _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString())
            : null;
    }

    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        // Cancel the download
        _downloadManager.CancelDownload();

        // Update UI
        var cancelingdownload2 = (string)Application.Current.TryFindResource("Cancelingdownload") ?? "Canceling download...";
        DownloadStatus = cancelingdownload2;
        StopDownloadButton.IsEnabled = false;

        // Reset progress
        DownloadProgressBar.Value = 0;
    }

    private void AddSystemButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return;

        // Determine the system folder to use
        var systemFolder = SystemFolderTextBox.Text;
        if (string.IsNullOrEmpty(systemFolder))
        {
            systemFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roms", selectedSystem.SystemName);
            SystemFolderTextBox.Text = systemFolder;
        }

        // // Remove the leading dot from the SystemImageFolder for the message
        // var systemImageFolderForMessage = selectedSystem.SystemImageFolder.TrimStart('.').TrimStart('\\', '/');
        //
        // // Combine with the base directory for the message
        // var fullImageFolderPathForMessage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, systemImageFolderForMessage);

        var systemImageFolderRaw = selectedSystem.SystemImageFolder ?? string.Empty;

        string systemImageFolderAbsolute;

        if (Path.IsPathRooted(systemImageFolderRaw))
        {
            systemImageFolderAbsolute = PathHelper.ResolveRelativeToCurrentDirectory(systemImageFolderRaw);
        }
        else
        {
            // Combine the relative path with the base directory to get the absolute path
            systemImageFolderAbsolute = PathHelper.ResolveRelativeToAppDirectory(systemImageFolderRaw);
        }

        var addingsystemtoconfiguration2 = (string)Application.Current.TryFindResource("Addingsystemtoconfiguration") ?? "Adding system to configuration...";
        DownloadStatus = addingsystemtoconfiguration2;

        // Path to the system.xml file
        var systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

        try
        {
            // Load existing system configurations
            var xmlDoc = XDocument.Load(systemXmlPath);
            var systemConfigs = xmlDoc.Descendants("SystemConfig").ToList();

            // Check if the system already exists
            var existingSystem = systemConfigs.FirstOrDefault(config => config.Element("SystemName")?.Value == selectedSystem.SystemName);
            if (existingSystem != null)
            {
                OverwriteExistingSystem(existingSystem, selectedSystem, systemFolder, systemImageFolderAbsolute);
            }
            else
            {
                var newSystemElement = SaveNewSystem(selectedSystem, systemFolder, systemImageFolderAbsolute);

                // Add the new system to the XML document
                xmlDoc.Root?.Add(newSystemElement);
            }

            // Sort the systems alphabetically by SystemName
            xmlDoc.Root?.ReplaceNodes(xmlDoc.Root.Elements("SystemConfig")
                .OrderBy(static systemElement => systemElement.Element("SystemName")?.Value));

            // Save the updated XML document
            xmlDoc.Save(systemXmlPath);

            var creatingsystemfolders2 = (string)Application.Current.TryFindResource("Creatingsystemfolders") ?? "Creating system folders...";
            DownloadStatus = creatingsystemfolders2;

            // Create the necessary folders for the system
            CreateSystemFolders(selectedSystem.SystemName, systemFolder, systemImageFolderAbsolute);

            var systemhasbeensuccessfullyadded2 = (string)Application.Current.TryFindResource("Systemhasbeensuccessfullyadded") ?? "System has been successfully added!";
            DownloadStatus = systemhasbeensuccessfullyadded2;

            // Notify user
            MessageBoxLibrary.SystemAddedMessageBox(systemFolder, systemImageFolderAbsolute, selectedSystem);

            // Disable Add System Button
            AddSystemButton.IsEnabled = false;

            // Close window
            Close();
        }
        catch (Exception ex)
        {
            var errorFailedtoaddsystem2 = (string)Application.Current.TryFindResource("ErrorFailedtoaddsystem") ?? "Error: Failed to add system.";
            DownloadStatus = errorFailedtoaddsystem2;

            // Notify developer
            const string contextMessage = "Error adding system.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.AddSystemFailedMessageBox();
        }
    }

    private static XElement SaveNewSystem(EasyModeSystemConfig selectedSystem, string systemFolder,
        string systemImageFolderAbsolute)
    {
        // Create a new XElement for the selected system
        var newSystemElement = new XElement("SystemConfig",
            new XElement("SystemName", selectedSystem.SystemName),
            new XElement("SystemFolder", systemFolder),
            new XElement("SystemImageFolder", systemImageFolderAbsolute),
            new XElement("SystemIsMAME", selectedSystem.SystemIsMame.ToString()),
            new XElement("FileFormatsToSearch", selectedSystem.FileFormatsToSearch.Select(static format => new XElement("FormatToSearch", format))),
            new XElement("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch.ToString()),
            new XElement("FileFormatsToLaunch", selectedSystem.FileFormatsToLaunch.Select(static format => new XElement("FormatToLaunch", format))),
            new XElement("Emulators",
                new XElement("Emulator",
                    new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                    new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                    new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
                )
            )
        );
        return newSystemElement;
    }

    private static void OverwriteExistingSystem(XElement existingSystem, EasyModeSystemConfig selectedSystem,
        string systemFolder, string systemImageFolderAbsolute)
    {
        // Ask user if they want to overwrite the existing system
        // if (MessageBoxLibrary.OverwriteSystemMessageBox(selectedSystem)) return;

        // Overwrite existing system
        existingSystem.SetElementValue("SystemName", selectedSystem.SystemName);
        existingSystem.SetElementValue("SystemFolder", systemFolder);
        existingSystem.SetElementValue("SystemImageFolder", systemImageFolderAbsolute);
        existingSystem.SetElementValue("SystemIsMAME", selectedSystem.SystemIsMame.ToString());
        existingSystem.Element("FileFormatsToSearch")?.Remove();
        existingSystem.Add(new XElement("FileFormatsToSearch", selectedSystem.FileFormatsToSearch.Select(static format => new XElement("FormatToSearch", format))));
        existingSystem.SetElementValue("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch.ToString());
        existingSystem.Element("FileFormatsToLaunch")?.Remove();
        existingSystem.Add(new XElement("FileFormatsToLaunch", selectedSystem.FileFormatsToLaunch.Select(static format => new XElement("FormatToLaunch", format))));
        existingSystem.Element("Emulators")?.Remove();
        existingSystem.Add(new XElement("Emulators",
            new XElement("Emulator",
                new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
            )
        ));
    }

    private void UpdateAddSystemButtonState()
    {
        AddSystemButton.IsEnabled = _isEmulatorDownloaded && _isCoreDownloaded;
    }

    private static void CreateSystemFolders(string systemName, string systemFolder, string systemImageFolder)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Paths for the primary system folder and image folder
        var systemFolderPath = PathHelper.ResolveRelativeToCurrentDirectory(systemFolder);
        var imagesFolderPath = PathHelper.ResolveRelativeToCurrentDirectory(systemImageFolder);

        // List of additional folders to create
        string[] additionalFolders = ["roms", "images", "title_snapshots", "gameplay_snapshots", "videos", "manuals", "walkthrough", "cabinets", "carts", "flyers", "pcbs"];

        try
        {
            // Create the primary system folder if it doesn't exist
            if (!Directory.Exists(systemFolderPath))
            {
                IoOperations.CreateDirectory(systemFolderPath);
            }

            // Create the primary image folder if it doesn't exist
            if (!Directory.Exists(imagesFolderPath))
            {
                IoOperations.CreateDirectory(imagesFolderPath);
            }

            // Create each additional folder
            foreach (var folder in additionalFolders)
            {
                var folderPath = Path.Combine(baseDirectory, folder, systemName);
                if (!Directory.Exists(folderPath))
                {
                    IoOperations.CreateDirectory(folderPath);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "The application failed to create the necessary folders for the newly added system.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FolderCreationFailedMessageBox();

            throw;
        }
    }

    private void CloseWindowRoutine(object sender, EventArgs e)
    {
        _manager = null;
        Dispose();
    }

    private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var chooseaFolderwithRoMsorIsOs2 = (string)Application.Current.TryFindResource("ChooseaFolderwithROMsorISOs") ?? "Choose a folder with 'ROMs' or 'ISOs' for this system";

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
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _downloadManager?.Dispose();

        // NOTE: _manager (EasyModeManager) doesn't implement IDisposable
        // If EasyModeManager implements IDisposable in the future, it should be disposed here

        _disposed = true;

        // Tell GC not to call the finalizer since we've already cleaned up
        GC.SuppressFinalize(this);
    }
}