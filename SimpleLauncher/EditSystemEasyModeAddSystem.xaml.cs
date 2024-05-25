using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher
{
    public partial class EditSystemEasyModeAddSystem
    {
        private EmulatorList _emulatorList;

        public EditSystemEasyModeAddSystem()
        {
            InitializeComponent();
            LoadEmulatorList();
            PopulateSystemDropdown();
        }

        private void LoadEmulatorList()
        {
            string filePath = "emulatorlist.xml";
            if (File.Exists(filePath))
            {
                _emulatorList = EmulatorList.LoadFromFile(filePath);
            }
            else
            {
                MessageBox.Show("Emulator list file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void PopulateSystemDropdown()
        {
            var systems = _emulatorList.Emulators
                .SelectMany(emulator => emulator.RelatedSystems)
                .Distinct()
                .OrderBy(system => system) // Order systems alphabetically
                .ToList();

            foreach (var system in systems)
            {
                SystemNameDropdown.Items.Add(system);
            }
        }

        private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SystemNameDropdown.SelectedItem != null)
            {
                LoadListOfEmulators(SystemNameDropdown.SelectedItem.ToString());
            }
        }

        private void LoadListOfEmulators(string system)
        {
            EmulatorDropdown.Items.Clear();

            var emulators = _emulatorList.Emulators
                .Where(emulator => emulator.RelatedSystems.Contains(system))
                .OrderBy(emulator => emulator.EmulatorName)
                .ToList();

            foreach (var emulator in emulators)
            {
                EmulatorDropdown.Items.Add(emulator.EmulatorName);
            }

            if (EmulatorDropdown.Items.Count > 0)
            {
                DownloadEmulatorButton.IsEnabled = false;
                DownloadCoreButton.IsEnabled = false;
                DownloadExtrasButton.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("No emulator found for the selected system.", "No Emulators", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void EmulatorDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmulatorDropdown.SelectedItem != null)
            {
                var selectedEmulator =
                    _emulatorList.Emulators.Find(em => em.EmulatorName == EmulatorDropdown.SelectedItem.ToString());

                if (selectedEmulator != null)
                {
                    DownloadEmulatorButton.IsEnabled = !string.IsNullOrEmpty(selectedEmulator.EmulatorDownloadBinary);
                    DownloadCoreButton.IsEnabled = !string.IsNullOrEmpty(selectedEmulator.EmulatorDownloadCore);
                    DownloadExtrasButton.IsEnabled = !string.IsNullOrEmpty(selectedEmulator.EmulatorDownloadExtras);

                    DownloadEmulatorButton.Content = $"Download {selectedEmulator.EmulatorName} Emulator";
                    DownloadCoreButton.Content = $"Download {selectedEmulator.EmulatorName} Core";
                    DownloadExtrasButton.Content = $"Download {selectedEmulator.EmulatorName} Extras";
                }
            }
        }

        private async void DownloadEmulatorButton_Click(object sender, RoutedEventArgs e)
        {
            string emulatorName = EmulatorDropdown.SelectedItem.ToString();
            string appFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            string emulatorsFolderPath = Path.Combine(appFolderPath, "emulators");

            var selectedEmulator = _emulatorList.Emulators.Find(em => em.EmulatorName == emulatorName);
            if (selectedEmulator != null)
            {
                string emulatorFolderPath = Path.Combine(emulatorsFolderPath, emulatorName);
                string filePath = Path.Combine(emulatorFolderPath,
                    Path.GetFileName(selectedEmulator.EmulatorDownloadBinary));

                // Create directories if they don't exist
                Directory.CreateDirectory(emulatorFolderPath);

                bool success = await DownloadFileAsync(selectedEmulator.EmulatorDownloadBinary, filePath);

                if (success)
                {
                    // Check if the downloaded file is an executable
                    string fileExtension = Path.GetExtension(filePath).ToLower();
                    if (fileExtension == ".exe" || fileExtension == ".msi")
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = filePath,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error launching installer: {ex.Message}", "Launch Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        return;
                    }

                    // Show PleaseWaitExtraction window
                    var pleaseWaitWindow = new PleaseWaitExtraction();
                    pleaseWaitWindow.Show();

                    bool extractSuccess = await ExtractFileWith7ZipAsync(filePath, emulatorFolderPath);

                    // Close PleaseWaitExtraction window
                    pleaseWaitWindow.Close();

                    if (extractSuccess)
                    {
                        File.Delete(filePath);
                        MessageBox.Show("The download and extraction were successful!", "Download Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("The extraction failed. Please try again.", "Extraction Failed",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("The download failed. Please try again.", "Download Failed", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private async Task<bool> DownloadFileAsync(string url, string outputPath)
        {
            try
            {
                using HttpClient client = new HttpClient();
                using HttpResponseMessage response =
                    await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                await using Stream contentStream = await response.Content.ReadAsStreamAsync(),
                    fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192,
                        true);
                var totalRead = 0L;
                var buffer = new byte[8192];
                var isMoreToRead = true;

                while (isMoreToRead)
                {
                    var read = await contentStream.ReadAsync(buffer);
                    if (read == 0)
                    {
                        isMoreToRead = false;
                    }
                    else
                    {
                        await fileStream.WriteAsync(buffer.AsMemory(0, read));

                        totalRead += read;
                        if (canReportProgress)
                        {
                            var read1 = totalRead;
                            Dispatcher.Invoke(() => { DownloadProgressBar.Value = (double)read1 / totalBytes * 100; });
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading file: {ex.Message}", "Download Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
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

                // Optionally read output and error streams
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
        
        private void DownloadCoreButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement core download logic here
            throw new NotImplementedException();
        }

        private void DownloadExtrasButton_Click(object sender, RoutedEventArgs e)
        {
            // Implement extras download logic here
            throw new NotImplementedException();
        }
    }
}