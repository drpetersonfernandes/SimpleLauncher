using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher
{
    public partial class EditSystemEasyModeAddSystem : Window
    {
        private static readonly string EmulatorDownloadUrl = "https://github.com/mamedev/mame/releases/download/mame0265/mame0265b_64bit.exe";

        public EditSystemEasyModeAddSystem()
        {
            InitializeComponent();
            ListOfSystems();
        }

        private void ListOfSystems()
        {
            SystemNameDropdown.Items.Add("Amstrad CPC GX4000");
            SystemNameDropdown.Items.Add("Atari 2600");
            SystemNameDropdown.Items.Add("Atari 5200");
            SystemNameDropdown.Items.Add("Atari 7800");
            SystemNameDropdown.Items.Add("Atari 8-Bit");
            SystemNameDropdown.Items.Add("Atari Jaguar");
            SystemNameDropdown.Items.Add("Atari Jaguar CD");
            SystemNameDropdown.Items.Add("Atari Lynx");
            SystemNameDropdown.Items.Add("Atari ST");
            SystemNameDropdown.Items.Add("Bandai WonderSwan");
            SystemNameDropdown.Items.Add("Bandai WonderSwan Color");
            SystemNameDropdown.Items.Add("Casio PV-1000");
            SystemNameDropdown.Items.Add("Colecovision");
            SystemNameDropdown.Items.Add("Commodore 64");
            SystemNameDropdown.Items.Add("Commodore Amiga CD32");
            SystemNameDropdown.Items.Add("LaserDisk");
            SystemNameDropdown.Items.Add("Magnavox Odyssey 2");
            SystemNameDropdown.Items.Add("MAME");
            SystemNameDropdown.Items.Add("Mattel Aquarius");
            SystemNameDropdown.Items.Add("Mattel Intellivision");
            SystemNameDropdown.Items.Add("Microsoft MSX");
            SystemNameDropdown.Items.Add("Microsoft MSX2");
            SystemNameDropdown.Items.Add("Microsoft Windows");
            SystemNameDropdown.Items.Add("Microsoft Xbox");
            SystemNameDropdown.Items.Add("Microsoft Xbox 360");
            SystemNameDropdown.Items.Add("NEC PC Engine");
            SystemNameDropdown.Items.Add("NEC PC Engine CD");
            SystemNameDropdown.Items.Add("NEC PC-FX");
            SystemNameDropdown.Items.Add("NEC Supergrafx");
            SystemNameDropdown.Items.Add("Nintendo 3DS");
            SystemNameDropdown.Items.Add("Nintendo 64");
            SystemNameDropdown.Items.Add("Nintendo 64DD");
            SystemNameDropdown.Items.Add("Nintendo DS");
            SystemNameDropdown.Items.Add("Nintendo Family Computer Disk System");
            SystemNameDropdown.Items.Add("Nintendo Game Boy");
            SystemNameDropdown.Items.Add("Nintendo Game Boy Advance");
            SystemNameDropdown.Items.Add("Nintendo Game Boy Color");
            SystemNameDropdown.Items.Add("Nintendo GameCube");
            SystemNameDropdown.Items.Add("Nintendo NES");
            SystemNameDropdown.Items.Add("Nintendo Satellaview");
            SystemNameDropdown.Items.Add("Nintendo SNES");
            SystemNameDropdown.Items.Add("Nintendo SNES MSU1");
            SystemNameDropdown.Items.Add("Nintendo Switch");
            SystemNameDropdown.Items.Add("Nintendo Wii");
            SystemNameDropdown.Items.Add("Nintendo WiiU");
            SystemNameDropdown.Items.Add("Nintendo WiiWare");
            SystemNameDropdown.Items.Add("Panasonic 3DO");
            SystemNameDropdown.Items.Add("Philips CD-i");
            SystemNameDropdown.Items.Add("ScummVM");
            SystemNameDropdown.Items.Add("Sega Dreamcast");
            SystemNameDropdown.Items.Add("Sega Game Gear");
            SystemNameDropdown.Items.Add("Sega Genesis");
            SystemNameDropdown.Items.Add("Sega Genesis 32X");
            SystemNameDropdown.Items.Add("Sega Genesis CD");
            SystemNameDropdown.Items.Add("Sega Master System");
            SystemNameDropdown.Items.Add("Sega Model3");
            SystemNameDropdown.Items.Add("Sega Saturn");
            SystemNameDropdown.Items.Add("Sega SC-3000");
            SystemNameDropdown.Items.Add("Sega SG-1000");
            SystemNameDropdown.Items.Add("Sinclair ZX Spectrum");
            SystemNameDropdown.Items.Add("SNK Neo Geo CD");
            SystemNameDropdown.Items.Add("SNK Neo Geo Pocket");
            SystemNameDropdown.Items.Add("SNK Neo Geo Pocket Color");
            SystemNameDropdown.Items.Add("Sony Playstation 1");
            SystemNameDropdown.Items.Add("Sony Playstation 2");
            SystemNameDropdown.Items.Add("Sony Playstation 3");
            SystemNameDropdown.Items.Add("Sony PSP");
        }

        private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SystemNameDropdown.SelectedItem != null)
            {
                AskUser1.Visibility = Visibility.Visible;
                AlreadyHaveEmulatorDropdown.Visibility = Visibility.Visible;
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
                    // You can add more logic here if needed
                }
                else if (AlreadyHaveEmulatorDropdown.SelectedItem.ToString() == "No")
                {
                    MessageBox.Show("Please choose one emulator from the list below", "Choose one emulator", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadListOfEmulators(SystemNameDropdown.SelectedItem.ToString());
                }
            }
        }

        private void LoadListOfEmulators(string system)
        {
            EmulatorDropdown.Items.Clear();
            EmulatorDropdown.Visibility = Visibility.Visible;
            AskUser2.Visibility = Visibility.Visible;

            switch (system)
            {
                case "Amstrad CPC GX4000":
                    EmulatorDropdown.Items.Add("Emulator1 for Amstrad CPC GX4000");
                    EmulatorDropdown.Items.Add("Emulator2 for Amstrad CPC GX4000");
                    break;
                case "Atari 2600":
                    EmulatorDropdown.Items.Add("Emulator1 for Atari 2600");
                    EmulatorDropdown.Items.Add("Emulator2 for Atari 2600");
                    break;
                case "Atari 5200":
                    EmulatorDropdown.Items.Add("Emulator1 for Atari 5200");
                    EmulatorDropdown.Items.Add("Emulator2 for Atari 5200");
                    break;
                case "Atari 7800":
                    EmulatorDropdown.Items.Add("Emulator1 for Atari 7800");
                    EmulatorDropdown.Items.Add("Emulator2 for Atari 7800");
                    break;
                // Add other cases for other systems
                default:
                    EmulatorDropdown.Items.Add("Default Emulator");
                    break;
            }
            DownloadButton.Visibility = Visibility.Visible;
        }

        private void EmulatorDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmulatorDropdown.SelectedItem != null)
            {
                DownloadButton.Visibility = Visibility.Visible;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string emulatorName = EmulatorDropdown.SelectedItem.ToString();
            string appFolderPath = AppDomain.CurrentDomain.BaseDirectory;
            string emulatorsFolderPath = Path.Combine(appFolderPath, "emulators");
            string emulatorFolderPath = Path.Combine(emulatorsFolderPath, emulatorName);
            string filePath = Path.Combine(emulatorFolderPath, "mame0265b_64bit.exe");

            // Create directories if they don't exist
            Directory.CreateDirectory(emulatorFolderPath);

            MessageBox.Show($"I will try to download {emulatorName} for you.", "Download Emulator", 
                MessageBoxButton.OK, MessageBoxImage.Information);

            DownloadProgressBar.Visibility = Visibility.Visible;
            bool success = await DownloadFileAsync(EmulatorDownloadUrl, filePath);

            DownloadProgressBar.Visibility = Visibility.Hidden;

            if (success)
            {
                MessageBox.Show("The download was successful!", "Download Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("The download failed. Please try again.", "Download Failed", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
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
                            Dispatcher.Invoke(() =>
                            {
                                DownloadProgressBar.Value = (double)totalRead / totalBytes * 100;
                            });
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
