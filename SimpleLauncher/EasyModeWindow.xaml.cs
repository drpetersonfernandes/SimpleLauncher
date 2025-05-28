using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Net.Http;
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

namespace SimpleLauncher;

public partial class EasyModeWindow : IDisposable
{
    private EasyModeManager _manager;
    private bool _isEmulatorDownloaded;
    private bool _isCoreDownloaded;
    private readonly DownloadManager _downloadManager;
    private bool _disposed;

    private readonly string _basePath = AppDomain.CurrentDomain.BaseDirectory;

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

        // Get the factory from the service provider
        var httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();

        // Initialize the DownloadManager, passing the factory
        _downloadManager = new DownloadManager(httpClientFactory);
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
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Assign an empty list if there's any error
            SystemNameDropdown.ItemsSource = new List<string>();
        }
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem == null)
        {
            return;
        }

        var selectedSystem = _manager.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem == null)
        {
            return;
        }

        DownloadEmulatorButton.IsEnabled = true;
        DownloadCoreButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.CoreDownloadLink);
        DownloadImagePackButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink);

        // Reset download status
        _isEmulatorDownloaded = false;
        // _isCoreDownloaded = !DownloadCoreButton.IsEnabled;
        _isCoreDownloaded = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.CoreDownloadLink);

        UpdateAddSystemButtonState();

        // Populate SystemFolder with value from easymode.xml
        var originalSystemFolder = selectedSystem.SystemFolder;
        var fixedSystemFolder = originalSystemFolder.Replace("%SIMPLELAUNCHERFOLDER%", _basePath);
        var finalSystemFolder = Path.GetFullPath(fixedSystemFolder);
        SystemFolderTextBox.Text = finalSystemFolder;
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
            // Notify developer
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
            // Notify developer
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
            // Notify developer
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

                var emulatorDownloadExtractPath = selectedSystem.Emulators.Emulator.EmulatorDownloadExtractPath;
                var fixedEmulatorDownloadExtractPath = emulatorDownloadExtractPath.Replace("%SIMPLELAUNCHERFOLDER%", _basePath);
                var finalEmulatorDownloadExtractPath = Path.GetFullPath(fixedEmulatorDownloadExtractPath);

                destinationPath = finalEmulatorDownloadExtractPath;
                componentName = "Emulator";
                break;
            case DownloadType.Core:
                downloadUrl = selectedSystem.Emulators.Emulator.CoreDownloadLink;

                var coreDownloadExtractPath = selectedSystem.Emulators.Emulator.CoreDownloadExtractPath;
                var fixedCoreDownloadExtractPath = coreDownloadExtractPath.Replace("%SIMPLELAUNCHERFOLDER%", _basePath);
                var finalCoreDownloadExtractPath = Path.GetFullPath(fixedCoreDownloadExtractPath);

                destinationPath = finalCoreDownloadExtractPath;
                componentName = "Core";
                break;
            case DownloadType.ImagePack:
                downloadUrl = selectedSystem.Emulators.Emulator.ImagePackDownloadLink;

                var imagePackDownloadExtractPath = selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath;
                var fixedImagePackDownloadExtractPath = imagePackDownloadExtractPath.Replace("%SIMPLELAUNCHERFOLDER%", _basePath);
                var finalImagePackDownloadExtractPath = Path.GetFullPath(fixedImagePackDownloadExtractPath);

                destinationPath = finalImagePackDownloadExtractPath;
                componentName = "Image Pack";
                break;
            default:
                return false;
        }

        // Ensure valid URL
        if (string.IsNullOrEmpty(downloadUrl))
        {
            var errorNodownloadUrLfor = (string)Application.Current.TryFindResource("ErrorNodownloadURLfor") ?? "Error: No download URL for";
            DownloadStatus = $"{errorNodownloadUrLfor} {componentName}";
            return false;
        }

        try
        {
            // Reset status
            var preparingtodownload = (string)Application.Current.TryFindResource("Preparingtodownload") ?? "Preparing to download";
            DownloadStatus = $"{preparingtodownload} {componentName}...";

            // Display progress bar
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StopDownloadButton.IsEnabled = true;

            // Download and extract
            var success = false; // Initialize variable

            var extracting = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
            var pleaseWaitWindow = new PleaseWaitWindow($"{extracting} {componentName}...");
            pleaseWaitWindow.Owner = this;

            // Use the DownloadAndExtractAsync method in DownloadManager
            var downloading = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            DownloadStatus = $"{downloading} {componentName}...";

            // First download
            var downloadedFile = await _downloadManager.DownloadFileAsync(downloadUrl);

            if (downloadedFile != null && _downloadManager.IsDownloadCompleted)
            {
                // Then extract
                DownloadStatus = $"{extracting} {componentName}...";

                pleaseWaitWindow.Show();
                success = await _downloadManager.ExtractFileAsync(downloadedFile, destinationPath);
                pleaseWaitWindow.Close();
            }

            // Update UI based on the result
            if (success)
            {
                // Notify user
                var hasbeensuccessfullydownloadedandinstalled = (string)Application.Current.TryFindResource("hasbeensuccessfullydownloadedandinstalled") ?? "has been successfully downloaded and installed.";
                DownloadStatus = $"{componentName} {hasbeensuccessfullydownloadedandinstalled}";
                MessageBoxLibrary.DownloadAndExtrationWereSuccessfulMessageBox();

                StopDownloadButton.IsEnabled = false;
                return true;
            }
            else
            {
                if (_downloadManager.IsUserCancellation)
                {
                    var downloadof = (string)Application.Current.TryFindResource("Downloadof") ?? "Download of";
                    var wascanceled = (string)Application.Current.TryFindResource("wascanceled") ?? "was canceled.";
                    DownloadStatus = $"{downloadof} {componentName} {wascanceled}";
                }
                else
                {
                    // Log extraction failure
                    var errorFailedtoextract = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    DownloadStatus = $"{errorFailedtoextract} {componentName}.";

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

    private static async Task UpdateSystemXmlAsync(
        string xmlPath,
        EasyModeSystemConfig selectedSystem,
        string finalSystemFolder,
        string finalSystemImageFolder)
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
                            _ = LogErrors.LogErrorAsync(new XmlException("Loaded system.xml has missing or invalid root element."), "Invalid root in system.xml, creating new.");
                        }
                    }
                }
                catch (XmlException ex) // Catch specific XML parsing errors
                {
                    // Notify developer
                    // Log the parsing error but proceed to create a new document
                    _ = LogErrors.LogErrorAsync(ex, "Error parsing existing system.xml, creating new.");
                    xmlDoc = null; // Ensure we create a new one
                }
                catch (Exception ex) // Catch other file reading errors
                {
                    // Notify developer
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
                    OverwriteExistingSystem(existingSystem, selectedSystem, finalSystemFolder, finalSystemImageFolder);
                }
                else
                {
                    // Create new system element (in memory)
                    var newSystemElement = SaveNewSystem(selectedSystem, finalSystemFolder, finalSystemImageFolder);
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

            // Save the updated and sorted XML document asynchronously with formatting
            // Use SaveOptions.None for default indentation
            await Task.Run(() =>
            {
                if (xmlPath != null) xmlDoc.Save(xmlPath, SaveOptions.None);
            });
        }
        catch (IOException ex) // Handle file saving errors (permissions, disk full, etc.)
        {
            // Notify developer
            const string contextMessage = "Error saving system.xml.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            throw new InvalidOperationException("Could not save system configuration.", ex);
        }
        catch (Exception ex) // Catch other potential errors
        {
            // Notify developer
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
            var finalSystemFolder = SystemFolderTextBox.Text;

            // Resolve System Image Folder Path
            var originalSystemImageFolder = selectedSystem.SystemImageFolder;
            var fixedSystemImageFolder = originalSystemImageFolder.Replace("%SIMPLELAUNCHERFOLDER%", _basePath);
            var finalSystemImageFolder = Path.GetFullPath(fixedSystemImageFolder);

            var addingsystemtoconfiguration = (string)Application.Current.TryFindResource("Addingsystemtoconfiguration") ?? "Adding system to configuration...";
            DownloadStatus = addingsystemtoconfiguration;

            var systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

            // --- Start Async Operation ---
            try
            {
                // Disable button during operation
                AddSystemButton.IsEnabled = false;

                // Call the asynchronous helper method to update the XML
                await UpdateSystemXmlAsync(systemXmlPath, selectedSystem, finalSystemFolder, finalSystemImageFolder);

                // --- If XML update succeeds, continue with folder creation and UI updates ---
                var creatingsystemfolders = (string)Application.Current.TryFindResource("Creatingsystemfolders") ?? "Creating system folders...";
                DownloadStatus = creatingsystemfolders;

                // Create the necessary folders for the system
                CreateSystemFolders(selectedSystem.SystemName, finalSystemFolder, finalSystemImageFolder);

                var systemhasbeensuccessfullyadded = (string)Application.Current.TryFindResource("Systemhasbeensuccessfullyadded") ?? "System has been successfully added!";
                DownloadStatus = systemhasbeensuccessfullyadded;

                // Notify user
                MessageBoxLibrary.SystemAddedMessageBox(finalSystemFolder, finalSystemImageFolder, selectedSystem);

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
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.AddSystemFailedMessageBox();
            }
            finally
            {
                if (IsLoaded) // Check if the window is still loaded
                {
                    AddSystemButton.IsEnabled = true;
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error adding system.");
        }
    }

    private static XElement SaveNewSystem(EasyModeSystemConfig selectedSystem, string finalSystemFolder, string finalSystemImageFolder)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var sanitizedBasePath = PathHelper.SanitizePathToken(basePath);

        var emulatorLocation = selectedSystem.Emulators.Emulator.EmulatorLocation;
        var fixedEmulatorLocation = emulatorLocation.Replace("%SIMPLELAUNCHERFOLDER%", sanitizedBasePath);
        var finalEmulatorLocation = Path.GetFullPath(fixedEmulatorLocation);
        var finalEmulatorLocationFolderName = Path.GetDirectoryName(finalEmulatorLocation);

        var emulatorParameters = selectedSystem.Emulators.Emulator.EmulatorParameters;
        var fixedEmulatorParameters = emulatorParameters.Replace("%SIMPLELAUNCHERFOLDER%", sanitizedBasePath);
        var fixedEmulatorParameters2 = fixedEmulatorParameters.Replace("%EMULATORFOLDER%", finalEmulatorLocationFolderName);
        var finalEmulatorParameters = fixedEmulatorParameters2.Replace("%SYSTEMFOLDER%", finalSystemFolder);

        // Create a new XElement for the selected system
        var newSystemElement = new XElement("SystemConfig",
            new XElement("SystemName", selectedSystem.SystemName),
            new XElement("SystemFolder", finalSystemFolder),
            new XElement("SystemImageFolder", finalSystemImageFolder),
            new XElement("SystemIsMAME", selectedSystem.SystemIsMame.ToString()),
            new XElement("FileFormatsToSearch", selectedSystem.FileFormatsToSearch.Select(static format => new XElement("FormatToSearch", format))),
            new XElement("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch.ToString()),
            new XElement("FileFormatsToLaunch", selectedSystem.FileFormatsToLaunch.Select(static format => new XElement("FormatToLaunch", format))),
            new XElement("Emulators",
                new XElement("Emulator",
                    new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                    new XElement("EmulatorLocation", finalEmulatorLocation),
                    new XElement("EmulatorParameters", finalEmulatorParameters)
                )
            )
        );
        return newSystemElement;
    }

    private static void OverwriteExistingSystem(XElement existingSystem, EasyModeSystemConfig selectedSystem, string finalSystemFolder, string finalSystemImageFolder)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        var sanitizedBasePath = PathHelper.SanitizePathToken(basePath);

        var emulatorLocation = selectedSystem.Emulators.Emulator.EmulatorLocation;
        var fixedEmulatorLocation = emulatorLocation.Replace("%SIMPLELAUNCHERFOLDER%", sanitizedBasePath);
        var finalEmulatorLocation = Path.GetFullPath(fixedEmulatorLocation);
        var finalEmulatorLocationFolderName = Path.GetDirectoryName(finalEmulatorLocation);

        var emulatorParameters = selectedSystem.Emulators.Emulator.EmulatorParameters;
        var fixedEmulatorParameters = emulatorParameters.Replace("%SIMPLELAUNCHERFOLDER%", sanitizedBasePath);
        var fixedEmulatorParameters2 = fixedEmulatorParameters.Replace("%EMULATORFOLDER%", finalEmulatorLocationFolderName);
        var finalEmulatorParameters = fixedEmulatorParameters2.Replace("%SYSTEMFOLDER%", finalSystemFolder);

        // Overwrite existing system
        existingSystem.SetElementValue("SystemName", selectedSystem.SystemName);
        existingSystem.SetElementValue("SystemFolder", finalSystemFolder);
        existingSystem.SetElementValue("SystemImageFolder", finalSystemImageFolder);
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
                new XElement("EmulatorLocation", finalEmulatorLocation),
                new XElement("EmulatorParameters", finalEmulatorParameters)
            )
        ));
    }

    private void UpdateAddSystemButtonState()
    {
        AddSystemButton.IsEnabled = _isEmulatorDownloaded && _isCoreDownloaded;
    }

    private static void CreateSystemFolders(string systemName, string finalSystemFolder, string finalSystemImageFolder)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Paths for the primary system folder and image folder
        var systemFolderPath = PathHelper.ResolveRelativeToAppDirectory(finalSystemFolder);
        var imagesFolderPath = PathHelper.ResolveRelativeToAppDirectory(finalSystemImageFolder);

        // List of additional folders to create from appsettings.json
        var additionalFolders = GetAdditionalFolders.GetFolders();

        try
        {
            // Create the primary system folder if it doesn't exist
            if (!Directory.Exists(systemFolderPath))
            {
                try
                {
                    if (systemFolderPath != null) Directory.CreateDirectory(systemFolderPath);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, "Error creating the primary system folder.");
                }
            }

            // Create the primary image folder if it doesn't exist
            if (!Directory.Exists(imagesFolderPath))
            {
                try
                {
                    if (imagesFolderPath != null) Directory.CreateDirectory(imagesFolderPath);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, "Error creating the primary image folder.");
                }
            }

            // Create each additional folder
            foreach (var folder in additionalFolders)
            {
                var folderPath = Path.Combine(baseDirectory, folder, systemName);
                if (Directory.Exists(folderPath)) continue;

                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, $"Error creating the {folder} folder.");
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
            _ = LogErrors.LogErrorAsync(ex, "Error closing the Add System window.");
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

