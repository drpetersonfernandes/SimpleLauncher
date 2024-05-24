using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace SimpleLauncher
{
    public partial class EditSystemEasyModeAddSystem : Window
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
            var systems = new[]
            {
                "Amstrad CPC GX4000", "Arcade", "Atari 2600", "Atari 5200", "Atari 7800", "Atari 8-Bit",
                "Atari Jaguar", "Atari Jaguar CD", "Atari Lynx", "Atari ST", "Bandai WonderSwan", "Bandai WonderSwan Color",
                "Casio PV-1000", "Colecovision", "Commodore 64", "Commodore Amiga CD32", "LaserDisk", "Magnavox Odyssey 2",
                "Mattel Aquarius", "Mattel Intellivision", "Microsoft MSX", "Microsoft MSX2", "Microsoft Windows",
                "Microsoft Xbox", "Microsoft Xbox 360", "NEC PC Engine", "NEC PC Engine CD", "NEC PC-FX", "NEC Supergrafx",
                "Nintendo 3DS", "Nintendo 64", "Nintendo 64DD", "Nintendo DS", "Nintendo Family Computer Disk System",
                "Nintendo Game Boy", "Nintendo Game Boy Advance", "Nintendo Game Boy Color", "Nintendo GameCube",
                "Nintendo NES", "Nintendo Satellaview", "Nintendo SNES", "Nintendo SNES MSU1", "Nintendo Switch",
                "Nintendo Wii", "Nintendo WiiU", "Nintendo WiiWare", "Panasonic 3DO", "Philips CD-i", "ScummVM",
                "Sega Dreamcast", "Sega Game Gear", "Sega Genesis", "Sega Genesis 32X", "Sega Genesis CD", "Sega Master System",
                "Sega Model3", "Sega Saturn", "Sega SC-3000", "Sega SG-1000", "Sinclair ZX Spectrum", "SNK Neo Geo CD",
                "SNK Neo Geo Pocket", "SNK Neo Geo Pocket Color", "Sony Playstation 1", "Sony Playstation 2",
                "Sony Playstation 3", "Sony PSP"
            };

            foreach (var system in systems)
            {
                SystemNameDropdown.Items.Add(system);
            }
        }

        private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SystemNameDropdown.SelectedItem != null)
            {
                AlreadyHaveEmulatorDropdown.Items.Clear();
                AlreadyHaveEmulatorDropdown.Items.Add("Yes");
                AlreadyHaveEmulatorDropdown.Items.Add("No");
            }
        }

        private void AlreadyHaveEmulatorDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AlreadyHaveEmulatorDropdown.SelectedItem != null)
            {
                if (AlreadyHaveEmulatorDropdown.SelectedItem.ToString() == "Yes")
                {
                    // Handle the case where the user already has an emulator
                }
                else if (AlreadyHaveEmulatorDropdown.SelectedItem.ToString() == "No")
                {
                    LoadListOfEmulators(SystemNameDropdown.SelectedItem.ToString());
                }
            }
        }

        private void LoadListOfEmulators(string system)
        {
            EmulatorDropdown.Items.Clear();

            foreach (var emulator in _emulatorList.Emulators)
            {
                if (emulator.RelatedSystems.Contains(system))
                {
                    EmulatorDropdown.Items.Add(emulator.EmulatorName);
                }
            }

            if (EmulatorDropdown.Items.Count > 0)
            {
                DownloadEmulatorButton.IsEnabled = false;
                DownloadCoreButton.IsEnabled = false;
                DownloadExtrasButton.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("No emulator found for the selected system.", "No Emulators", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EmulatorDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmulatorDropdown.SelectedItem != null)
            {
                var selectedEmulator = _emulatorList.Emulators.Find(em => em.EmulatorName == EmulatorDropdown.SelectedItem.ToString());

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
                string filePath = Path.Combine(emulatorFolderPath, Path.GetFileName(selectedEmulator.EmulatorDownloadBinary));

                // Create directories if they don't exist
                Directory.CreateDirectory(emulatorFolderPath);

                bool success = await DownloadFileAsync(selectedEmulator.EmulatorDownloadBinary, filePath);

                if (success)
                {
                    bool extractSuccess = ExtractFile(filePath, emulatorFolderPath);

                    if (extractSuccess)
                    {
                        File.Delete(filePath);
                        MessageBox.Show("The download and extraction were successful!", "Download Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("The extraction failed. Please try again.", "Extraction Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("The download failed. Please try again.", "Download Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task<bool> DownloadFileAsync(string url, string outputPath)
        {
            try
            {
                using HttpClient client = new HttpClient();
                using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                await using Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
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
                            Dispatcher.Invoke(() =>
                            {
                                DownloadProgressBar.Value = (double)read1 / totalBytes * 100;
                            });
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading file: {ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool ExtractFile(string filePath, string destinationFolder)
        {
            try
            {
                using var archive = ArchiveFactory.Open(filePath);
                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(destinationFolder, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting file: {ex.Message}", "Extraction Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
