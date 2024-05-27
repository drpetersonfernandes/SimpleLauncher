using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Net.Http;

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
                DownloadEmulatorButton.IsEnabled = true;
                DownloadCoreButton.IsEnabled = true;
                DownloadExtrasButton.IsEnabled = true;
                AddSystemButton.IsEnabled = true;
            }
        }

        private async void DownloadEmulatorButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string emulatorDownloadUrl = selectedSystem.Emulator.EmulatorBinaryDownload;
                string emulatorsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emulators");
                Directory.CreateDirectory(emulatorsFolderPath); // Ensure the emulators folder exists
                string downloadFilePath = Path.Combine(emulatorsFolderPath, Path.GetFileName(emulatorDownloadUrl));
                string destinationPath = selectedSystem.Emulator.EmulatorBinaryExtractPathTo;

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
                        // Clean up the downloaded file only if extraction is successful
                        File.Delete(downloadFilePath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading emulator: {ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // // Hide progress bar
                    // DownloadProgressBar.Visibility = Visibility.Collapsed;
                }
            }
        }



        private void DownloadCoreButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string coreDownloadUrl = selectedSystem.Emulator.EmulatorCoreDownload;
                string destinationPath = selectedSystem.Emulator.EmulatorCoreExtractPath;

                // Implement download logic here, e.g., using HttpClient and await the task
                // Example:
                // await DownloadFileAsync(coreDownloadUrl, destinationPath);

                MessageBox.Show($"Download Core logic for {selectedSystem.SystemName} goes here.", "Download Core", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DownloadExtrasButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSystem = _config.Systems.FirstOrDefault(system => system.SystemName == SystemNameDropdown.SelectedItem.ToString());
            if (selectedSystem != null)
            {
                string extrasDownloadUrl = selectedSystem.Emulator.EmulatorExtrasDownload;
                string destinationPath = selectedSystem.Emulator.EmulatorExtrasExtractPath;

                // Implement download logic here, e.g., using HttpClient and await the task
                // Example:
                // await DownloadFileAsync(extrasDownloadUrl, destinationPath);

                MessageBox.Show($"Download Extras logic for {selectedSystem.SystemName} goes here.", "Download Extras", MessageBoxButton.OK, MessageBoxImage.Information);
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

                var processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = sevenZipPath,
                    Arguments = $"x \"{filePath}\" -o\"{destinationFolder}\" -y",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = new System.Diagnostics.Process();
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
                // Implement logic to add system to Simple Launcher
                MessageBox.Show($"Add System logic for {selectedSystem.SystemName} goes here.", "Add System", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}