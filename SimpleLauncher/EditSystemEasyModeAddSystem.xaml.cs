using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

namespace SimpleLauncher
{
    public partial class EditSystemEasyModeAddSystem
    {
        private EasyModeConfig _config;
        private bool _isEmulatorDownloaded;
        private bool _isCoreDownloaded;
        private bool _isExtrasDownloaded;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly HttpClient _httpClient = new HttpClient();
        
        public EditSystemEasyModeAddSystem()
        {
            InitializeComponent();
            
            // Apply the theme to this window
            App.ApplyThemeToWindow(this);
            
            LoadConfig();
            PopulateSystemDropdown();
            
            // Subscribe to the Closed event
            Closed += EditSystemEasyModeAddSystem_Closed; 
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
                    _isExtrasDownloaded = !DownloadExtrasButton.IsEnabled; // Assume downloaded if no download needed

                    UpdateAddSystemButtonState();
                }
            }
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
        
        private async void DownloadEmulatorButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string emulatorLocation = selectedSystem.Emulators.Emulator.EmulatorLocation;
                string emulatorDownloadUrl = selectedSystem.Emulators.Emulator.EmulatorDownloadLink;
                string emulatorsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators");
                Directory.CreateDirectory(emulatorsFolderPath); // Ensure the emulators folder exists
                string downloadFilePath = Path.Combine(emulatorsFolderPath, Path.GetFileName(emulatorDownloadUrl));
                string destinationPath = selectedSystem.Emulators.Emulator.EmulatorDownloadExtractPath;
                string destinationPath2 = Path.GetDirectoryName(selectedSystem.Emulators.Emulator.EmulatorLocation);
                string latestVersionString = selectedSystem.Emulators.Emulator.EmulatorLatestVersion;

                // Check if the emulator is already installed and get the installed version
                if (File.Exists(emulatorLocation))
                {
                    string installedVersionString = GetInstalledEmulatorVersion(emulatorLocation);

                    if (Version.TryParse(installedVersionString, out Version installedVersion) &&
                        Version.TryParse(latestVersionString, out Version latestVersion) &&
                        installedVersion.CompareTo(latestVersion) >= 0)
                    {
                        MessageBox.Show($"Emulator for {selectedSystem.SystemName} is already installed and up to date.", "Emulator Already Installed", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Mark as downloaded and disable button
                        _isEmulatorDownloaded = true;
                        DownloadEmulatorButton.IsEnabled = false;

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

                    // Download the file
                    await DownloadWithProgressAsync(emulatorDownloadUrl, downloadFilePath, _cancellationTokenSource.Token);

                    // Rename the file to .7z if EmulatorDownloadRename is true
                    if (selectedSystem.Emulators.Emulator.EmulatorDownloadRename)
                    {
                        string newFilePath = Path.ChangeExtension(downloadFilePath, ".7z");
                        File.Move(downloadFilePath, newFilePath);
                        downloadFilePath = newFilePath;
                    }

                    // Show the PleaseWaitExtraction window
                    PleaseWaitExtraction pleaseWaitWindow = new PleaseWaitExtraction();
                    pleaseWaitWindow.Show();

                    // Extract the downloaded file
                    bool extractionSuccess = await ExtractFileWith7ZipAsync(downloadFilePath, destinationPath);

                    // Close the PleaseWaitExtraction window after extraction
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
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show("Download was canceled.", "Download Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBoxResult result = MessageBox.Show($"Error downloading emulator: {ex.Message}\n\nWould you like to be redirected to the download page?", "Download Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadPage,
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
        
        private string GetInstalledCoreVersion(string coreLocation)
        {
            string versionFilePath = Path.Combine(Path.GetDirectoryName(coreLocation) ?? string.Empty, "version_core.txt");
            if (File.Exists(versionFilePath))
            {
                return File.ReadAllText(versionFilePath).Trim();
            }
            return null;
        }

        private async void DownloadCoreButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string coreLocation = selectedSystem.Emulators.Emulator.CoreLocation;
                string coreDownloadUrl = selectedSystem.Emulators.Emulator.CoreDownloadLink;
                string emulatorsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators");
                Directory.CreateDirectory(emulatorsFolderPath); // Ensure the emulators folder exists
                string downloadFilePath = Path.Combine(emulatorsFolderPath, Path.GetFileName(coreDownloadUrl));
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
                        MessageBox.Show($"Core for {selectedSystem.SystemName} is already installed and up to date.", "Core Already Installed", MessageBoxButton.OK, MessageBoxImage.Information);

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

                    // Download the file
                    await DownloadWithProgressAsync(coreDownloadUrl, downloadFilePath, _cancellationTokenSource.Token);

                    // Show the PleaseWaitExtraction window
                    PleaseWaitExtraction pleaseWaitWindow = new PleaseWaitExtraction();
                    pleaseWaitWindow.Show();

                    // Extract the downloaded file
                    bool extractionSuccess = await ExtractFileWith7ZipAsync(downloadFilePath, destinationPath);

                    // Close the PleaseWaitExtraction window after extraction
                    pleaseWaitWindow.Close();

                    if (extractionSuccess)
                    {
                        MessageBox.Show($"Core for {selectedSystem.SystemName} downloaded and extracted successfully.", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);

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
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show("Download was canceled.", "Download Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBoxResult result = MessageBox.Show($"Error downloading core: {ex.Message}\n\nWould you like to be redirected to the download page?", "Download Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadPage,
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

        private async void DownloadExtrasButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string extrasDownloadUrl = selectedSystem.Emulators.Emulator.ExtrasDownloadLink;
                string emulatorsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators");
                Directory.CreateDirectory(emulatorsFolderPath); // Ensure the emulators folder exists
                string downloadFilePath = Path.Combine(emulatorsFolderPath, Path.GetFileName(extrasDownloadUrl));
                string destinationPath = selectedSystem.Emulators.Emulator.ExtrasDownloadExtractPath;

                try
                {
                    // Display progress bar
                    DownloadProgressBar.Visibility = Visibility.Visible;
                    DownloadProgressBar.Value = 0;
                    StopDownloadButton.IsEnabled = true;
                    
                    // Initialize cancellation token source
                    _cancellationTokenSource = new CancellationTokenSource();

                    // Download the file
                    await DownloadWithProgressAsync(extrasDownloadUrl, downloadFilePath, _cancellationTokenSource.Token);

                    // Show the PleaseWaitExtraction window
                    PleaseWaitExtraction pleaseWaitWindow = new PleaseWaitExtraction();
                    pleaseWaitWindow.Show();

                    // Extract the downloaded file
                    bool extractionSuccess = await ExtractFileWith7ZipAsync(downloadFilePath, destinationPath);

                    // Close the PleaseWaitExtraction window after extraction
                    pleaseWaitWindow.Close();

                    if (extractionSuccess)
                    {
                        MessageBox.Show($"Extras for {selectedSystem.SystemName} downloaded and extracted successfully.", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Clean up the downloaded file only if extraction is successful
                        File.Delete(downloadFilePath);

                        // Mark as downloaded and disable button
                        _isExtrasDownloaded = true;
                        DownloadExtrasButton.IsEnabled = false;

                        // Update AddSystemButton state
                        UpdateAddSystemButtonState();
                    }
                }
                catch (TaskCanceledException)
                {
                    MessageBox.Show("Download was canceled.", "Download Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBoxResult result = MessageBox.Show($"Error downloading extras: {ex.Message}\n\nWould you like to be redirected to the download page?", "Download Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadPage,
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
                if (totalBytes != null && totalBytesRead != totalBytes.Value)
                {
                    throw new IOException("Download incomplete. Bytes downloaded do not match the expected file size.");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Network error: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new Exception($"File read/write error: {ex.Message}", ex);
            }
            catch (TaskCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new TaskCanceledException("Download was canceled by the user.");
                }
                else
                {
                    throw new TaskCanceledException("Download timed out or was canceled unexpectedly.");
                }
            }
        }

        private async Task<bool> ExtractFileWith7ZipAsync(string filePath, string destinationFolder)
        {
            try
            {
                string sevenZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.exe");
                if (!File.Exists(sevenZipPath))
                {
                    MessageBox.Show("7z.exe not found. Please ensure 7-Zip is installed and 7z.exe is available.",
                        "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = $"x \"{filePath}\" -o\"{destinationFolder}\" -y",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new Process();
                process.StartInfo = processStartInfo;
                process.Start();

                await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    MessageBox.Show($"Error extracting file: {error}", "Extraction Error", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting file: {ex.Message}", "Extraction Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }
        }
        
        private void StopDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel(); // Cancel the ongoing download
                StopDownloadButton.IsEnabled = false; // Disable the stop button once the download is canceled
            }
        }

        private void UpdateAddSystemButtonState()
        {
            AddSystemButton.IsEnabled = _isEmulatorDownloaded && _isCoreDownloaded && _isExtrasDownloaded;
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
                        MessageBoxResult result = MessageBox.Show($"The system {selectedSystem.SystemName} already exists. Do you want to overwrite it?", "System Already Exists", MessageBoxButton.YesNo, MessageBoxImage.Question);
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

                    MessageBox.Show($"The system {selectedSystem.SystemName} has been added successfully.\n\nPut your ROMs for this system inside '{systemFolder}'\n\n" +
                                    $"Put your cover images for this system inside '{fullImageFolderPathForMessage}'.", "System Added", MessageBoxButton.OK, MessageBoxImage.Information);
                    AddSystemButton.IsEnabled = false;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding system: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void CreateSystemFolders(string systemName, string systemFolder, string systemImageFolder)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Paths for the primary system folder and image folder
            string systemFolderPath = Path.Combine(baseDirectory, systemFolder);
            string imagesFolderPath = Path.Combine(baseDirectory, systemImageFolder);

            // List of additional folders to create
            string[] additionalFolders = ["title_snapshots", "gameplay_snapshots", "videos", "manuals", "walkthrough", "cabinets", "carts", "flyers", "pcbs"
            ];

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
            catch (Exception exception)
            {
                string formattedException = $"The Simple Launcher failed to create the necessary folders for the newly added system.\n\nException detail: {exception}";
                Task logTask = LogErrors.LogErrorAsync(exception, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
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
        
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
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
    }
}