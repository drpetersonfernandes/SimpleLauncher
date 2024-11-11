using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace SimpleLauncher
{
    public partial class EditSystem
    {
        private XDocument _xmlDoc;
        private readonly string _xmlFilePath = "system.xml";
        private static readonly char[] SplitSeparators = [',', '|'];
        private readonly SettingsConfig _settings;

        public EditSystem(SettingsConfig settings)
        {
            InitializeComponent();
            _settings = settings;
            LoadXml();
            PopulateSystemNamesDropdown();
            
            // Attach event handler
            this.Closing += EditSystem_Closing; 
            
            SaveSystemButton.IsEnabled = false;
            DeleteSystemButton.IsEnabled = false;
            
            App.ApplyThemeToWindow(this);
        }

        private void LoadXml()
        {
            if (!File.Exists(_xmlFilePath))
            {
                MessageBox.Show("'system.xml' not found inside the application folder!\n\nPlease restart Simple Launcher.\n\nIf that does not work, please reinstall Simple Launcher.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Shutdown SimpleLauncher
                Application.Current.Shutdown();
                Environment.Exit(0);
            }
            else
            {
                _xmlDoc = XDocument.Load(_xmlFilePath);
            }
        }

        private void PopulateSystemNamesDropdown()
        {
            if (_xmlDoc == null) return;
            SystemNameDropdown.ItemsSource = _xmlDoc.Descendants("SystemConfig")
                .Select(element => element.Element("SystemName")?.Value)
                .OrderBy(name => name)
                .ToList();
        }

        private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnableFields();
            SaveSystemButton.IsEnabled = true;
            DeleteSystemButton.IsEnabled = true;
            
            if (SystemNameDropdown.SelectedItem == null) return;
            if (_xmlDoc == null) return;

            string selectedSystemName = SystemNameDropdown.SelectedItem.ToString();
            var selectedSystem = _xmlDoc.Descendants("SystemConfig")
                .FirstOrDefault(x => x.Element("SystemName")?.Value == selectedSystemName);

            if (selectedSystem != null)
            {
                SystemNameTextBox.Text = selectedSystem.Element("SystemName")?.Value ?? string.Empty;
                SystemFolderTextBox.Text = selectedSystem.Element("SystemFolder")?.Value ?? string.Empty;
                SystemImageFolderTextBox.Text = selectedSystem.Element("SystemImageFolder")?.Value ?? string.Empty;

                var systemIsMameValue = selectedSystem.Element("SystemIsMAME")?.Value == "true" ? "true" : "false";
                SystemIsMameComboBox.SelectedItem = SystemIsMameComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == systemIsMameValue);

                // Handle multiple FormatToSearch values
                var formatToSearchValues = selectedSystem.Element("FileFormatsToSearch")?.Elements("FormatToSearch")
                    .Select(x => x.Value)
                    .ToArray();
                FormatToSearchTextBox.Text = formatToSearchValues != null
                    ? String.Join(", ", formatToSearchValues)
                    : string.Empty;

                var extractFileBeforeLaunchValue = selectedSystem.Element("ExtractFileBeforeLaunch")?.Value == "true"
                    ? "true"
                    : "false";
                ExtractFileBeforeLaunchComboBox.SelectedItem = ExtractFileBeforeLaunchComboBox.Items
                    .Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == extractFileBeforeLaunchValue);

                // Handle multiple FormatToLaunch values
                var formatToLaunchValues = selectedSystem.Element("FileFormatsToLaunch")?.Elements("FormatToLaunch")
                    .Select(x => x.Value)
                    .ToArray();
                FormatToLaunchTextBox.Text = formatToLaunchValues != null
                    ? String.Join(", ", formatToLaunchValues)
                    : string.Empty;

                var emulators = selectedSystem.Element("Emulators")?.Elements("Emulator").ToList();
                if (emulators != null)
                {
                    var emulator1 = emulators.ElementAtOrDefault(0);
                    if (emulator1 != null)
                    {
                        Emulator1NameTextBox.Text = emulator1.Element("EmulatorName")?.Value ?? string.Empty;
                        Emulator1LocationTextBox.Text = emulator1.Element("EmulatorLocation")?.Value ?? string.Empty;
                        Emulator1ParametersTextBox.Text =
                            emulator1.Element("EmulatorParameters")?.Value ?? string.Empty;
                    }
                    else
                    {
                        Emulator1NameTextBox.Clear();
                        Emulator1LocationTextBox.Clear();
                        Emulator1ParametersTextBox.Clear();
                    }

                    var emulator2 = emulators.ElementAtOrDefault(1);
                    if (emulator2 != null)
                    {
                        Emulator2NameTextBox.Text = emulator2.Element("EmulatorName")?.Value ?? string.Empty;
                        Emulator2LocationTextBox.Text = emulator2.Element("EmulatorLocation")?.Value ?? string.Empty;
                        Emulator2ParametersTextBox.Text =
                            emulator2.Element("EmulatorParameters")?.Value ?? string.Empty;
                    }
                    else
                    {
                        Emulator2NameTextBox.Clear();
                        Emulator2LocationTextBox.Clear();
                        Emulator2ParametersTextBox.Clear();
                    }

                    var emulator3 = emulators.ElementAtOrDefault(2);
                    if (emulator3 != null)
                    {
                        Emulator3NameTextBox.Text = emulator3.Element("EmulatorName")?.Value ?? string.Empty;
                        Emulator3LocationTextBox.Text = emulator3.Element("EmulatorLocation")?.Value ?? string.Empty;
                        Emulator3ParametersTextBox.Text =
                            emulator3.Element("EmulatorParameters")?.Value ?? string.Empty;
                    }
                    else
                    {
                        Emulator3NameTextBox.Clear();
                        Emulator3LocationTextBox.Clear();
                        Emulator3ParametersTextBox.Clear();
                    }

                    var emulator4 = emulators.ElementAtOrDefault(3);
                    if (emulator4 != null)
                    {
                        Emulator4NameTextBox.Text = emulator4.Element("EmulatorName")?.Value ?? string.Empty;
                        Emulator4LocationTextBox.Text = emulator4.Element("EmulatorLocation")?.Value ?? string.Empty;
                        Emulator4ParametersTextBox.Text =
                            emulator4.Element("EmulatorParameters")?.Value ?? string.Empty;
                    }
                    else
                    {
                        Emulator4NameTextBox.Clear();
                        Emulator4LocationTextBox.Clear();
                        Emulator4ParametersTextBox.Clear();
                    }

                    var emulator5 = emulators.ElementAtOrDefault(4);
                    if (emulator5 != null)
                    {
                        Emulator5NameTextBox.Text = emulator5.Element("EmulatorName")?.Value ?? string.Empty;
                        Emulator5LocationTextBox.Text = emulator5.Element("EmulatorLocation")?.Value ?? string.Empty;
                        Emulator5ParametersTextBox.Text =
                            emulator5.Element("EmulatorParameters")?.Value ?? string.Empty;
                    }
                    else
                    {
                        Emulator5NameTextBox.Clear();
                        Emulator5LocationTextBox.Clear();
                        Emulator5ParametersTextBox.Clear();
                    }
                    // Adjust the visibility of the placeholder based on the newly loaded data
                    AdjustPlaceholderVisibility();
                }
            }
            
            // Validate System Folder and System Image Folder
            MarkInvalid(SystemFolderTextBox, IsValidPath(SystemFolderTextBox.Text));
            MarkInvalid(SystemImageFolderTextBox, IsValidPath(SystemImageFolderTextBox.Text));

            // Validate Emulator Location Text Boxes (considered valid if empty)
            MarkInvalid(Emulator1LocationTextBox, string.IsNullOrWhiteSpace(Emulator1LocationTextBox.Text) || IsValidPath(Emulator1LocationTextBox.Text));
            MarkInvalid(Emulator2LocationTextBox, string.IsNullOrWhiteSpace(Emulator2LocationTextBox.Text) || IsValidPath(Emulator2LocationTextBox.Text));
            MarkInvalid(Emulator3LocationTextBox, string.IsNullOrWhiteSpace(Emulator3LocationTextBox.Text) || IsValidPath(Emulator3LocationTextBox.Text));
            MarkInvalid(Emulator4LocationTextBox, string.IsNullOrWhiteSpace(Emulator4LocationTextBox.Text) || IsValidPath(Emulator4LocationTextBox.Text));
            MarkInvalid(Emulator5LocationTextBox, string.IsNullOrWhiteSpace(Emulator5LocationTextBox.Text) || IsValidPath(Emulator5LocationTextBox.Text));
            
            // Validate Parameters (considered valid if empty)
            MarkInvalid(Emulator1ParametersTextBox, string.IsNullOrWhiteSpace(Emulator1ParametersTextBox.Text) || IsValidPath2(Emulator1ParametersTextBox.Text));
            MarkInvalid(Emulator2ParametersTextBox, string.IsNullOrWhiteSpace(Emulator2ParametersTextBox.Text) || IsValidPath2(Emulator2ParametersTextBox.Text));
            MarkInvalid(Emulator3ParametersTextBox, string.IsNullOrWhiteSpace(Emulator3ParametersTextBox.Text) || IsValidPath2(Emulator3ParametersTextBox.Text));
            MarkInvalid(Emulator4ParametersTextBox, string.IsNullOrWhiteSpace(Emulator4ParametersTextBox.Text) || IsValidPath2(Emulator4ParametersTextBox.Text));
            MarkInvalid(Emulator5ParametersTextBox, string.IsNullOrWhiteSpace(Emulator5ParametersTextBox.Text) || IsValidPath2(Emulator5ParametersTextBox.Text));
        }
        
        private bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            // Directly check if the path exists (for absolute paths)
            if (Directory.Exists(path) || File.Exists(path)) return true;

            // Allow relative paths
            // Combine with the base directory to check for relative paths
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            // Ensure we correctly handle relative paths that go up from the base directory
            string fullPath = Path.GetFullPath(new Uri(Path.Combine(basePath, path)).LocalPath);

            return Directory.Exists(fullPath) || File.Exists(fullPath);
        }
        
        private bool IsValidPath2(string parameters)
        {
            // Return true immediately if the parameter string is empty or null.
            if (string.IsNullOrWhiteSpace(parameters)) return true;

            // This pattern looks for paths inside double or single quotes, excluding non-path arguments.
            string pattern = @"(?:-L\s+""([^""]+)""|-rompath\s+""([^""]+)"")|""([^""]+\\[^""]+|[a-zA-Z]:\\[^""]+)""|'([^']+\\[^']+|[a-zA-Z]:\\[^']+)'";
            var matches = Regex.Matches(parameters, pattern);

            // Use the application's current directory as the base for relative paths.
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Iterate over all matches to validate each path found.
            foreach (Match match in matches)
            {
                // Extract the path from the match, considering different groups in the regex.
                string path = match.Groups[1].Success ? match.Groups[1].Value :
                    match.Groups[2].Success ? match.Groups[2].Value :
                    match.Groups[3].Success ? match.Groups[3].Value :
                    match.Groups[4].Value;

                // Check if the path contains more than one '..\' sequence.
                int doubleDotCount = path.Split(new string[] { @"..\" }, StringSplitOptions.None).Length - 1;
                if (doubleDotCount > 1)
                {
                    continue; // Skip validation for paths with multiple '..\' components.
                }

                // Convert relative paths to absolute paths using the base directory.
                string absolutePath = Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(basePath, path));

                // Check if the path (either absolute or converted from relative) is valid.
                bool isValid = Directory.Exists(absolutePath) || File.Exists(absolutePath);

                // If any path is invalid, return false immediately.
                if (!isValid)
                {
                    return false;
                }
            }

            // Return true if all paths (if any) are valid.
            return true;
        }

        private void MarkInvalid(TextBox textBox, bool isValid)
        {
            if (isValid)
            {
                SetTextBoxForeground(textBox, true); // Valid state
            }
            else
            {
                textBox.Foreground = System.Windows.Media.Brushes.Red; // Invalid state
            }
        }
       
        private void MarkValid(TextBox textBox)
        {
            SetTextBoxForeground(textBox, true); // Always valid state
        }
        
        private void SetTextBoxForeground(TextBox textBox, bool isValid)
        {
            string baseTheme = _settings.BaseTheme;
            if (baseTheme == "Dark")
            {
                textBox.Foreground = isValid ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Red;
            }
            else
            {
                textBox.Foreground = isValid ? System.Windows.Media.Brushes.Black : System.Windows.Media.Brushes.Red;
            }
        }

        private void ChooseSystemFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = @"Please select the System Folder";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string foldername = dialog.SelectedPath;
                SystemFolderTextBox.Text = foldername;

                // Adjust the visibility of the placeholder based on the newly loaded data
                AdjustPlaceholderVisibility();
                MarkValid(SystemFolderTextBox);
            }
        }
        
        private void ChooseSystemImageFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.Description = @"Please select the System Image Folder";
            DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string foldername = dialog.SelectedPath.Trim();
                SystemImageFolderTextBox.Text = foldername;

                // Adjust the visibility of the placeholder based on the newly loaded data
                AdjustPlaceholderVisibility();
                MarkValid(SystemImageFolderTextBox);
            }
        }

        private void ChooseEmulator1Location(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "Exe File (.exe)|*.exe",
                Title = "Select Emulator 1"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                Emulator1LocationTextBox.Text = filename;
                MarkValid(Emulator1LocationTextBox);
            }
        }

        private void ChooseEmulator2Location(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "EXE File (.exe)|*.exe",
                Title = "Select Emulator 2"
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                Emulator2LocationTextBox.Text = filename;
                MarkValid(Emulator2LocationTextBox);
            }
        }

        private void ChooseEmulator3Location(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "EXE File (.exe)|*.exe",
                Title = "Select Emulator 3"
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                Emulator3LocationTextBox.Text = filename;
                MarkValid(Emulator3LocationTextBox);
            }
        }

        private void ChooseEmulator4Location(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "EXE File (.exe)|*.exe",
                Title = "Select Emulator 4"
            };

            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                Emulator4LocationTextBox.Text = filename;
                MarkValid(Emulator4LocationTextBox);
            }
        }

        private void ChooseEmulator5Location(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".exe",
                Filter = "EXE File (.exe)|*.exe",
                Title = "Select Emulator 5"
            };
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                Emulator5LocationTextBox.Text = filename;
                MarkValid(Emulator5LocationTextBox);
            }
        }

        private void AddSystemButton_Click(object sender, RoutedEventArgs e)
        {
            EnableFields();
            ClearFields();
            AdjustPlaceholderVisibility();

            SaveSystemButton.IsEnabled = true;
            DeleteSystemButton.IsEnabled = false;
            
            MessageBox.Show("You can add a new system now.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void EnableFields()
        {
            SystemNamePlaceholderTextBox.IsReadOnly = false;
            SystemNamePlaceholderTextBox.IsEnabled = true;

            SystemFolderPlaceholderTextBox.IsReadOnly = false;
            SystemFolderPlaceholderTextBox.IsEnabled = true;
            
            SystemImageFolderTextBox.IsReadOnly = false;
            SystemImageFolderTextBox.IsEnabled = true;
            
            SystemIsMameComboBox.IsReadOnly = false;
            SystemIsMameComboBox.IsEnabled = true;
            
            FormatToSearchPlaceholderTextBox.IsReadOnly = false;
            FormatToSearchPlaceholderTextBox.IsEnabled = true;
            
            ExtractFileBeforeLaunchComboBox.IsReadOnly = false;
            ExtractFileBeforeLaunchComboBox.IsEnabled = true;
            
            FormatToLaunchPlaceholderTextBox.IsReadOnly = false;
            FormatToLaunchPlaceholderTextBox.IsEnabled = true;
            
            Emulator1NamePlaceholderTextBox.IsReadOnly = false;
            Emulator1NamePlaceholderTextBox.IsEnabled = true;
            
            Emulator1LocationTextBox.IsReadOnly = false;
            Emulator1LocationTextBox.IsEnabled = true;
            
            Emulator1ParametersTextBox.IsReadOnly = false;
            Emulator1ParametersTextBox.IsEnabled = true;
            
            Emulator2NameTextBox.IsReadOnly = false;
            Emulator2NameTextBox.IsEnabled = true;
            
            Emulator2LocationTextBox.IsReadOnly = false;
            Emulator2LocationTextBox.IsEnabled = true;
            
            Emulator2ParametersTextBox.IsReadOnly = false;
            Emulator2ParametersTextBox.IsEnabled = true;
            
            Emulator3NameTextBox.IsReadOnly = false;
            Emulator3NameTextBox.IsEnabled = true;
            
            Emulator3LocationTextBox.IsReadOnly = false;
            Emulator3LocationTextBox.IsEnabled = true;
            
            Emulator3ParametersTextBox.IsReadOnly = false;
            Emulator3ParametersTextBox.IsEnabled = true;
            
            Emulator4NameTextBox.IsReadOnly = false;
            Emulator4NameTextBox.IsEnabled = true;
            
            Emulator4LocationTextBox.IsReadOnly = false;
            Emulator4LocationTextBox.IsEnabled = true;
            
            Emulator4ParametersTextBox.IsReadOnly = false;
            Emulator4ParametersTextBox.IsEnabled = true;
            
            Emulator5NameTextBox.IsReadOnly = false;
            Emulator5NameTextBox.IsEnabled = true;
            
            Emulator5LocationTextBox.IsReadOnly = false;
            Emulator5LocationTextBox.IsEnabled = true;
            
            Emulator5ParametersTextBox.IsReadOnly = false;
            Emulator5ParametersTextBox.IsEnabled = true;
            
            ChooseSystemFolderButton.IsEnabled = true;
            ChooseSystemImageFolderButton.IsEnabled = true;
            ChooseEmulator1LocationButton.IsEnabled = true;
            ChooseEmulator2LocationButton.IsEnabled = true;
            ChooseEmulator3LocationButton.IsEnabled = true;
            ChooseEmulator4LocationButton.IsEnabled = true;
            ChooseEmulator5LocationButton.IsEnabled = true;
        }

        private void ClearFields()
        {
            SystemNameDropdown.SelectedItem = null;
            SystemNameTextBox.Text = string.Empty;
            SystemFolderTextBox.Text = string.Empty;
            SystemImageFolderTextBox.Text = string.Empty;
            SystemIsMameComboBox.SelectedItem = null;
            FormatToSearchTextBox.Text = string.Empty;
            ExtractFileBeforeLaunchComboBox.SelectedItem = null;
            FormatToLaunchTextBox.Text = string.Empty;
            Emulator1NameTextBox.Text = string.Empty;
            Emulator1LocationTextBox.Text = string.Empty;
            Emulator1ParametersTextBox.Text = string.Empty;
            Emulator2NameTextBox.Text = string.Empty;
            Emulator2LocationTextBox.Text = string.Empty;
            Emulator2ParametersTextBox.Text = string.Empty;
            Emulator3NameTextBox.Text = string.Empty;
            Emulator3LocationTextBox.Text = string.Empty;
            Emulator3ParametersTextBox.Text = string.Empty;
            Emulator4NameTextBox.Text = string.Empty;
            Emulator4LocationTextBox.Text = string.Empty;
            Emulator4ParametersTextBox.Text = string.Empty;
            Emulator5NameTextBox.Text = string.Empty;
            Emulator5LocationTextBox.Text = string.Empty;
            Emulator5ParametersTextBox.Text = string.Empty;
        }

        private void SaveSystemButton_Click(object sender, RoutedEventArgs e)
        {
            
            // Format input values
            string systemName = SystemNamePlaceholderTextBox.Text.Trim();
            string systemFolder = string.IsNullOrWhiteSpace(SystemFolderTextBox.Text) ? $".\\roms\\{systemName}" : SystemFolderTextBox.Text.Trim();
            string systemImageFolder = string.IsNullOrWhiteSpace(SystemImageFolderTextBox.Text) ? $".\\images\\{systemName}" : SystemImageFolderTextBox.Text.Trim();
            bool systemIsMame = (bool)SystemIsMameComboBox.SelectedItem;

            List<string> formatToSearch = FormatToSearchTextBox.Text
                .Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(format => format.Trim().ToLower())
                .ToList();
            
            bool extractFileBeforeLaunch = (bool)ExtractFileBeforeLaunchComboBox.SelectedItem;

            List<string> formatToLaunch = FormatToLaunchPlaceholderTextBox.Text
                .Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(format => format.Trim().ToLower())
                .ToList();
            
            string emulator1Name = Emulator1NamePlaceholderTextBox.Text.Trim();
            string emulator1Location = Emulator1LocationTextBox.Text.Trim();
            string emulator1Parameters = Emulator1ParametersTextBox.Text.Trim();
            
            string emulator2Name = Emulator2NameTextBox.Text.Trim();
            string emulator2Location = Emulator2LocationTextBox.Text.Trim();
            string emulator2Parameters = Emulator2ParametersTextBox.Text.Trim();
            
            string emulator3Name = Emulator3NameTextBox.Text.Trim();
            string emulator3Location = Emulator3LocationTextBox.Text.Trim();
            string emulator3Parameters = Emulator3ParametersTextBox.Text.Trim();
            
            string emulator4Name = Emulator4NameTextBox.Text.Trim();
            string emulator4Location = Emulator4LocationTextBox.Text.Trim();
            string emulator4Parameters = Emulator4ParametersTextBox.Text.Trim();
            
            string emulator5Name = Emulator5NameTextBox.Text.Trim();
            string emulator5Location = Emulator5LocationTextBox.Text.Trim();
            string emulator5Parameters = Emulator5ParametersTextBox.Text.Trim();
            
            // Check systemName
            if (string.IsNullOrWhiteSpace(systemName))
            {
                MessageBox.Show("'System Name' is required and cannot contain empty values.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Check systemFolder
            bool isSystemFolderValid = IsValidPath(systemFolder);
            MarkInvalid(SystemFolderTextBox, isSystemFolderValid);
            
            // Check systemImageFolder
            bool isSystemImageFolderValid = IsValidPath(systemImageFolder);
            MarkInvalid(SystemImageFolderTextBox, isSystemImageFolderValid);
            
            // Check formatToSearch
            if (formatToSearch.Count == 0 || formatToSearch.Any(string.IsNullOrWhiteSpace))
            {
                MessageBox.Show("'Format to Search in the System Folder' is required and cannot contain empty values.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (extractFileBeforeLaunch && !formatToSearch.Any(f => f == "7z" || f == "zip" || f == "rar"))
            {
                MessageBox.Show("When 'Extract File Before Launch' is set to true the 'Format to Search in the System Folder' must include 7z, zip, or rar.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Check formatToLaunch
            if (extractFileBeforeLaunch && (formatToLaunch.Count == 0))
            {
                MessageBox.Show("When 'Extract File Before Launch' is set to true, the 'Format to Launch After Extraction' is required.",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Check emulator1Name
            if (string.IsNullOrWhiteSpace(emulator1Name))
            {
                MessageBox.Show("'Emulator 1 Name' is required.", "Validation error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Check emulator1Location
            bool isEmulator1LocationValid = IsValidPath(emulator1Location);
            MarkInvalid(Emulator1LocationTextBox, isEmulator1LocationValid);

            // Check emulator1Parameters
            bool isEmulator1ParametersValid = string.IsNullOrWhiteSpace(emulator1Parameters) || IsValidPath2(emulator1Parameters);
            MarkInvalid(Emulator1ParametersTextBox, isEmulator1ParametersValid);
            
            // Check emulator2Name
            
            // Check emulator2Location
            bool isEmulator2LocationValid = IsValidPath(emulator2Location);
            MarkInvalid(Emulator2LocationTextBox, isEmulator2LocationValid);
            
            // Check emulator2Parameters
            bool isEmulator2ParametersValid = string.IsNullOrWhiteSpace(emulator2Parameters) || IsValidPath2(emulator2Parameters);
            MarkInvalid(Emulator2ParametersTextBox, isEmulator2ParametersValid);
            
            // Check emulator3Name
            
            // Check emulator3Location
            bool isEmulator3LocationValid = IsValidPath(emulator3Location);
            MarkInvalid(Emulator3LocationTextBox, isEmulator3LocationValid);
            
            // Check emulator3Parameters
            bool isEmulator3ParametersValid = string.IsNullOrWhiteSpace(emulator3Parameters) || IsValidPath2(emulator3Parameters);
            MarkInvalid(Emulator3ParametersTextBox, isEmulator3ParametersValid);
            
            // Check emulator4Name
            
            // Check emulator4Location
            bool isEmulator4LocationValid = IsValidPath(emulator4Location);
            MarkInvalid(Emulator4LocationTextBox, isEmulator4LocationValid);
            
            // Check emulator4Parameters
            bool isEmulator4ParametersValid = string.IsNullOrWhiteSpace(emulator4Parameters) || IsValidPath2(emulator4Parameters);
            MarkInvalid(Emulator4ParametersTextBox, isEmulator4ParametersValid);
            
            // Check emulator5Name
            
            // Check emulator5Location
            bool isEmulator5LocationValid = IsValidPath(emulator5Location);
            MarkInvalid(Emulator5LocationTextBox, isEmulator5LocationValid);
            
            // Check emulator4Parameters
            bool isEmulator5ParametersValid = string.IsNullOrWhiteSpace(emulator5Parameters) || IsValidPath2(emulator5Parameters);
            MarkInvalid(Emulator5ParametersTextBox, isEmulator5ParametersValid);

            // Check validation results before proceeding
            if (!isSystemFolderValid || !isSystemImageFolderValid || !isEmulator1LocationValid || !isEmulator2LocationValid || !isEmulator3LocationValid ||
                !isEmulator4LocationValid || !isEmulator5LocationValid || !isEmulator1ParametersValid || !isEmulator2ParametersValid ||
                !isEmulator3ParametersValid || !isEmulator4ParametersValid || !isEmulator5ParametersValid)
            {
                MessageBox.Show("One or more paths or parameters are invalid.\n\nPlease correct them to proceed.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Create emulator configurations
            List<SystemConfig.Emulator> emulators = new List<SystemConfig.Emulator>();
            AddEmulator(emulators, emulator1Name, emulator1Location, emulator1Parameters);
            AddEmulator(emulators, emulator2Name, emulator2Location, emulator2Parameters);
            AddEmulator(emulators, emulator3Name, emulator3Location, emulator3Parameters);
            AddEmulator(emulators, emulator4Name, emulator4Location, emulator4Parameters);
            AddEmulator(emulators, emulator5Name, emulator5Location, emulator5Parameters);
            
            // Create a new SystemConfig instance
            var newSystemConfig = new SystemConfig
            {
                SystemName = systemName,
                SystemFolder = systemFolder,
                SystemImageFolder = systemImageFolder,
                SystemIsMame = systemIsMame,
                FileFormatsToSearch = formatToSearch,
                ExtractFileBeforeLaunch = extractFileBeforeLaunch,
                FileFormatsToLaunch = formatToLaunch,
                Emulators = emulators
            };
            
            // Load current configs, update or add new config, and save
            List<SystemConfig> systemConfigs = SystemConfig.LoadSystemConfigs() ?? new List<SystemConfig>();
            var existingConfig = systemConfigs.FirstOrDefault(config => config.SystemName == systemName);
            if (existingConfig != null)
                systemConfigs.Remove(existingConfig);

            systemConfigs.Add(newSystemConfig);
            SystemConfig.SaveSystemConfigs(systemConfigs);
            
            // Create folders for each system
            CreateSystemFolders(systemName);
            
            MessageBox.Show("System configuration saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        // Helper to add emulator only if name is provided
        private void AddEmulator(List<SystemConfig.Emulator> emulators, string name, string location, string parameters)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                emulators.Add(new SystemConfig.Emulator
                {
                    EmulatorName = name.Trim(),
                    EmulatorLocation = location.Trim(),
                    EmulatorParameters = parameters.Trim()
                });
            }
        }
        
        private void CreateSystemFolders(string systemName)
        {
            string applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string[] folderNames = ["roms", "images", "title_snapshots", "gameplay_snapshots", "videos", "manuals", "walkthrough", "cabinets", "flyers", "pcbs", "carts"];

            foreach (var folderName in folderNames)
            {
                string parentDirectory = Path.Combine(applicationDirectory, folderName);

                if (!Directory.Exists(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }

                string newFolderPath = Path.Combine(parentDirectory, systemName);

                try
                {
                    if (!Directory.Exists(newFolderPath))
                    {
                        Directory.CreateDirectory(newFolderPath);
                        if (folderName == "images")
                        {
                            MessageBox.Show($"'Simple Launcher' created a image folder for this System at {applicationDirectory}\\images\\{systemName}.\n\n" +
                                            $"You may place the cover images for this System inside this folder.\n\n" +
                                            $"I also created folders for \"title_snapshots\", \"gameplay_snapshots\", \"videos\", \"manuals\", \"walkthrough\", \"cabinets\", \"flyers\", \"pcbs\" and \"carts\" inside the 'Simple Launcher' folder.",
                                "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string formattedException = $"'Simple Launcher' failed to create the necessary folders for the newly created system.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                    Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                    logTask.Wait(TimeSpan.FromSeconds(2));

                    MessageBox.Show($"'Simple Launcher' failed to create the necessary folders for this system.\n\n" +
                                    $"Probably the application does not have write privileges.\n\n" +
                                    $"Try to run 'Simple Launcher' with administrative privileges.",
                        "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeleteSystemButton_Click(object sender, RoutedEventArgs e)
        {
            if (SystemNameDropdown.SelectedItem == null)
            {
                MessageBox.Show("Please select a system to delete.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedSystemName = SystemNameDropdown.SelectedItem.ToString();

            if (_xmlDoc == null) return;
            var systemNode = _xmlDoc.Descendants("SystemConfig")
                .FirstOrDefault(element => element.Element("SystemName")?.Value == selectedSystemName);

            if (systemNode != null)
            {
                //Ask user if he really wants to delete the system
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this system?", "Confirmation",
                    MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    systemNode.Remove();
                    _xmlDoc.Save(_xmlFilePath);
                    PopulateSystemNamesDropdown();
                    ClearFields();
                    AdjustPlaceholderVisibility();
                    
                    MessageBox.Show($"System '{selectedSystemName}' has been deleted.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Selected system not found in the XML document!", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void ToggleEmulator3Button_Click(object sender, RoutedEventArgs e)
        {
            if (Emulator3NameLabel.Visibility == Visibility.Visible)
            {
                ToggleEmulator3Button.Visibility = Visibility.Collapsed;
                Emulator3NameLabel.Visibility = Visibility.Collapsed;
                Emulator3NameTextBox.Visibility = Visibility.Collapsed;
                Emulator3NameLabel2.Visibility = Visibility.Collapsed;
                Emulator3LocationTextBox.Visibility = Visibility.Collapsed;
                ChooseEmulator3LocationButton.Visibility = Visibility.Collapsed;
                Emulator3NameLabel3.Visibility = Visibility.Collapsed;
                Emulator3ParametersTextBox.Visibility = Visibility.Collapsed;
                Emulator3ParametersHelper.Visibility = Visibility.Collapsed;
            }
            else
            {
                ToggleEmulator3Button.Visibility = Visibility.Collapsed;
                Emulator3NameLabel.Visibility = Visibility.Visible;
                Emulator3NameTextBox.Visibility = Visibility.Visible;
                Emulator3NameLabel2.Visibility = Visibility.Visible;
                Emulator3LocationTextBox.Visibility = Visibility.Visible;
                ChooseEmulator3LocationButton.Visibility = Visibility.Visible;
                Emulator3NameLabel3.Visibility = Visibility.Visible;
                Emulator3ParametersTextBox.Visibility = Visibility.Visible;
                Emulator3ParametersHelper.Visibility = Visibility.Visible;
            }
        }

        private void ToggleEmulator4Button_Click(object sender, RoutedEventArgs e)
        {
            if (Emulator4NameLabel.Visibility == Visibility.Visible)
            {
                ToggleEmulator4Button.Visibility = Visibility.Collapsed;
                Emulator4NameLabel.Visibility = Visibility.Collapsed;
                Emulator4NameTextBox.Visibility = Visibility.Collapsed;
                Emulator4NameLabel2.Visibility = Visibility.Collapsed;
                Emulator4LocationTextBox.Visibility = Visibility.Collapsed;
                ChooseEmulator4LocationButton.Visibility = Visibility.Collapsed;
                Emulator4NameLabel3.Visibility = Visibility.Collapsed;
                Emulator4ParametersTextBox.Visibility = Visibility.Collapsed;
                Emulator4ParametersHelper.Visibility = Visibility.Collapsed;
            }
            else
            {
                ToggleEmulator4Button.Visibility = Visibility.Collapsed;
                Emulator4NameLabel.Visibility = Visibility.Visible;
                Emulator4NameTextBox.Visibility = Visibility.Visible;
                Emulator4NameLabel2.Visibility = Visibility.Visible;
                Emulator4LocationTextBox.Visibility = Visibility.Visible;
                ChooseEmulator4LocationButton.Visibility = Visibility.Visible;
                Emulator4NameLabel3.Visibility = Visibility.Visible;
                Emulator4ParametersTextBox.Visibility = Visibility.Visible;
                Emulator4ParametersHelper.Visibility = Visibility.Visible;
            }
        }

        private void ToggleEmulator5Button_Click(object sender, RoutedEventArgs e)
        {
            if (Emulator5NameLabel.Visibility == Visibility.Visible)
            {
                ToggleEmulator5Button.Visibility = Visibility.Collapsed;
                Emulator5NameLabel.Visibility = Visibility.Collapsed;
                Emulator5NameTextBox.Visibility = Visibility.Collapsed;
                Emulator5NameLabel2.Visibility = Visibility.Collapsed;
                Emulator5LocationTextBox.Visibility = Visibility.Collapsed;
                ChooseEmulator5LocationButton.Visibility = Visibility.Collapsed;
                Emulator5NameLabel3.Visibility = Visibility.Collapsed;
                Emulator5ParametersTextBox.Visibility = Visibility.Collapsed;
                Emulator5ParametersHelper.Visibility = Visibility.Collapsed;
            }
            else
            {
                ToggleEmulator5Button.Visibility = Visibility.Collapsed;
                Emulator5NameLabel.Visibility = Visibility.Visible;
                Emulator5NameTextBox.Visibility = Visibility.Visible;
                Emulator5NameLabel2.Visibility = Visibility.Visible;
                Emulator5LocationTextBox.Visibility = Visibility.Visible;
                ChooseEmulator5LocationButton.Visibility = Visibility.Visible;
                Emulator5NameLabel3.Visibility = Visibility.Visible;
                Emulator5ParametersTextBox.Visibility = Visibility.Visible;
                Emulator5ParametersHelper.Visibility = Visibility.Visible;
            }
        }

        private void EditSystem_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to save a backup copy of the current configuration?",
                "Alert",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                string appFolderPath = AppDomain.CurrentDomain.BaseDirectory;
                string sourceFilePath = Path.Combine(appFolderPath, "system.xml");
                string backupFileName = $"system_backup{DateTime.Now:yyyyMMdd_HHmmss}.xml";
                string backupFilePath = Path.Combine(appFolderPath, backupFileName);

                if (File.Exists(sourceFilePath))
                {
                    File.Copy(sourceFilePath, backupFilePath);
                    MessageBox.Show("The backup was created in the application folder", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("The system.xml file was not found in the application folder.\n\nWe could not backup it!\n\nTry to reinstall Simple Launcher to fix the issue.", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }

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
        
        //Placeholder Visibility
        private void AdjustPlaceholderVisibility()
        {
            SystemNamePlaceholderTextBox.Visibility = string.IsNullOrEmpty(SystemNameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            SystemFolderPlaceholderTextBox.Visibility = string.IsNullOrEmpty(SystemFolderTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            FormatToSearchPlaceholderTextBox.Visibility = string.IsNullOrEmpty(FormatToSearchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            FormatToLaunchPlaceholderTextBox.Visibility = string.IsNullOrEmpty(FormatToLaunchTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
            Emulator1NamePlaceholderTextBox.Visibility = string.IsNullOrEmpty(Emulator1NameTextBox.Text) ? Visibility.Visible : Visibility.Collapsed;
        }
        
        //SystemName Placeholder
        private void SystemNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SystemNamePlaceholderTextBox.Visibility = Visibility.Collapsed;
        }
        private void SystemNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SystemNameTextBox.Text))
            {
                SystemNamePlaceholderTextBox.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(SystemNameTextBox.Text))
            {
                SystemNamePlaceholderTextBox.Visibility = Visibility.Collapsed;
            }
        }

        //SystemFolder Placeholder
        private void SystemFolderTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SystemFolderPlaceholderTextBox.Visibility = Visibility.Collapsed;
        }
        private void SystemFolderTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SystemFolderTextBox.Text))
            {
                SystemFolderPlaceholderTextBox.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(SystemFolderTextBox.Text))
            {
                SystemFolderPlaceholderTextBox.Visibility = Visibility.Collapsed;
            }
        }

        //FormatToSearch Placeholder
        private void FormatToSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FormatToSearchPlaceholderTextBox.Visibility = Visibility.Collapsed;
        }
        private void FormatToSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FormatToSearchTextBox.Text))
            {
                FormatToSearchPlaceholderTextBox.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(FormatToSearchTextBox.Text))
            {
                FormatToSearchPlaceholderTextBox.Visibility = Visibility.Collapsed;
            }
        }
        
        //FormatToLaunch Placeholder
        private void FormatToLaunchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FormatToLaunchPlaceholderTextBox.Visibility = Visibility.Collapsed;
        }
        private void FormatToLaunchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FormatToLaunchTextBox.Text))
            {
                FormatToLaunchPlaceholderTextBox.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(FormatToLaunchTextBox.Text))
            {
                FormatToLaunchPlaceholderTextBox.Visibility = Visibility.Collapsed;
            }
        }
        
        //Emulator1Name Placeholder
        private void Emulator1NameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Emulator1NamePlaceholderTextBox.Visibility = Visibility.Collapsed;
        }
        private void Emulator1NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Emulator1NameTextBox.Text))
            {
                Emulator1NamePlaceholderTextBox.Visibility = Visibility.Visible;
            }
            else if (!string.IsNullOrEmpty(Emulator1NameTextBox.Text))
            {
                Emulator1NamePlaceholderTextBox.Visibility = Visibility.Collapsed;
            }
        }

        private void HelpLink_Click(object sender, RoutedEventArgs e)
        {
            PlayClick.PlayClickSound();
            string searchUrl = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters";
            Process.Start(new ProcessStartInfo
            {
                FileName = searchUrl,
                UseShellExecute = true
            });
        }

    }
}