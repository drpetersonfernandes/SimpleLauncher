using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Navigation;
using System.Xml.Linq;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace SimpleLauncher;

public partial class EditSystemEasyModeAddSystem
{
    private EasyModeConfig _config;
    private bool _isEmulatorDownloaded;
    private bool _isCoreDownloaded;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly HttpClient _httpClient = new();
    private bool _isDownloadCompleted;
        
    public EditSystemEasyModeAddSystem()
    {
        InitializeComponent();
            
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
            
        App.ApplyThemeToWindow(this);
            
        LoadConfig();
        PopulateSystemDropdown();
            
        // Subscribe to the Closed event
        Closed += EditSystemEasyModeAddSystem_Closed;

        MessageBox.Show("Some antivirus programs may lock or prevent the extraction of newly downloaded files, causing access issues during installation.\n\n" +
                        "If you encounter errors, try temporarily disabling real-time protection and run 'Simple Launcher' with administrative privileges.",
            "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void LoadConfig()
    {
        string configPath = "easymode.xml";
        _config = EasyModeConfig.Load(configPath);
    }

    private void PopulateSystemDropdown()
    {
        if (_config?.Systems != null)
        {
            var sortedSystemNames = _config.Systems.Select(system => system.SystemName)
                .OrderBy(name => name)
                .ToList();
            SystemNameDropdown.ItemsSource = sortedSystemNames;
        }
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (SystemNameDropdown.SelectedItem != null)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                DownloadEmulatorButton.IsEnabled = true;
                DownloadCoreButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.CoreDownloadLink);
                DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.ExtrasDownloadLink);

                // Reset download status
                _isEmulatorDownloaded = false;
                _isCoreDownloaded = !DownloadCoreButton.IsEnabled; // Assume downloaded if no download needed

                UpdateAddSystemButtonState();
            }
        }
    }
        
    private async void DownloadEmulatorButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _isDownloadCompleted = false;
    
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string emulatorLocation = selectedSystem.Emulators.Emulator.EmulatorLocation;
                string emulatorDownloadUrl = selectedSystem.Emulators.Emulator.EmulatorDownloadLink;
                string emulatorsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators");
                Directory.CreateDirectory(emulatorsFolderPath);
                string downloadFilePath = Path.Combine(emulatorsFolderPath, Path.GetFileName(emulatorDownloadUrl) ?? throw new InvalidOperationException("'Simple Launcher' could not get emulatorDownloadUrl"));
                string destinationPath = selectedSystem.Emulators.Emulator.EmulatorDownloadExtractPath;
                string destinationPath2 = Path.GetDirectoryName(selectedSystem.Emulators.Emulator.EmulatorLocation);
                string latestVersionString = selectedSystem.Emulators.Emulator.EmulatorLatestVersion;

                // Check if the emulator is already installed and up to date
                if (File.Exists(emulatorLocation))
                {
                    string installedVersionString = GetInstalledEmulatorVersion(emulatorLocation);
                    if (Version.TryParse(installedVersionString, out Version installedVersion) &&
                        Version.TryParse(latestVersionString, out Version latestVersion) &&
                        installedVersion.CompareTo(latestVersion) >= 0)
                    {
                        MessageBox.Show($"Emulator for {selectedSystem.SystemName} is already installed and up to date.",
                            "Emulator Already Installed", MessageBoxButton.OK, MessageBoxImage.Information);
                        _isEmulatorDownloaded = true;
                        DownloadEmulatorButton.IsEnabled = false;
                        UpdateAddSystemButtonState();
                        return;
                    }
                }

                try
                {
                    DownloadProgressBar.Visibility = Visibility.Visible;
                    DownloadProgressBar.Value = 0;
                    StopDownloadButton.IsEnabled = true;

                    _cancellationTokenSource = new CancellationTokenSource();

                    // Determine file size
                    long fileSize = await GetFileSizeAsync(emulatorDownloadUrl);

                    if (fileSize < 2000 * 1024 * 1024) // Less than 2000 MB
                    {
                        await DownloadAndExtractInMemoryAsync(emulatorDownloadUrl, destinationPath, _cancellationTokenSource.Token);
                    }
                    else
                    {
                        await DownloadWithProgressAsync(emulatorDownloadUrl, downloadFilePath, _cancellationTokenSource.Token);

                        if (_isDownloadCompleted)
                        {
                            // Rename the file to .7z if EmulatorDownloadRename is true
                            if (selectedSystem.Emulators.Emulator.EmulatorDownloadRename)
                            {
                                string newFilePath = Path.ChangeExtension(downloadFilePath, ".7z");
                                if (File.Exists(downloadFilePath) && !File.Exists(newFilePath))
                                {
                                    File.Move(downloadFilePath, newFilePath);
                                }
                                downloadFilePath = newFilePath;
                            }

                            // Show the PleaseWaitExtraction window
                            PleaseWaitExtraction pleaseWaitWindow = new PleaseWaitExtraction();
                            pleaseWaitWindow.Show();

                            bool extractionSuccess = await ExtractCompressedFile.Instance2.ExtractFileWith7ZipAsync(downloadFilePath, destinationPath);
                            pleaseWaitWindow.Close();

                            if (extractionSuccess)
                            {
                                MessageBox.Show($"Emulator for {selectedSystem.SystemName} downloaded and extracted successfully.", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                                // Clean up the downloaded file only if extraction is successful
                                File.Delete(downloadFilePath);
                                
                                // Update the version file if necessary
                                if (destinationPath2 != null)
                                {
                                    string versionFilePath = Path.Combine(destinationPath2, "version_emulator.txt");
                                    await File.WriteAllTextAsync(versionFilePath, latestVersionString);
                                }
                                // Mark as downloaded and disable button
                                _isEmulatorDownloaded = true;
                                DownloadEmulatorButton.IsEnabled = false;
                                // Update AddSystemButton state
                                UpdateAddSystemButtonState();
                            }
                            else
                            {
                                // Extraction failed - offer redirect option
                                MessageBoxResult result = MessageBox.Show($"Extraction failed for {selectedSystem.SystemName} emulator.\n\n" +
                                                                          $"Would you like to be redirected to the download page?", "Extraction Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                                if (result == MessageBoxResult.Yes)
                                {
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadLink,
                                        UseShellExecute = true
                                    });
                                }
                            }
                        }
                        else
                        {
                            // Download was incomplete
                            MessageBoxResult result = MessageBox.Show($"Download was incomplete and will not be extracted.\n\n" +
                                                                      $"Would you like to be redirected to the download page?",
                                "Download Incomplete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadLink,
                                    UseShellExecute = true
                                });
                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Delete a partially downloaded file
                    if (File.Exists(downloadFilePath)) File.Delete(downloadFilePath);
                    
                    MessageBox.Show("Emulator download was canceled.", "Download Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    // Error downloading
                    string formattedException = $"Error downloading emulator.\n\n" +
                                                $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, formattedException);
            
                    MessageBoxResult result = MessageBox.Show($"Error downloading emulator.\n\n" +
                                                              $"Would you like to be redirected to the download page?",
                        "Download Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadLink,
                            UseShellExecute = true
                        });
                    }
                }
                finally
                {
                    StopDownloadButton.IsEnabled = false;
                }
            }
        }
        catch (Exception ex)
        {
            // Error downloading
            string formattedException = $"Error downloading emulator.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);
        }
    }

    private async void DownloadCoreButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _isDownloadCompleted = false;

            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string coreLocation = selectedSystem.Emulators.Emulator.CoreLocation;
                string coreDownloadUrl = selectedSystem.Emulators.Emulator.CoreDownloadLink;
                string emulatorsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators");
                Directory.CreateDirectory(emulatorsFolderPath);
                string downloadFilePath = Path.Combine(emulatorsFolderPath, Path.GetFileName(coreDownloadUrl) ?? throw new InvalidOperationException("Simple Launcher could not get coreDownloadUrl"));
                string destinationPath = selectedSystem.Emulators.Emulator.CoreDownloadExtractPath;
                string destinationPath2 = Path.GetDirectoryName(selectedSystem.Emulators.Emulator.CoreLocation);
                string latestVersionString = selectedSystem.Emulators.Emulator.CoreLatestVersion;

                // Check if the core is already installed and get the installed version
                if (File.Exists(coreLocation))
                {
                    string installedVersionString = GetInstalledCoreVersion(coreLocation);
                    
                    if (Version.TryParse(installedVersionString, out Version installedVersion) &&
                        Version.TryParse(latestVersionString, out Version latestVersion) &&
                        installedVersion.CompareTo(latestVersion) >= 0)
                    {
                        MessageBox.Show($"Core for {selectedSystem.SystemName} is already installed and up to date.",
                            "Core Already Installed", MessageBoxButton.OK, MessageBoxImage.Information);
                        
                        // Mark as downloaded and disable button
                        _isCoreDownloaded = true;
                        DownloadCoreButton.IsEnabled = false;
                        // Update AddSystemButton state
                        UpdateAddSystemButtonState();
                        
                        return;
                    }
                }

                try
                {
                    // Display progress bar
                    DownloadProgressBar.Visibility = Visibility.Visible;
                    DownloadProgressBar.Value = 0;
                    StopDownloadButton.IsEnabled = true;

                    // Initialize cancellation token source
                    _cancellationTokenSource = new CancellationTokenSource();

                    // Determine file size
                    long fileSize = await GetFileSizeAsync(coreDownloadUrl);

                    if (fileSize < 2000 * 1024 * 1024) // Less than 2000 MB
                    {
                        await DownloadAndExtractInMemoryAsync(coreDownloadUrl, destinationPath, _cancellationTokenSource.Token);
                    }
                    else
                    {
                        await DownloadWithProgressAsync(coreDownloadUrl, downloadFilePath, _cancellationTokenSource.Token);

                        // Only proceed with extraction if the download completed successfully
                        if (_isDownloadCompleted)
                        {
                            // Show the PleaseWaitExtraction window
                            PleaseWaitExtraction pleaseWaitWindow = new PleaseWaitExtraction();
                            pleaseWaitWindow.Show();

                            bool extractionSuccess = await ExtractCompressedFile.Instance2.ExtractFileWith7ZipAsync(downloadFilePath, destinationPath);
                            pleaseWaitWindow.Close();

                            if (extractionSuccess)
                            {
                                MessageBox.Show($"Core for {selectedSystem.SystemName} downloaded and extracted successfully.",
                                    "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                                
                                // Clean up the downloaded file only if extraction is successful
                                File.Delete(downloadFilePath);
                                
                                // Update the version file if necessary
                                if (destinationPath2 != null)
                                {
                                    string versionFilePath = Path.Combine(destinationPath2, "version_core.txt");
                                    await File.WriteAllTextAsync(versionFilePath, latestVersionString);
                                }
                                // Mark as downloaded and disable button
                                _isCoreDownloaded = true;
                                DownloadCoreButton.IsEnabled = false;
                                // Update AddSystemButton state
                                UpdateAddSystemButtonState();
                            }
                            else
                            {
                                // Extraction failed - offer redirect option
                                MessageBoxResult result = MessageBox.Show($"Extraction failed for {selectedSystem.SystemName} core.\n\n" +
                                                                          $"Would you like to be redirected to the download page?",
                                    "Extraction Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                                if (result == MessageBoxResult.Yes)
                                {
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = selectedSystem.Emulators.Emulator.CoreDownloadLink,
                                        UseShellExecute = true
                                    });
                                }
                            }
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show($"Download was incomplete and will not be extracted.\n\n" +
                                                                      $"Would you like to be redirected to the download page?",
                                "Download Incomplete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = selectedSystem.Emulators.Emulator.CoreDownloadLink,
                                    UseShellExecute = true
                                });
                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Delete a partially downloaded file
                    if (File.Exists(downloadFilePath)) File.Delete(downloadFilePath);
                    
                    MessageBox.Show("Core download was canceled.", "Download Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    string formattedException = $"Error downloading the core.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, formattedException);

                    MessageBoxResult result = MessageBox.Show($"Error downloading the core for this system.\n\n" +
                                                              $"Would you like to be redirected to the download page?",
                        "Download Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = selectedSystem.Emulators.Emulator.CoreDownloadLink,
                            UseShellExecute = true
                        });
                    }
                }
                finally
                {
                    StopDownloadButton.IsEnabled = false;
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"Error downloading the core.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);        }
    }

    private async void DownloadExtrasButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reset the flag at the start of the download
            _isDownloadCompleted = false;

            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string extrasDownloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;
                string extrasFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
                Directory.CreateDirectory(extrasFolderPath);
                string downloadFilePath = Path.Combine(extrasFolderPath, Path.GetFileName(extrasDownloadUrl) ?? throw new InvalidOperationException("Simple Launcher could not get extrasDownloadUrl"));
                string destinationPath = selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;

                try
                {
                    // Display progress bar
                    DownloadProgressBar.Visibility = Visibility.Visible;
                    DownloadProgressBar.Value = 0;
                    StopDownloadButton.IsEnabled = true;

                    // Initialize cancellation token source
                    _cancellationTokenSource = new CancellationTokenSource();

                    // Determine file size
                    long fileSize = await GetFileSizeAsync(extrasDownloadUrl);

                    if (fileSize < 2000 * 1024 * 1024) // Less than 2000 MB
                    {
                        await DownloadAndExtractInMemoryAsync(extrasDownloadUrl, destinationPath, _cancellationTokenSource.Token);
                    }
                    else
                    {
                        await DownloadWithProgressAsync(extrasDownloadUrl, downloadFilePath, _cancellationTokenSource.Token);

                        // Only proceed with extraction if the download completed successfully
                        if (_isDownloadCompleted)
                        {
                            // Show the PleaseWaitExtraction window
                            PleaseWaitExtraction pleaseWaitWindow = new PleaseWaitExtraction();
                            pleaseWaitWindow.Show();

                            bool extractionSuccess = await ExtractCompressedFile.Instance2.ExtractFileWith7ZipAsync(downloadFilePath, destinationPath);
                            pleaseWaitWindow.Close();

                            if (extractionSuccess)
                            {
                                MessageBox.Show($"Image pack for {selectedSystem.SystemName} downloaded and extracted successfully.",
                                    "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                                
                                // Clean up the downloaded file only if extraction is successful
                                File.Delete(downloadFilePath);
                                // Mark as downloaded and disable button
                                DownloadExtrasButton.IsEnabled = false;
                                // Update AddSystemButton state
                                UpdateAddSystemButtonState();
                            }
                            else
                            {
                                // Extraction failed - offer redirect option
                                MessageBoxResult result = MessageBox.Show($"Extraction failed for the image pack.\n\n" +
                                                                          $"Would you like to be redirected to the download page?",
                                    "Extraction Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                                if (result == MessageBoxResult.Yes)
                                {
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = selectedSystem.Emulators.Emulator.ExtrasDownloadLink,
                                        UseShellExecute = true
                                    });
                                }
                            }
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show($"Download was incomplete and will not be extracted.\n\n" +
                                                                      $"Would you like to be redirected to the download page?",
                                "Download Incomplete", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = selectedSystem.Emulators.Emulator.ExtrasDownloadLink,
                                    UseShellExecute = true
                                });
                            }
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Delete a partially downloaded file
                    if (File.Exists(downloadFilePath)) File.Delete(downloadFilePath);
                    
                    MessageBox.Show("ImagePack download was canceled.", "Download Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    string formattedException = $"Error downloading the ImagePack.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, formattedException);

                    MessageBoxResult result = MessageBox.Show($"Error downloading the Image Pack.\n\n" +
                                                              $"Would you like to be redirected to the download page?",
                        "Download Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = selectedSystem.Emulators.Emulator.ExtrasDownloadLink,
                            UseShellExecute = true
                        });
                    }
                }
                finally
                {
                    StopDownloadButton.IsEnabled = false;
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"Error downloading the ImagePack.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);        }
    }
        
    private async Task DownloadWithProgressAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            long? totalBytes = response.Content.Headers.ContentLength;
            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;

                if (totalBytes.HasValue)
                {
                    DownloadProgressBar.Value = (double)totalBytesRead / totalBytes.Value * 100;
                }
            }

            // Check if the file was fully downloaded
            if (totalBytes.HasValue && totalBytesRead == totalBytes.Value)
            {
                _isDownloadCompleted = true;
            }
            else
            {
                _isDownloadCompleted = false;
                throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
            }

        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            string formattedException = $"The requested file was not available on the server.\n\nURL: {downloadUrl}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            MessageBox.Show("The requested file is not available on the server.\n\nThe error was reported to the developer that will try to fix the issue.", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (HttpRequestException ex)
        {
            string formattedException = $"Network error during file download.\n\nURL: {downloadUrl}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            MessageBox.Show("There was a network error either with your internet access or the server.\n\n" +
                            "Please try again later.", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (IOException ex)
        {
            string formattedException = $"File read/write error after file download.\n\nURL: {downloadUrl}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            MessageBox.Show("There was a file read/write error after the file download.\n\n" +
                            "Some antivirus programs may lock or scan newly downloaded files, causing access issues. Try temporarily disabling real-time protection.", "Read/Write Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (TaskCanceledException ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // If user canceled, delete the partially downloaded file
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
                    
                string formattedException = $"Download was canceled by the user.\n\nURL: {downloadUrl}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
            }
            else
            {
                // If user canceled, delete the partially downloaded file
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
                    
                string formattedException = $"Download timed out or was canceled unexpectedly.\n\nURL: {downloadUrl}\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, formattedException);
                    
                MessageBox.Show("Download timed out or was canceled unexpectedly.\n\nYou can try again later.", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
        
    private async Task DownloadAndExtractInMemoryAsync(string downloadUrl, string destinationPath, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
    
        await using var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Seek(0, SeekOrigin.Begin);

        using (var archive = SharpCompress.Archives.ArchiveFactory.Open(memoryStream))
        {
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory)
                {
                    string entryDestinationPath = Path.Combine(destinationPath, entry.Key ?? throw new InvalidOperationException("Could not get entry.key"));
                    Directory.CreateDirectory(Path.GetDirectoryName(entryDestinationPath) ?? throw new InvalidOperationException("Could not get Destination Path"));
                    await using var entryStream = entry.OpenEntryStream();
                    await using var fileStream = new FileStream(entryDestinationPath, FileMode.Create, FileAccess.Write);
                    await entryStream.CopyToAsync(fileStream, cancellationToken);
                }
            }
        }

        _isDownloadCompleted = true;
        MessageBox.Show("Download and extraction is complete.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }
        
    private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel(); // Cancel the ongoing download
            StopDownloadButton.IsEnabled = false; // Disable the stop button once the download is canceled

            // Reset completion flag and progress
            _isDownloadCompleted = false; 
            DownloadProgressBar.Value = 0;
                
            // Reinitialize the cancellation token source for the next download
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    private void AddSystemButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
        if (selectedSystem != null)
        {
            // Determine the system folder to use
            string systemFolder =
                // Use the default ROM folder
                string.IsNullOrEmpty(SystemFolderTextBox.Text) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roms", selectedSystem.SystemName) :
                    // Use the value from SystemFolderTextBox
                    SystemFolderTextBox.Text;

            // Remove the leading dot from the SystemImageFolder for the message
            string systemImageFolderForMessage = selectedSystem.SystemImageFolder.TrimStart('.').TrimStart('\\', '/');

            // Combine with the base directory for the message
            string fullImageFolderPathForMessage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, systemImageFolderForMessage);

            // Path to the system.xml file
            string systemXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.xml");

            try
            {
                // Load existing system configurations
                XDocument xmlDoc = XDocument.Load(systemXmlPath);
                var systemConfigs = xmlDoc.Descendants("SystemConfig").ToList();

                // Check if the system already exists
                var existingSystem = systemConfigs.FirstOrDefault(config => config.Element("SystemName")?.Value == selectedSystem.SystemName);
                if (existingSystem != null)
                {
                    // Ask user if they want to overwrite the existing system
                    MessageBoxResult result = MessageBox.Show($"The system {selectedSystem.SystemName} already exists.\n\nDo you want to overwrite it?", "System Already Exists", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }

                    // Overwrite existing system
                    existingSystem.SetElementValue("SystemName", selectedSystem.SystemName);
                    existingSystem.SetElementValue("SystemFolder", systemFolder);
                    existingSystem.SetElementValue("SystemImageFolder", selectedSystem.SystemImageFolder); // Keep the dot here
                    existingSystem.SetElementValue("SystemIsMAME", selectedSystem.SystemIsMame);
                    existingSystem.Element("FileFormatsToSearch")?.Remove();
                    existingSystem.Add(new XElement("FileFormatsToSearch", selectedSystem.FileFormatsToSearch.Select(format => new XElement("FormatToSearch", format))));
                    existingSystem.SetElementValue("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch);
                    existingSystem.Element("FileFormatsToLaunch")?.Remove();
                    existingSystem.Add(new XElement("FileFormatsToLaunch", selectedSystem.FileFormatsToLaunch.Select(format => new XElement("FormatToLaunch", format))));
                    existingSystem.Element("Emulators")?.Remove();
                    existingSystem.Add(new XElement("Emulators",
                        new XElement("Emulator",
                            new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                            new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                            new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
                        )
                    ));
                }
                else
                {
                    // Create a new XElement for the selected system
                    var newSystemElement = new XElement("SystemConfig",
                        new XElement("SystemName", selectedSystem.SystemName),
                        new XElement("SystemFolder", systemFolder),
                        new XElement("SystemImageFolder", selectedSystem.SystemImageFolder), // Keep the dot here
                        new XElement("SystemIsMAME", selectedSystem.SystemIsMame),
                        new XElement("FileFormatsToSearch", selectedSystem.FileFormatsToSearch.Select(format => new XElement("FormatToSearch", format))),
                        new XElement("ExtractFileBeforeLaunch", selectedSystem.ExtractFileBeforeLaunch),
                        new XElement("FileFormatsToLaunch", selectedSystem.FileFormatsToLaunch.Select(format => new XElement("FormatToLaunch", format))),
                        new XElement("Emulators",
                            new XElement("Emulator",
                                new XElement("EmulatorName", selectedSystem.Emulators.Emulator.EmulatorName),
                                new XElement("EmulatorLocation", selectedSystem.Emulators.Emulator.EmulatorLocation),
                                new XElement("EmulatorParameters", selectedSystem.Emulators.Emulator.EmulatorParameters)
                            )
                        )
                    );

                    // Add the new system to the XML document
                    xmlDoc.Root?.Add(newSystemElement);
                }

                // Sort the systems alphabetically by SystemName
                if (xmlDoc.Root != null)
                    xmlDoc.Root.ReplaceNodes(xmlDoc.Root.Elements("SystemConfig")
                        .OrderBy(systemElement => systemElement.Element("SystemName")?.Value));

                // Save the updated XML document
                xmlDoc.Save(systemXmlPath);

                // Create the necessary folders for the system
                CreateSystemFolders(selectedSystem.SystemName, systemFolder, fullImageFolderPathForMessage);

                MessageBox.Show($"The system {selectedSystem.SystemName} has been added successfully.\n\nPut ROMs or ISOs for this system inside '{systemFolder}'\n\n" +
                                $"Put cover images for this system inside '{fullImageFolderPathForMessage}'.", "System Added", MessageBoxButton.OK, MessageBoxImage.Information);
                AddSystemButton.IsEnabled = false;

            }
            catch (Exception ex)
            {
                string formattedException = $"Error adding system.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                    
                MessageBox.Show($"There was an error adding this system.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
        
    private void UpdateAddSystemButtonState()
    {
        AddSystemButton.IsEnabled = _isEmulatorDownloaded && _isCoreDownloaded;
    }
        
    private void CreateSystemFolders(string systemName, string systemFolder, string systemImageFolder)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Paths for the primary system folder and image folder
        string systemFolderPath = Path.Combine(baseDirectory, systemFolder);
        string imagesFolderPath = Path.Combine(baseDirectory, systemImageFolder);

        // List of additional folders to create
        string[] additionalFolders = ["roms", "images", "title_snapshots", "gameplay_snapshots", "videos", "manuals", "walkthrough", "cabinets", "carts", "flyers", "pcbs"];

        try
        {
            // Create the primary system folder if it doesn't exist
            if (!Directory.Exists(systemFolderPath))
            {
                Directory.CreateDirectory(systemFolderPath);
            }

            // Create the primary image folder if it doesn't exist
            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            // Create each additional folder
            foreach (var folder in additionalFolders)
            {
                string folderPath = Path.Combine(baseDirectory, folder, systemName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
        }
        catch (Exception ex)
        {
            string formattedException = $"The application failed to create the necessary folders for the newly added system.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            MessageBox.Show($"The application failed to create the necessary folders for this system.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }
        
    private void EditSystemEasyModeAddSystem_Closed(object sender, EventArgs e)
    {
        _config = null;
            
        // Prepare the process start info
        var processModule = Process.GetCurrentProcess().MainModule;
        if (processModule != null)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = processModule.FileName,
                UseShellExecute = true
            };

            // Start the new application instance
            Process.Start(startInfo);

            // Shutdown the current application instance
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
        
    private void ChooseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog();
        dialog.Description = @"Choose a Folder with ROMs or ISOs for this System";
        dialog.ShowNewFolderButton = true;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SystemFolderTextBox.Text = dialog.SelectedPath;
        }
    }
        
    private async Task<long> GetFileSizeAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        using var response = await _httpClient.SendAsync(request);
        return response.Content.Headers.ContentLength ?? 0;
    }
        
    private string GetInstalledEmulatorVersion(string emulatorLocation)
    {
        string versionFilePath = Path.Combine(Path.GetDirectoryName(emulatorLocation) ?? string.Empty, "version_emulator.txt");
        if (File.Exists(versionFilePath))
        {
            return File.ReadAllText(versionFilePath).Trim();
        }
        return null;
    }
        
    private string GetInstalledCoreVersion(string coreLocation)
    {
        string versionFilePath = Path.Combine(Path.GetDirectoryName(coreLocation) ?? string.Empty, "version_core.txt");
        if (File.Exists(versionFilePath))
        {
            return File.ReadAllText(versionFilePath).Trim();
        }
        return null;
    }
        
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }

}