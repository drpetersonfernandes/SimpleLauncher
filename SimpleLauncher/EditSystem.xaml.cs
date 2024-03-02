﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
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

        public EditSystem()
        {
            InitializeComponent();
            LoadXml();
            PopulateSystemNamesDropdown();
            this.Closing += EditSystem_Closing; // attach event handler
        }

        private void LoadXml()
        {
            if (!File.Exists(_xmlFilePath))
            {
                MessageBox.Show("system.xml not found inside the application folder.\nWe created one for you.");
                _xmlDoc = new XDocument();
                _xmlDoc.Add(new XElement("SystemConfigs"));
                _xmlDoc.Save(_xmlFilePath);
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
            if (SystemNameDropdown.SelectedItem == null) return;
            if (_xmlDoc == null) return;

            string selectedSystemName = SystemNameDropdown.SelectedItem.ToString()!;
            var selectedSystem = _xmlDoc.Descendants("SystemConfig")
                .FirstOrDefault(x => x.Element("SystemName")?.Value == selectedSystemName);

            if (selectedSystem != null)
            {
                SystemNameTextBox.Text = selectedSystem.Element("SystemName")?.Value ?? string.Empty;
                SystemFolderTextBox.Text = selectedSystem.Element("SystemFolder")?.Value ?? string.Empty;

                var systemIsMameValue = selectedSystem.Element("SystemIsMAME")?.Value == "true" ? "True" : "False";
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
                    ? "True"
                    : "False";
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
            }
        }

        private void AddSystemButton_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            AdjustPlaceholderVisibility();
            MessageBox.Show("You can add a new system now", "Info", MessageBoxButton.OK);
        }

        private void ClearFields()
        {
            SystemNameDropdown.SelectedItem = null;
            SystemNameTextBox.Text = string.Empty;
            SystemFolderTextBox.Text = string.Empty;
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
            var systemName = SystemNameTextBox.Text;
            var systemFolder = SystemFolderTextBox.Text;

            string systemIsMame = SystemIsMameComboBox.SelectedItem == null ? "false" : (SystemIsMameComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            string extractFileBeforeLaunch = ExtractFileBeforeLaunchComboBox.SelectedItem == null ? "false" : (ExtractFileBeforeLaunchComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            var formatToSearchInput = FormatToSearchTextBox.Text;
            var formatsToSearch = formatToSearchInput.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(format => format.Trim())
                .Where(format => !string.IsNullOrEmpty(format))
                .ToList(); // Convert to list to allow adding
            if(string.IsNullOrEmpty(formatToSearchInput))
                formatsToSearch.Add(string.Empty); // Add an empty string if the input was empty
            
            var formatToLaunchInput = FormatToLaunchTextBox.Text;
            var formatsToLaunch = formatToLaunchInput.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
                .Select(format => format.Trim())
                .Where(format => !string.IsNullOrEmpty(format))
                .ToList(); // Convert to list to allow adding
            if(string.IsNullOrEmpty(formatToLaunchInput))
                formatsToLaunch.Add(string.Empty); // Add an empty string if the input was empty
            
            // Emulator1 validation
            var emulator1Name = Emulator1NameTextBox.Text;
            bool hasValidEmulator = !string.IsNullOrEmpty(emulator1Name);
            if (!hasValidEmulator)
            {
                MessageBox.Show("The name for Emulator 1 is required.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Initialize 'emulatorsElement' as an XElement
            var emulatorsElement = new XElement("Emulators");

            // Add Emulator1 details to XML
            AddEmulatorToXml(emulatorsElement, Emulator1NameTextBox.Text, Emulator1LocationTextBox.Text, Emulator1ParametersTextBox.Text);

            // Arrays for emulator names, locations, and parameters TextBoxes
            TextBox[] nameTextBoxes = new[] { Emulator2NameTextBox, Emulator3NameTextBox, Emulator4NameTextBox, Emulator5NameTextBox };
            TextBox[] locationTextBoxes = new[] { Emulator2LocationTextBox, Emulator3LocationTextBox, Emulator4LocationTextBox, Emulator5LocationTextBox };
            TextBox[] parametersTextBoxes = new[] { Emulator2ParametersTextBox, Emulator3ParametersTextBox, Emulator4ParametersTextBox, Emulator5ParametersTextBox };

            // Loop over the emulators 2 through 5 to validate and add their details
            for (int i = 0; i < nameTextBoxes.Length; i++)
            {
                var emulatorName = nameTextBoxes[i].Text;
                var emulatorLocation = locationTextBoxes[i].Text;
                var emulatorParameters = parametersTextBoxes[i].Text;

                // Check if any data related to the emulator is provided
                if (!string.IsNullOrEmpty(emulatorLocation) || !string.IsNullOrEmpty(emulatorParameters))
                {
                    // Make the emulator name required if related data is provided
                    if (string.IsNullOrEmpty(emulatorName))
                    {
                        MessageBox.Show($"Emulator {i + 2} name is required because related data has been provided.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                        return; // Exit the method to prevent saving incomplete data
                    }
                }

                // If the emulator name is provided, add the emulator details to XML
                if (!string.IsNullOrEmpty(emulatorName))
                {
                    AddEmulatorToXml(emulatorsElement, emulatorName, emulatorLocation, emulatorParameters);
                }
            }
            
            
            _xmlDoc ??= new XDocument(new XElement("SystemConfigs"));
            var existingSystem = _xmlDoc.XPathSelectElement($"//SystemConfigs/SystemConfig[SystemName='{systemName}']");

            if (existingSystem != null)
            {
                // Update existing system
                existingSystem.SetElementValue("SystemFolder", systemFolder);
                existingSystem.SetElementValue("SystemIsMAME", systemIsMame);
                existingSystem.Element("FileFormatsToSearch")
                    ?.ReplaceNodes(formatsToSearch.Select(format => new XElement("FormatToSearch", format)));
                existingSystem.SetElementValue("ExtractFileBeforeLaunch", extractFileBeforeLaunch);
                existingSystem.Element("FileFormatsToLaunch")
                    ?.ReplaceNodes(formatsToLaunch.Select(format => new XElement("FormatToLaunch", format)));
                existingSystem.Element("Emulators")
                    ?.Remove(); // Remove existing emulators section before adding updated one
                existingSystem.Add(emulatorsElement);
            }
            else if (string.IsNullOrEmpty(systemName))
            {
                MessageBox.Show("The System Name is empty.\nPlease fill this field.", "Alert", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            else if (string.IsNullOrEmpty(systemFolder))
            {
                MessageBox.Show("The System Folder is empty.\nPlease fill this field.", "Alert", MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            else if (!formatsToSearch.Any())
            {
                MessageBox.Show("The File Formats To Search is empty.\nPlease fill this field.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else if (string.IsNullOrEmpty(formatToLaunchInput) && extractFileBeforeLaunch == "true")
            {
                MessageBox.Show("The File Formats To Launch is required when Extract File Before Launch is set to true.\nPlease fill this field.", "Alert", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                // Add new system
                var newSystem = new XElement("SystemConfig",
                    new XElement("SystemName", systemName),
                    new XElement("SystemFolder", systemFolder),
                    new XElement("SystemIsMAME", systemIsMame),
                    new XElement("FileFormatsToSearch",
                        formatsToSearch.Select(format => new XElement("FormatToSearch", format))),
                    new XElement("ExtractFileBeforeLaunch", extractFileBeforeLaunch),
                    new XElement("FileFormatsToLaunch",
                        formatsToLaunch.Select(format => new XElement("FormatToLaunch", format))),
                    emulatorsElement);
                _xmlDoc.Element("SystemConfigs")?.Add(newSystem);
            }

            // Sort the XML elements by "SystemName" before saving
            var sortedDoc = new XDocument(new XElement("SystemConfigs",
                from system in _xmlDoc.Descendants("SystemConfig")
                orderby system.Element("SystemName")?.Value
                select system));

            sortedDoc.Save(_xmlFilePath);
            PopulateSystemNamesDropdown();
            SystemNameDropdown.SelectedItem = systemName;

            MessageBox.Show("System saved successfully.", "Info", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private static void AddEmulatorToXml(XElement emulatorsElement, string name, string location, string parameters)
        {
            if (!string.IsNullOrEmpty(name)) // Check if the emulator name is not empty
            {
                var emulatorElement = new XElement("Emulator",
                    new XElement("EmulatorName", name),
                    new XElement("EmulatorLocation", location),
                    new XElement("EmulatorParameters", parameters));
                emulatorsElement.Add(emulatorElement);
            }
        }

        private void DeleteSystemButton_Click(object sender, RoutedEventArgs e)
        {
            if (SystemNameDropdown.SelectedItem == null)
            {
                MessageBox.Show("Please select a system to delete.", "Alert", MessageBoxButton.OK);
                return;
            }

            string selectedSystemName = SystemNameDropdown.SelectedItem.ToString()!;

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
                    MessageBox.Show($"System '{selectedSystemName}' has been deleted.", "Info", MessageBoxButton.OK);
                }
            }
            else
            {
                MessageBox.Show("Selected system not found in the XML document!", "Alert", MessageBoxButton.OK);
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
                Emulator3LocationButton.Visibility = Visibility.Collapsed;
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
                Emulator3LocationButton.Visibility = Visibility.Visible;
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
                Emulator4LocationButton.Visibility = Visibility.Collapsed;
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
                Emulator4LocationButton.Visibility = Visibility.Visible;
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
                Emulator5LocationButton.Visibility = Visibility.Collapsed;
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
                Emulator5LocationButton.Visibility = Visibility.Visible;
                Emulator5NameLabel3.Visibility = Visibility.Visible;
                Emulator5ParametersTextBox.Visibility = Visibility.Visible;
                Emulator5ParametersHelper.Visibility = Visibility.Visible;
            }
        }

        private void EditSystem_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
            }

            // Shutdown the current application instance
            Application.Current.Shutdown();
            Environment.Exit(0);
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
            string searchUrl = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki";
            Process.Start(new ProcessStartInfo
            {
                FileName = searchUrl,
                UseShellExecute = true
            });
        }

    }
}