using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;
using System.Xml.Linq;

namespace SimpleLauncher
{
    public partial class EditSystemEasyModeAddSystem
    {
        private EasyModeConfig _config;

        public EditSystemEasyModeAddSystem()
        {
            InitializeComponent();
            LoadConfig();
            PopulateSystemDropdown();
            this.Closed += EditSystemEasyModeAddSystem_Closed; // Subscribe to the Closed event
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
                SystemNameDropdown.ItemsSource = _config.Systems.Select(system => system.SystemName).ToList();
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
                    DownloadCoreButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.EmulatorCoreDownload);
                    DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedSystem.Emulators.Emulator.EmulatorExtrasDownload);
                }
            }
        }

        private async void DownloadEmulatorButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string emulatorDownloadUrl = selectedSystem.Emulators.Emulator.EmulatorBinaryDownload;
                string emulatorsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators");
                Directory.CreateDirectory(emulatorsFolderPath); // Ensure the emulators folder exists
                string downloadFilePath = Path.Combine(emulatorsFolderPath, Path.GetFileName(emulatorDownloadUrl));
                string destinationPath = selectedSystem.Emulators.Emulator.EmulatorBinaryExtractPath;

                try
                {
                    // Display progress bar
                    DownloadProgressBar.Visibility = Visibility.Visible;
                    DownloadProgressBar.Value = 0;

                    // Download the file
                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage response = await client.GetAsync(emulatorDownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    await using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    await using (Stream streamToWriteTo = File.Open(downloadFilePath, FileMode.Create))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;
                        while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                            if (totalBytes != -1)
                            {
                                DownloadProgressBar.Value = (double)totalBytesRead / totalBytes * 100;
                            }
                        }
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
                        // Enable the AddSystemButton
                        AddSystemButton.IsEnabled = true;
                        // Clean up the downloaded file only if extraction is successful
                        File.Delete(downloadFilePath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading emulator: {ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // finally
                // {
                //     // Hide progress bar
                //     DownloadProgressBar.Visibility = Visibility.Collapsed;
                // }
            }
        }

        private async void DownloadCoreButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string coreDownloadUrl = selectedSystem.Emulators.Emulator.EmulatorCoreDownload;
                string emulatorsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators");
                Directory.CreateDirectory(emulatorsFolderPath); // Ensure the emulators folder exists
                string downloadFilePath = Path.Combine(emulatorsFolderPath, Path.GetFileName(coreDownloadUrl));
                string destinationPath = selectedSystem.Emulators.Emulator.EmulatorCoreExtractPath;

                try
                {
                    // Display progress bar
                    DownloadProgressBar.Visibility = Visibility.Visible;
                    DownloadProgressBar.Value = 0;

                    // Download the file
                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage response = await client.GetAsync(coreDownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    await using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    await using (Stream streamToWriteTo = File.Open(downloadFilePath, FileMode.Create))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;
                        while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                            if (totalBytes != -1)
                            {
                                DownloadProgressBar.Value = (double)totalBytesRead / totalBytes * 100;
                            }
                        }
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
                        MessageBox.Show($"Core for {selectedSystem.SystemName} downloaded and extracted successfully.", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        // Enable the AddSystemButton
                        AddSystemButton.IsEnabled = true;
                        // Clean up the downloaded file only if extraction is successful
                        File.Delete(downloadFilePath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading core: {ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // finally
                // {
                //     // Hide progress bar
                //     DownloadProgressBar.Visibility = Visibility.Collapsed;
                // }
            }
        }

        private async void DownloadExtrasButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string extrasDownloadUrl = selectedSystem.Emulators.Emulator.EmulatorExtrasDownload;
                string emulatorsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators");
                Directory.CreateDirectory(emulatorsFolderPath); // Ensure the emulators folder exists
                string downloadFilePath = Path.Combine(emulatorsFolderPath, Path.GetFileName(extrasDownloadUrl));
                string destinationPath = selectedSystem.Emulators.Emulator.EmulatorExtrasExtractPath;

                try
                {
                    // Display progress bar
                    DownloadProgressBar.Visibility = Visibility.Visible;
                    DownloadProgressBar.Value = 0;

                    // Download the file
                    using (HttpClient client = new HttpClient())
                    using (HttpResponseMessage response = await client.GetAsync(extrasDownloadUrl, HttpCompletionOption.ResponseHeadersRead))
                    await using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                    await using (Stream streamToWriteTo = File.Open(downloadFilePath, FileMode.Create))
                    {
                        byte[] buffer = new byte[8192];
                        int bytesRead;
                        long totalBytesRead = 0;
                        long totalBytes = response.Content.Headers.ContentLength ?? -1;
                        while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await streamToWriteTo.WriteAsync(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                            if (totalBytes != -1)
                            {
                                DownloadProgressBar.Value = (double)totalBytesRead / totalBytes * 100;
                            }
                        }
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
                        MessageBox.Show($"Extras for {selectedSystem.SystemName} downloaded and extracted successfully.", "Download Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                        // Enable the AddSystemButton
                        AddSystemButton.IsEnabled = true;
                        // Clean up the downloaded file only if extraction is successful
                        File.Delete(downloadFilePath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading extras: {ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // finally
                // {
                //     // Hide progress bar
                //     DownloadProgressBar.Visibility = Visibility.Collapsed;
                // }
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

        private void AddSystemButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
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
                        existingSystem.SetElementValue("SystemFolder", selectedSystem.SystemFolder);
                        existingSystem.SetElementValue("SystemImageFolder", selectedSystem.SystemImageFolder);
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
                            new XElement("SystemFolder", selectedSystem.SystemFolder),
                            new XElement("SystemImageFolder", selectedSystem.SystemImageFolder),
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

                    // Save the updated XML document
                    xmlDoc.Save(systemXmlPath);

                    // Create a folder inside the application folder named after the value in _config.SystemFolder
                    string systemFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selectedSystem.SystemFolder);
                    if (!Directory.Exists(systemFolderPath))
                    {
                        Directory.CreateDirectory(systemFolderPath);
                    }

                    // Create a folder inside the images folder based on the value in _config.SystemImageFolder
                    string imagesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selectedSystem.SystemImageFolder);
                    if (!Directory.Exists(imagesFolderPath))
                    {
                        Directory.CreateDirectory(imagesFolderPath);
                    }

                    MessageBox.Show($"The system {selectedSystem.SystemName} has been added successfully, and the folders '{selectedSystem.SystemFolder}' and '{selectedSystem.SystemImageFolder}' have been created.", "System Added", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding system: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

    }
}