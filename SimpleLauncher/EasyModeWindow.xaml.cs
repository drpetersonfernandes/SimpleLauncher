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
using System.Xml;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;

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
        // if (_manager?.Systems == null) return;

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
        DownloadImagePackButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ImagePackDownloadLink);

        // Reset download status
        _isEmulatorDownloaded = false;
        // _isCoreDownloaded = !DownloadCoreButton.IsEnabled;
        _isCoreDownloaded = string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.CoreDownloadLink);

        UpdateAddSystemButtonState();

        // Automatically populate the SystemFolder by the default path
        var applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemName = SystemNameDropdown.SelectedItem.ToString();
        if (systemName == null) return;

        // Sanitize SystemName
        var sanitizedSystemName = SanitizePaths.SanitizeFolderName(systemName);

        var systemFolderPath = Path.Combine(applicationDirectory, "roms", sanitizedSystemName);
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
            DownloadImagePackButton.IsEnabled = false;

            var success = await DownloadAndExtractAsync(DownloadType.ImagePack);

            // Re-enable the button if not successful
            if (!success)
            {
                DownloadImagePackButton.IsEnabled = true;
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
                destinationPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystem.Emulators.Emulator.EmulatorDownloadExtractPath);
                componentName = "Emulator";
                break;
            case DownloadType.Core:
                downloadUrl = selectedSystem.Emulators.Emulator.CoreDownloadLink;
                destinationPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystem.Emulators.Emulator.CoreDownloadExtractPath);
                componentName = "Core";
                break;
            case DownloadType.ImagePack:
                downloadUrl = selectedSystem.Emulators.Emulator.ImagePackDownloadLink;

                // Determine the extraction folder
                destinationPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath);
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

            var extracting2 = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
            var pleaseWaitWindow = new PleaseWaitWindow($"{extracting2} {componentName}...");
            pleaseWaitWindow.Owner = this;

            // Use the DownloadAndExtractAsync method in DownloadManager
            var downloading2 = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            DownloadStatus = $"{downloading2} {componentName}...";

            // First download
            var downloadedFile = await _downloadManager.DownloadFileAsync(downloadUrl);

            if (downloadedFile != null && _downloadManager.IsDownloadCompleted)
            {
                // Then extract
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

    // Helper method to save data into the XML
    private static async Task UpdateSystemXmlAsync(
        string xmlPath,
        EasyModeSystemConfig selectedSystem,
        string systemFolder,
        string systemImageFolderAbsolute)
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
                            _ = LogErrors.LogErrorAsync(
                                new XmlException("Loaded system.xml has missing or invalid root element."),
                                "Invalid root in system.xml, creating new.");
                        }
                    }
                }
                catch (XmlException ex) // Catch specific XML parsing errors
                {
                    // Log the parsing error but proceed to create a new document
                    _ = LogErrors.LogErrorAsync(ex, "Error parsing existing system.xml, creating new.");
                    xmlDoc = null; // Ensure we create a new one
                }
                catch (Exception ex) // Catch other file reading errors
                {
                    _ = LogErrors.LogErrorAsync(ex, "Error reading existing system.xml.");
                    throw new IOException("Could not read the existing system configuration file.",
                        ex); // Rethrow as IO
                }
            }

            // If xmlDoc is still null (file didn't exist, was empty, or had invalid root), create a new one
            xmlDoc ??= new XDocument(new XElement("SystemConfigs"));
            // --- Proceed with modification logic ---
            if (xmlDoc.Root != null)
            {
                var systemConfigs =
                    xmlDoc.Root.Descendants("SystemConfig").ToList(); // Safe now because Root is guaranteed
                var existingSystem = systemConfigs.FirstOrDefault(config =>
                    config.Element("SystemName")?.Value == selectedSystem.SystemName);
                if (existingSystem != null)
                {
                    // Overwrite existing system (in memory)
                    OverwriteExistingSystem(existingSystem, selectedSystem, systemFolder, systemImageFolderAbsolute);
                }
                else
                {
                    // Create new system element (in memory)
                    var newSystemElement = SaveNewSystem(selectedSystem, systemFolder, systemImageFolderAbsolute);
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

            // Save the updated and sorted XML document asynchronously
            if (xmlPath != null)
            {
                await File.WriteAllTextAsync(xmlPath, xmlDoc.ToString());
            }
        }
        catch (IOException ex) // Handle file saving errors (permissions, disk full, etc.)
        {
            const string contextMessage = "Error saving system.xml.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
            throw new InvalidOperationException("Could not save system configuration.", ex);
        }
        catch (Exception ex) // Catch other potential errors
        {
            const string contextMessage = "Unexpected error updating system.xml.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
            throw new InvalidOperationException("An unexpected error occurred while updating system configuration.",
                ex);
        }
    }

    private async void AddSystemButton_Click(object sender, RoutedEventArgs e)
    {
        try
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

            // Resolve System Image Folder Path
            var systemImageFolderRaw = selectedSystem.SystemImageFolder ?? string.Empty;
            string systemImageFolderAbsolute;

            if (Path.IsPathRooted(systemImageFolderRaw))
            {
                // Use ResolveRelativeToCurrentDirectory for consistency if it's already absolute
                systemImageFolderAbsolute = PathHelper.ResolveRelativeToCurrentDirectory(systemImageFolderRaw);
            }
            else
            {
                // Resolve relative path against the app directory
                systemImageFolderAbsolute = PathHelper.ResolveRelativeToAppDirectory(systemImageFolderRaw);
            }

            var addingsystemtoconfiguration2 = (string)Application.Current.TryFindResource("Addingsystemtoconfiguration") ?? "Adding system to configuration...";
            DownloadStatus = addingsystemtoconfiguration2;

            var systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

            // --- Start Async Operation ---
            try
            {
                // Disable button during operation
                AddSystemButton.IsEnabled = false;

                // Call the asynchronous helper method to update the XML
                await UpdateSystemXmlAsync(systemXmlPath, selectedSystem, systemFolder, systemImageFolderAbsolute);

                // --- If XML update succeeds, continue with folder creation and UI updates ---
                var creatingsystemfolders2 = (string)Application.Current.TryFindResource("Creatingsystemfolders") ?? "Creating system folders...";
                DownloadStatus = creatingsystemfolders2;

                // Create the necessary folders for the system
                CreateSystemFolders(selectedSystem.SystemName, systemFolder, systemImageFolderAbsolute);

                var systemhasbeensuccessfullyadded2 = (string)Application.Current.TryFindResource("Systemhasbeensuccessfullyadded") ?? "System has been successfully added!";
                DownloadStatus = systemhasbeensuccessfullyadded2;

                // Notify user
                MessageBoxLibrary.SystemAddedMessageBox(systemFolder, systemImageFolderAbsolute, selectedSystem);

                // Close the window after successful addition
                Close();
            }
            catch (InvalidOperationException ex) // Catch specific exceptions from the helper
            {
                var errorFailedtoaddsystem = (string)Application.Current.TryFindResource("ErrorFailedtoaddsystem") ?? "Error: Failed to add system.";
                DownloadStatus = $"{errorFailedtoaddsystem} {ex.Message}"; // Include details from exception

                // Error is already logged by the helper method.
                // Notify user about the specific failure reason.
                MessageBoxLibrary.AddSystemFailedMessageBox(ex.Message); // Pass details to user
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                var errorFailedtoaddsystem = (string)Application.Current.TryFindResource("ErrorFailedtoaddsystem") ?? "Error: Failed to add system.";
                DownloadStatus = errorFailedtoaddsystem;

                // Log unexpected error
                const string contextMessage = "Unexpected error adding system.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.AddSystemFailedMessageBox(); // Generic message for unexpected errors
            }
            finally
            {
                // Re-enable the button only if the operation failed and the window didn't close
                // If Close() was called on success, this won't execute for that button instance.
                // If an error occurred, re-enable the button.
                if (IsLoaded) // Check if the window is still loaded
                {
                    AddSystemButton.IsEnabled = true;
                }
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error adding system.");
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
        var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemFolder);
        var imagesFolderPath = PathHelper.ResolveRelativeToAppDirectory(systemImageFolder);

        // List of additional folders to create from appsettings.json
        var additionalFolders = GetAdditionalFolders.GetFolders();

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
            _ = LogErrors.LogErrorAsync(ex, "Error opening the download link.");

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
