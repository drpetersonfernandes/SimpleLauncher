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
    private bool _isImagePackDownloaded1;
    private bool _isImagePackDownloaded2;
    private bool _isImagePackDownloaded3;
    private bool _isImagePackDownloaded4;
    private bool _isImagePackDownloaded5;

    private readonly DownloadManager _downloadManager;
    private bool _disposed;

    private readonly string _basePath = AppDomain.CurrentDomain.BaseDirectory;

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

            SystemNameDropdown.ItemsSource = new List<string>(); // Assign an empty list if there's any error
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

        // Enable/disable Image Pack buttons based on their respective links
        DownloadImagePackButton1.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink);
        DownloadImagePackButton2.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2);
        DownloadImagePackButton3.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3);
        DownloadImagePackButton4.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4);
        DownloadImagePackButton5.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5);

        _isEmulatorDownloaded = false;
        _isCoreDownloaded = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.CoreDownloadLink); // If no core link, consider it "downloaded"
        _isImagePackDownloaded1 = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink);
        _isImagePackDownloaded2 = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2);
        _isImagePackDownloaded3 = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3);
        _isImagePackDownloaded4 = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4);
        _isImagePackDownloaded5 = string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5);

        UpdateAddSystemButtonState();

        SystemFolderTextBox.Text = selectedSystem.SystemFolder;
    }

    private async void DownloadEmulatorButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadEmulatorButton.IsEnabled = false;
            _isEmulatorDownloaded = false;
            UpdateAddSystemButtonState(); // Reflects that download is starting/not yet complete

            _isEmulatorDownloaded = await HandleDownloadAndExtractComponent(DownloadType.Emulator, DownloadEmulatorButton);
            // HandleDownloadAndExtractComponent already updates the button.IsEnabled and calls UpdateAddSystemButtonState.
            // So, no further calls needed here.
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadEmulatorButton_Click.");
            DownloadEmulatorButton.IsEnabled = true;
            _isEmulatorDownloaded = false;
            UpdateAddSystemButtonState();
        }
    }

    private async void DownloadCoreButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadCoreButton.IsEnabled = false;
            _isCoreDownloaded = false;
            UpdateAddSystemButtonState(); // Reflects that download is starting/not yet complete

            _isCoreDownloaded = await HandleDownloadAndExtractComponent(DownloadType.Core, DownloadCoreButton);
            // HandleDownloadAndExtractComponent already updates the button.IsEnabled and calls UpdateAddSystemButtonState.
            // So, no further calls needed here.
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadCoreButton_Click.");
            DownloadCoreButton.IsEnabled = true;
            _isCoreDownloaded = false;
            UpdateAddSystemButtonState();
        }
    }

    // New click handlers for each image pack button
    private async void DownloadImagePackButton1_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton1.IsEnabled = false;
            _isImagePackDownloaded1 = false;
            UpdateAddSystemButtonState();

            _isImagePackDownloaded1 = await HandleDownloadAndExtractComponent(DownloadType.ImagePack1, DownloadImagePackButton1);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton1_Click.");
            DownloadImagePackButton1.IsEnabled = true;
            _isImagePackDownloaded1 = false;
            UpdateAddSystemButtonState();
        }
    }

    private async void DownloadImagePackButton2_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton2.IsEnabled = false;
            _isImagePackDownloaded2 = false;
            UpdateAddSystemButtonState();

            _isImagePackDownloaded2 = await HandleDownloadAndExtractComponent(DownloadType.ImagePack2, DownloadImagePackButton2);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton2_Click.");
            DownloadImagePackButton2.IsEnabled = true;
            _isImagePackDownloaded2 = false;
            UpdateAddSystemButtonState();
        }
    }

    private async void DownloadImagePackButton3_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton3.IsEnabled = false;
            _isImagePackDownloaded3 = false;
            UpdateAddSystemButtonState();

            _isImagePackDownloaded3 = await HandleDownloadAndExtractComponent(DownloadType.ImagePack3, DownloadImagePackButton3);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton3_Click.");
            DownloadImagePackButton3.IsEnabled = true;
            _isImagePackDownloaded3 = false;
            UpdateAddSystemButtonState();
        }
    }

    private async void DownloadImagePackButton4_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton4.IsEnabled = false;
            _isImagePackDownloaded4 = false;
            UpdateAddSystemButtonState();

            _isImagePackDownloaded4 = await HandleDownloadAndExtractComponent(DownloadType.ImagePack4, DownloadImagePackButton4);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton4_Click.");
            DownloadImagePackButton4.IsEnabled = true;
            _isImagePackDownloaded4 = false;
            UpdateAddSystemButtonState();
        }
    }

    private async void DownloadImagePackButton5_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            DownloadImagePackButton5.IsEnabled = false;
            _isImagePackDownloaded5 = false;
            UpdateAddSystemButtonState();

            _isImagePackDownloaded5 = await HandleDownloadAndExtractComponent(DownloadType.ImagePack5, DownloadImagePackButton5);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in DownloadImagePackButton5_Click.");
            DownloadImagePackButton5.IsEnabled = true;
            _isImagePackDownloaded5 = false;
            UpdateAddSystemButtonState();
        }
    }

    // Helper method to reduce code duplication for downloads and extractions
    private async Task<bool> HandleDownloadAndExtractComponent(DownloadType type, Button buttonToDisable)
    {
        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null) return false;

        string downloadUrl;
        string componentName;
        string easyModeExtractPath;

        switch (type)
        {
            case DownloadType.Emulator:
                downloadUrl = selectedSystem.Emulators?.Emulator?.EmulatorDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.EmulatorDownloadExtractPath;
                componentName = "Emulator";
                break;
            case DownloadType.Core:
                downloadUrl = selectedSystem.Emulators?.Emulator?.CoreDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.CoreDownloadExtractPath;
                componentName = "Core";
                break;
            case DownloadType.ImagePack1:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath;
                componentName = "Image Pack 1";
                break;
            case DownloadType.ImagePack2:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath2;
                componentName = "Image Pack 2";
                break;
            case DownloadType.ImagePack3:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath3;
                componentName = "Image Pack 3";
                break;
            case DownloadType.ImagePack4:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath4;
                componentName = "Image Pack 4";
                break;
            case DownloadType.ImagePack5:
                downloadUrl = selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5;
                easyModeExtractPath = selectedSystem.Emulators?.Emulator?.ImagePackDownloadExtractPath5;
                componentName = "Image Pack 5";
                break;
            default:
                return false;
        }

        var destinationPath = PathHelper.ResolveRelativeToAppDirectory(easyModeExtractPath);

        // Ensure valid URL and destination path
        if (string.IsNullOrEmpty(downloadUrl))
        {
            var errorNodownloadUrLfor = (string)Application.Current.TryFindResource("ErrorNodownloadURLfor") ?? "Error: No download URL for";
            DownloadStatus = $"{errorNodownloadUrLfor} {componentName}";
            return false;
        }

        if (string.IsNullOrEmpty(destinationPath))
        {
            var errorInvalidDestinationPath = (string)Application.Current.TryFindResource("ErrorInvalidDestinationPath") ?? "Error: Invalid destination path for";
            DownloadStatus = $"{errorInvalidDestinationPath} {componentName}";

            // Notify developer
            _ = LogErrors.LogErrorAsync(null, $"Invalid destination path for {componentName}: {easyModeExtractPath}");

            return false;
        }

        try
        {
            // Initial state for the specific button
            buttonToDisable.IsEnabled = false;
            UpdateAddSystemButtonState(); // Update overall Add System button state based on this component's initial state

            var preparingtodownload = (string)Application.Current.TryFindResource("Preparingtodownload") ?? "Preparing to download";
            DownloadStatus = $"{preparingtodownload} {componentName}...";

            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressBar.Value = 0;
            StopDownloadButton.IsEnabled = true;

            var success = false;

            var downloading = (string)Application.Current.TryFindResource("Downloading") ?? "Downloading";
            DownloadStatus = $"{downloading} {componentName}...";

            var downloadedFile = await _downloadManager.DownloadFileAsync(downloadUrl);

            if (downloadedFile != null && _downloadManager.IsDownloadCompleted)
            {
                var extracting = (string)Application.Current.TryFindResource("Extracting") ?? "Extracting";
                DownloadStatus = $"{extracting} {componentName}...";
                LoadingMessage.Text = $"{extracting} {componentName}...";
                LoadingOverlay.Visibility = Visibility.Visible;
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
                buttonToDisable.IsEnabled = false; // Keep disabled on success
                UpdateAddSystemButtonState(); // Update overall Add System button state
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
                    var errorFailedtoextract = (string)Application.Current.TryFindResource("ErrorFailedtoextract") ?? "Error: Failed to extract";
                    DownloadStatus = $"{errorFailedtoextract} {componentName}.";

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

                StopDownloadButton.IsEnabled = false;
                buttonToDisable.IsEnabled = true; // Re-enable on failure/cancellation
                UpdateAddSystemButtonState(); // Update overall Add System button state
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

            StopDownloadButton.IsEnabled = false;
            buttonToDisable.IsEnabled = true; // Re-enable on exception
            UpdateAddSystemButtonState(); // Update overall Add System button state
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
        _downloadManager.CancelDownload();
        StopDownloadButton.IsEnabled = false;
        DownloadProgressBar.Value = 0;

        var cancelingdownload2 = (string)Application.Current.TryFindResource("Cancelingdownload") ?? "Canceling download...";
        DownloadStatus = cancelingdownload2;
    }

    private async void AddSystemButton_Click(object sender, RoutedEventArgs e)
    {
        try // Top-level catch for async void method
        {
            var selectedSystem = GetSelectedSystem();
            if (selectedSystem == null) return;

            string systemFolder;
            if (!string.IsNullOrEmpty(SystemFolderTextBox.Text) || !string.IsNullOrWhiteSpace(SystemFolderTextBox.Text))
            {
                systemFolder = SystemFolderTextBox.Text;
            }
            else
            {
                systemFolder = $"%BASEFOLDER%\\roms\\{selectedSystem.SystemName}";
                SystemFolderTextBox.Text = systemFolder;
            }

            var systemImageFolder = selectedSystem.SystemImageFolder;

            var addingsystemtoconfiguration = (string)Application.Current.TryFindResource("Addingsystemtoconfiguration") ?? "Adding system to configuration...";
            DownloadStatus = addingsystemtoconfiguration;

            var systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

            // --- Start Async Operation ---
            try
            {
                // Disable button during operation
                AddSystemButton.IsEnabled = false;

                await UpdateSystemXmlAsync(systemXmlPath, selectedSystem, systemFolder, systemImageFolder);

                // --- If XML update succeeds, continue with folder creation and UI updates ---
                var creatingsystemfolders = (string)Application.Current.TryFindResource("Creatingsystemfolders") ?? "Creating system folders...";
                DownloadStatus = creatingsystemfolders;

                // Create System Folders
                CreateSystemFolders.CreateFolders(selectedSystem.SystemName, systemFolder, systemImageFolder);

                var systemhasbeensuccessfullyadded = (string)Application.Current.TryFindResource("Systemhasbeensuccessfullyadded") ?? "System has been successfully added!";
                DownloadStatus = systemhasbeensuccessfullyadded;

                // Notify user
                var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(systemFolder);
                var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemImageFolder);
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
            _ = LogErrors.LogErrorAsync(ex, "Error in AddSystemButton_Click.");
        }
    }

    // Moved from EditSystemWindow.SaveSystem.cs and adapted for EasyModeWindow
    private async Task UpdateSystemXmlAsync(
        string xmlPath,
        EasyModeSystemConfig selectedSystem,
        string systemFolder,
        string systemImageFolder)
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

                    throw new IOException("Could not read the existing system configuration file.", ex); // Rethrow as IO
                }
            }

            // If xmlDoc is still null (file didn't exist, was empty, or had invalid root), create a new one
            xmlDoc ??= new XDocument(new XElement("SystemConfigs"));

            // --- Proceed with modification logic ---
            if (xmlDoc.Root != null)
            {
                var systemConfigs = xmlDoc.Root.Descendants("SystemConfig").ToList(); // Safe now because Root is guaranteed
                var existingSystem = systemConfigs.FirstOrDefault(config => config.Element("SystemName")?.Value == selectedSystem.SystemName);
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
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            throw new InvalidOperationException("Could not save system configuration.", ex);
        }
        catch (Exception ex) // Catch other potential errors
        {
            // Notify developer
            const string contextMessage = "Unexpected error updating system.xml.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

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
                    // Add new image pack download links and paths
                    , new XElement("ImagePackDownloadLink", selectedSystem.Emulators.Emulator.ImagePackDownloadLink)
                    , new XElement("ImagePackDownloadExtractPath", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath)
                    , new XElement("ImagePackDownloadLink2", selectedSystem.Emulators.Emulator.ImagePackDownloadLink2)
                    , new XElement("ImagePackDownloadExtractPath2", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath2)
                    , new XElement("ImagePackDownloadLink3", selectedSystem.Emulators.Emulator.ImagePackDownloadLink3)
                    , new XElement("ImagePackDownloadExtractPath3", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath3)
                    , new XElement("ImagePackDownloadLink4", selectedSystem.Emulators.Emulator.ImagePackDownloadLink4)
                    , new XElement("ImagePackDownloadExtractPath4", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath4)
                    , new XElement("ImagePackDownloadLink5", selectedSystem.Emulators.Emulator.ImagePackDownloadLink5)
                    , new XElement("ImagePackDownloadExtractPath5", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath5)
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
                // Add new image pack download links and paths
                , new XElement("ImagePackDownloadLink", selectedSystem.Emulators.Emulator.ImagePackDownloadLink)
                , new XElement("ImagePackDownloadExtractPath", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath)
                , new XElement("ImagePackDownloadLink2", selectedSystem.Emulators.Emulator.ImagePackDownloadLink2)
                , new XElement("ImagePackDownloadExtractPath2", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath2)
                , new XElement("ImagePackDownloadLink3", selectedSystem.Emulators.Emulator.ImagePackDownloadLink3)
                , new XElement("ImagePackDownloadExtractPath3", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath3)
                , new XElement("ImagePackDownloadLink4", selectedSystem.Emulators.Emulator.ImagePackDownloadLink4)
                , new XElement("ImagePackDownloadExtractPath4", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath4)
                , new XElement("ImagePackDownloadLink5", selectedSystem.Emulators.Emulator.ImagePackDownloadLink5)
                , new XElement("ImagePackDownloadExtractPath5", selectedSystem.Emulators.Emulator.ImagePackDownloadExtractPath5)
            )
        ));
    }

    private void UpdateAddSystemButtonState()
    {
        // Add System button is enabled only if emulator and core are downloaded, AND all *available* image packs are downloaded.
        var selectedSystem = GetSelectedSystem();
        if (selectedSystem == null)
        {
            AddSystemButton.IsEnabled = false;
            return;
        }

        var allRequiredImagePacksDownloaded = !(!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink) && !_isImagePackDownloaded1);

        // Check ImagePack1 if its link exists

        // Check ImagePack2 if its link exists
        if (!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink2) && !_isImagePackDownloaded2)
        {
            allRequiredImagePacksDownloaded = false;
        }

        // Check ImagePack3 if its link exists
        if (!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink3) && !_isImagePackDownloaded3)
        {
            allRequiredImagePacksDownloaded = false;
        }

        // Check ImagePack4 if its link exists
        if (!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink4) && !_isImagePackDownloaded4)
        {
            allRequiredImagePacksDownloaded = false;
        }

        // Check ImagePack5 if its link exists
        if (!string.IsNullOrEmpty(selectedSystem.Emulators?.Emulator?.ImagePackDownloadLink5) && !_isImagePackDownloaded5)
        {
            allRequiredImagePacksDownloaded = false;
        }

        AddSystemButton.IsEnabled = _isEmulatorDownloaded && _isCoreDownloaded && allRequiredImagePacksDownloaded;
    }

    private async void CloseWindowRoutine(object sender, EventArgs e)
    {
        try // Top-level catch for async void method
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
