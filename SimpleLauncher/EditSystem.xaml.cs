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
using System.Xml.XPath;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace SimpleLauncher;

public partial class EditSystem
{
    private XDocument _xmlDoc;
    private readonly string _xmlFilePath = "system.xml";
    private static readonly char[] SplitSeparators = [',', '|', ';'];
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
            }
        }
        
        // Create SystemFolder default
        if (SystemFolderTextBox.Text == $".\\roms\\{SystemNameTextBox.Text}" && !Directory.Exists(SystemFolderTextBox.Text))
        {
            Directory.CreateDirectory(SystemFolderTextBox.Text);
        }
        
        // Create SystemImageFolder default
        if (SystemImageFolderTextBox.Text == $".\\images\\{SystemNameTextBox.Text}" && !Directory.Exists(SystemImageFolderTextBox.Text))
        {
            Directory.CreateDirectory(SystemImageFolderTextBox.Text);
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
        
        // Update the HelpUserTextBox
        HelpUser.UpdateHelpUserRichTextBox(
            HelpUserRichTextBox,
            [
                Emulator1LocationTextBox,
                Emulator2LocationTextBox,
                Emulator3LocationTextBox,
                Emulator4LocationTextBox,
                Emulator5LocationTextBox
            ],
            [
                Emulator1NameTextBox,
                Emulator2NameTextBox,
                Emulator3NameTextBox,
                Emulator4NameTextBox,
                Emulator5NameTextBox
            ]);
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
            int doubleDotCount = path.Split([@"..\"], StringSplitOptions.None).Length - 1;
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
        
        // Update the HelpUserTextBox
        HelpUser.UpdateHelpUserRichTextBox(
            HelpUserRichTextBox,
            [
                Emulator1LocationTextBox,
                Emulator2LocationTextBox,
                Emulator3LocationTextBox,
                Emulator4LocationTextBox,
                Emulator5LocationTextBox
            ],
            [
                Emulator1NameTextBox,
                Emulator2NameTextBox,
                Emulator3NameTextBox,
                Emulator4NameTextBox,
                Emulator5NameTextBox
            ]);
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

        SaveSystemButton.IsEnabled = true;
        DeleteSystemButton.IsEnabled = false;
            
        MessageBox.Show("You can add a new system now.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }
        
    private void EnableFields()
    {
        SystemNameTextBox.IsReadOnly = false;
        SystemNameTextBox.IsEnabled = true;
        
        SystemFolderTextBox.IsReadOnly = false;
        SystemFolderTextBox.IsEnabled = true;
        
        SystemImageFolderTextBox.IsReadOnly = false;
        SystemImageFolderTextBox.IsEnabled = true;
            
        SystemIsMameComboBox.IsReadOnly = false;
        SystemIsMameComboBox.IsEnabled = true;
        
        FormatToSearchTextBox.IsReadOnly = false;
        FormatToSearchTextBox.IsEnabled = true;
            
        ExtractFileBeforeLaunchComboBox.IsReadOnly = false;
        ExtractFileBeforeLaunchComboBox.IsEnabled = true;
        
        FormatToLaunchTextBox.IsReadOnly = false;
        FormatToLaunchTextBox.IsEnabled = true;
        
        Emulator1NameTextBox.IsReadOnly = false;
        Emulator1NameTextBox.IsEnabled = true;

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
        // Trim input values
        TrimInputValues(out var systemNameText, out var systemFolderText, out var systemImageFolderText, out var formatToSearchText, out var formatToLaunchText, out var emulator1NameText, out var emulator2NameText, out var emulator3NameText, out var emulator4NameText, out var emulator5NameText, out var emulator1LocationText, out var emulator2LocationText, out var emulator3LocationText, out var emulator4LocationText, out var emulator5LocationText, out var emulator1ParametersText, out var emulator2ParametersText, out var emulator3ParametersText, out var emulator4ParametersText, out var emulator5ParametersText);

        // Validate paths
        ValidatePaths(systemNameText, systemFolderText, systemImageFolderText, emulator1LocationText, emulator2LocationText, emulator3LocationText, emulator4LocationText, emulator5LocationText, out var isSystemFolderValid, out var isSystemImageFolderValid, out var isEmulator1LocationValid, out var isEmulator2LocationValid, out var isEmulator3LocationValid, out var isEmulator4LocationValid, out var isEmulator5LocationValid);

        // Validate parameters
        ValidateParameters(emulator1ParametersText, emulator2ParametersText, emulator3ParametersText, emulator4ParametersText, emulator5ParametersText, out var isEmulator1ParametersValid, out var isEmulator2ParametersValid, out var isEmulator3ParametersValid, out var isEmulator4ParametersValid, out var isEmulator5ParametersValid);

        // Handle validation alerts
        HandleValidationAlerts(isSystemFolderValid, isSystemImageFolderValid, isEmulator1LocationValid, isEmulator2LocationValid, isEmulator3LocationValid, isEmulator4LocationValid, isEmulator5LocationValid, isEmulator1ParametersValid, isEmulator2ParametersValid, isEmulator3ParametersValid, isEmulator4ParametersValid, isEmulator5ParametersValid);

        // Validate SystemName
        if (ValidateSystemName(systemNameText)) return;
        
        // Validate SystemFolder
        if (ValidateSystemFolder(systemNameText, ref systemFolderText)) return;

        // Validate SystemImageFolder
        if (ValidateSystemImageFolder(systemNameText, ref systemImageFolderText)) return;

        // Validate systemIsMame
        // Set to false if user does not choose
        bool systemIsMame = SystemIsMameComboBox.SelectedItem != null && bool.Parse((SystemIsMameComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "false");
        
        // Validate extractFileBeforeLaunch
        // Set to false if user does not choose
        bool extractFileBeforeLaunch = ExtractFileBeforeLaunchComboBox.SelectedItem != null && bool.Parse((ExtractFileBeforeLaunchComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "false");
            
        // Validate FormatToSearch
        if (ValidateFormatToSearch(formatToSearchText, extractFileBeforeLaunch, out var formatsToSearch)) return;

        // Validate FormatToLaunch
        if (ValidateFormatToLaunch(formatToLaunchText, extractFileBeforeLaunch, out var formatsToLaunch)) return;

        // Validate Emulator1Name
        if (ValidateEmulator1Name(emulator1NameText)) return;
        
        // Check paths and paths from parameter
        if (CheckPaths(isSystemFolderValid, isSystemImageFolderValid, isEmulator1LocationValid, isEmulator2LocationValid, isEmulator3LocationValid, isEmulator4LocationValid,
                isEmulator5LocationValid, isEmulator1ParametersValid, isEmulator2ParametersValid, isEmulator3ParametersValid, isEmulator4ParametersValid, isEmulator5ParametersValid)) return;
            
        ////////////////
        // XML factory//
        ////////////////
        // Initialize 'emulatorsElement' as an XElement
        var emulatorsElement = new XElement("Emulators");
        
        // HashSet to store emulator names and ensure uniqueness
        var emulatorNames = new HashSet<string>();
        
        // Add Emulator1 details to XML and check uniqueness
        if (!emulatorNames.Add(emulator1NameText))
        {
            MessageBox.Show($"The name '{emulator1NameText}' is used multiple times. Each emulator name must be unique.",
                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        AddEmulatorToXml(emulatorsElement, emulator1NameText, emulator1LocationText, emulator1ParametersText);
        
        // Validate Emulators 2-5
        // Arrays for emulator names, locations, and parameters TextBoxes
        string[] nameText = [emulator2NameText, emulator3NameText, emulator4NameText, emulator5NameText];
        string[] locationText = [emulator2LocationText, emulator3LocationText, emulator4LocationText, emulator5LocationText];
        string[] parametersText = [emulator2ParametersText, emulator3ParametersText, emulator4ParametersText, emulator5ParametersText];

        // Loop over the emulators 2 through 5 to validate and add their details
        for (int i = 0; i < nameText.Length; i++)
        {
            var emulatorName = nameText[i];
            var emulatorLocation = locationText[i];
            var emulatorParameters = parametersText[i];

            // Check if any data related to the emulator is provided
            if (!string.IsNullOrEmpty(emulatorLocation) || !string.IsNullOrEmpty(emulatorParameters))
            {
                // Validate EmulatorName for Emulators 2-5
                // Make the emulator name required if related data is provided
                if (string.IsNullOrEmpty(emulatorName))
                {
                    MessageBox.Show($"Emulator {i + 2} name is required because related data has been provided.\n\n" +
                                    $"Please fix this field.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            // If the emulator name is provided, check for uniqueness and add the emulator details to XML
            if (!string.IsNullOrEmpty(emulatorName))
            {
                // Check for uniqueness
                if (!emulatorNames.Add(emulatorName))
                {
                    MessageBox.Show($"The name '{emulatorName}' is used multiple times. Each emulator name must be unique.",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                ////////////////
                // XML factory//
                ////////////////
                AddEmulatorToXml(emulatorsElement, emulatorName, emulatorLocation, emulatorParameters);
            }
        }
            
        ////////////////
        // XML factory//
        ////////////////
        _xmlDoc ??= new XDocument(new XElement("SystemConfigs"));
        var existingSystem = _xmlDoc.XPathSelectElement($"//SystemConfigs/SystemConfig[SystemName='{systemNameText}']");

        if (existingSystem != null)
        {
            UpdateXml(existingSystem, systemFolderText, systemImageFolderText, systemIsMame, formatsToSearch, extractFileBeforeLaunch, formatsToLaunch, emulatorsElement);
        }
        else
        {
            var newSystem = AddToXml(systemNameText, systemFolderText, systemImageFolderText, systemIsMame, formatsToSearch, extractFileBeforeLaunch, formatsToLaunch, emulatorsElement);
            _xmlDoc.Element("SystemConfigs")?.Add(newSystem);
        }

        // Sort the XML elements by "SystemName" before saving
        var sortedDoc = new XDocument(new XElement("SystemConfigs",
            from system in _xmlDoc.Descendants("SystemConfig")
            orderby system.Element("SystemName")?.Value
            select system));

        // Save
        sortedDoc.Save(_xmlFilePath);
            
        // Repopulate the SystemNamesDropbox
        PopulateSystemNamesDropdown();
        
        // Select a value from Dropbox
        SystemNameDropdown.SelectedItem = systemNameText;

        MessageBox.Show("System saved successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            
        CreateFolders(systemNameText);
        
        // Update the HelpUserTextBox
        HelpUser.UpdateHelpUserRichTextBox(
            HelpUserRichTextBox,
            [
                Emulator1LocationTextBox,
                Emulator2LocationTextBox,
                Emulator3LocationTextBox,
                Emulator4LocationTextBox,
                Emulator5LocationTextBox
            ],
            [
                Emulator1NameTextBox,
                Emulator2NameTextBox,
                Emulator3NameTextBox,
                Emulator4NameTextBox,
                Emulator5NameTextBox
            ]);

    }

    private void ValidateParameters(string emulator1ParametersText, string emulator2ParametersText,
        string emulator3ParametersText, string emulator4ParametersText, string emulator5ParametersText,
        out bool isEmulator1ParametersValid, out bool isEmulator2ParametersValid, out bool isEmulator3ParametersValid,
        out bool isEmulator4ParametersValid, out bool isEmulator5ParametersValid)
    {
        isEmulator1ParametersValid = string.IsNullOrWhiteSpace(emulator1ParametersText) || IsValidPath2(emulator1ParametersText);
        isEmulator2ParametersValid = string.IsNullOrWhiteSpace(emulator2ParametersText) || IsValidPath2(emulator2ParametersText);
        isEmulator3ParametersValid = string.IsNullOrWhiteSpace(emulator3ParametersText) || IsValidPath2(emulator3ParametersText);
        isEmulator4ParametersValid = string.IsNullOrWhiteSpace(emulator4ParametersText) || IsValidPath2(emulator4ParametersText);
        isEmulator5ParametersValid = string.IsNullOrWhiteSpace(emulator5ParametersText) || IsValidPath2(emulator5ParametersText);
    }

    private void ValidatePaths(string systemNameText, string systemFolderText, string systemImageFolderText, string emulator1LocationText,
        string emulator2LocationText, string emulator3LocationText, string emulator4LocationText,
        string emulator5LocationText, out bool isSystemFolderValid, out bool isSystemImageFolderValid,
        out bool isEmulator1LocationValid, out bool isEmulator2LocationValid, out bool isEmulator3LocationValid,
        out bool isEmulator4LocationValid, out bool isEmulator5LocationValid)
    {
        // Define the valid patterns
        var validSystemFolderPattern = $".\\roms\\{systemNameText}";
        var validSystemImageFolderPattern = $".\\images\\{systemNameText}";
        
        // Perform validation
        isSystemFolderValid = string.IsNullOrWhiteSpace(systemFolderText) || IsValidPath(systemFolderText) || systemFolderText == validSystemFolderPattern;
        isSystemImageFolderValid = string.IsNullOrWhiteSpace(systemImageFolderText) || IsValidPath(systemImageFolderText) || systemImageFolderText == validSystemImageFolderPattern;
        isEmulator1LocationValid = string.IsNullOrWhiteSpace(emulator1LocationText) || IsValidPath(emulator1LocationText);
        isEmulator2LocationValid = string.IsNullOrWhiteSpace(emulator2LocationText) || IsValidPath(emulator2LocationText);
        isEmulator3LocationValid = string.IsNullOrWhiteSpace(emulator3LocationText) || IsValidPath(emulator3LocationText);
        isEmulator4LocationValid = string.IsNullOrWhiteSpace(emulator4LocationText) || IsValidPath(emulator4LocationText);
        isEmulator5LocationValid = string.IsNullOrWhiteSpace(emulator5LocationText) || IsValidPath(emulator5LocationText);
    }

    private void TrimInputValues(out string systemNameText, out string systemFolderText, out string systemImageFolderText,
        out string formatToSearchText, out string formatToLaunchText, out string emulator1NameText,
        out string emulator2NameText, out string emulator3NameText, out string emulator4NameText,
        out string emulator5NameText, out string emulator1LocationText, out string emulator2LocationText,
        out string emulator3LocationText, out string emulator4LocationText, out string emulator5LocationText,
        out string emulator1ParametersText, out string emulator2ParametersText, out string emulator3ParametersText,
        out string emulator4ParametersText, out string emulator5ParametersText)
    {
        systemNameText = SystemNameTextBox.Text.Trim();
        systemFolderText = SystemFolderTextBox.Text.Trim();
        systemImageFolderText = SystemImageFolderTextBox.Text.Trim();
        formatToSearchText = FormatToSearchTextBox.Text.Trim();
        formatToLaunchText = FormatToLaunchTextBox.Text.Trim();
        emulator1NameText = Emulator1NameTextBox.Text.Trim();
        emulator2NameText = Emulator2NameTextBox.Text.Trim();
        emulator3NameText = Emulator3NameTextBox.Text.Trim();
        emulator4NameText = Emulator4NameTextBox.Text.Trim();
        emulator5NameText = Emulator5NameTextBox.Text.Trim();
        emulator1LocationText = Emulator1LocationTextBox.Text.Trim();
        emulator2LocationText = Emulator2LocationTextBox.Text.Trim();
        emulator3LocationText = Emulator3LocationTextBox.Text.Trim();
        emulator4LocationText = Emulator4LocationTextBox.Text.Trim();
        emulator5LocationText = Emulator5LocationTextBox.Text.Trim();
        emulator1ParametersText = Emulator1ParametersTextBox.Text.Trim();
        emulator2ParametersText = Emulator2ParametersTextBox.Text.Trim();
        emulator3ParametersText = Emulator3ParametersTextBox.Text.Trim();
        emulator4ParametersText = Emulator4ParametersTextBox.Text.Trim();
        emulator5ParametersText = Emulator5ParametersTextBox.Text.Trim();
    }

    private static XElement AddToXml(string systemNameText, string systemFolderText, string systemImageFolderText,
        bool systemIsMame, List<string> formatsToSearch, bool extractFileBeforeLaunch, List<string> formatsToLaunch,
        XElement emulatorsElement)
    {
        // Add a new system
        var newSystem = new XElement("SystemConfig",
            new XElement("SystemName", systemNameText),
            new XElement("SystemFolder", systemFolderText),
            new XElement("SystemImageFolder", systemImageFolderText),
            new XElement("SystemIsMAME", systemIsMame),
            new XElement("FileFormatsToSearch",
                formatsToSearch.Select(format => new XElement("FormatToSearch", format))),
            new XElement("ExtractFileBeforeLaunch", extractFileBeforeLaunch),
            new XElement("FileFormatsToLaunch",
                formatsToLaunch.Select(format => new XElement("FormatToLaunch", format))),
            emulatorsElement);
        return newSystem;
    }

    private static void UpdateXml(XElement existingSystem, string systemFolderText, string systemImageFolderText,
        bool systemIsMame, List<string> formatsToSearch, bool extractFileBeforeLaunch, List<string> formatsToLaunch,
        XElement emulatorsElement)
    {
        // Update existing system
        existingSystem.SetElementValue("SystemFolder", systemFolderText);
        existingSystem.SetElementValue("SystemImageFolder", systemImageFolderText);
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

    private static bool CheckPaths(bool isSystemFolderValid, bool isSystemImageFolderValid, bool isEmulator1LocationValid,
        bool isEmulator2LocationValid, bool isEmulator3LocationValid, bool isEmulator4LocationValid,
        bool isEmulator5LocationValid, bool isEmulator1ParametersValid, bool isEmulator2ParametersValid,
        bool isEmulator3ParametersValid, bool isEmulator4ParametersValid, bool isEmulator5ParametersValid)
    {
        if (!isSystemFolderValid || !isSystemImageFolderValid || !isEmulator1LocationValid || !isEmulator2LocationValid ||
            !isEmulator3LocationValid || !isEmulator4LocationValid || !isEmulator5LocationValid || !isEmulator1ParametersValid ||
            !isEmulator2ParametersValid || !isEmulator3ParametersValid || !isEmulator4ParametersValid || !isEmulator5ParametersValid)
        {
            MessageBox.Show("One or more paths or parameters are invalid.\n\n" +
                            "Please fix them to proceed.",
                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return true;
        }
        return false;
    }

    private static bool ValidateEmulator1Name(string emulator1NameText)
    {
        if (string.IsNullOrEmpty(emulator1NameText))
        {
            MessageBox.Show("'Emulator 1 Name' is required.\n\n" +
                            "Please fix this field.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        return false;
    }

    private static bool ValidateFormatToLaunch(string formatToLaunchText, bool extractFileBeforeLaunch,
        out List<string> formatsToLaunch)
    {
        formatsToLaunch = formatToLaunchText.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(format => format.Trim())
            .Where(format => !string.IsNullOrEmpty(format))
            .ToList();
        if (formatsToLaunch.Count == 0 && extractFileBeforeLaunch)
        {
            MessageBox.Show("'Extension to Launch After Extraction' is required when 'Extract File Before Launch' is set to true.\n\n" +
                            "Please fix this field.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }
        return false;
    }

    private static bool ValidateFormatToSearch(string formatToSearchText, bool extractFileBeforeLaunch,
        out List<string> formatsToSearch)
    {
        formatsToSearch = formatToSearchText.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(format => format.Trim())
            .Where(format => !string.IsNullOrEmpty(format))
            .ToList();
        if (formatsToSearch.Count == 0)
        {
            MessageBox.Show("'Extension to Search in the System Folder' cannot be empty or contain only spaces.\n\n" +
                            "Please fix this field.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return true;
        }
        if (extractFileBeforeLaunch && !formatsToSearch.All(f => f == "zip" || f == "7z" || f == "rar"))
        {
            MessageBox.Show("When 'Extract File Before Launch' is set to true, 'Extension to Search in the System Folder' must include 'zip', '7z', or 'rar'.\n\n" +
                            "It will not accept other extensions.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }
        return false;
    }

    private bool ValidateSystemImageFolder(string systemNameText, ref string systemImageFolderText)
    {
        // Add the default System Image Folder if not provided by user
        if (systemImageFolderText.Length == 0 || string.IsNullOrEmpty(systemImageFolderText))
        {
            systemImageFolderText = $".\\images\\{systemNameText}";
            SystemImageFolderTextBox.Text = systemImageFolderText;
        }
        
        // Create the directory if it doesn't exist
        if (!Directory.Exists(systemImageFolderText))
        {
            Directory.CreateDirectory(systemImageFolderText);
        }
        
        if (string.IsNullOrEmpty(systemImageFolderText))
        {
            MessageBox.Show("'System Image Folder' cannot be empty or contain only spaces.\n\n" +
                            "Please fix this field.",
                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }

        return false;
    }

    private bool ValidateSystemFolder(string systemNameText, ref string systemFolderText)
    {
        // Add the default System Folder if not provided by user
        if (systemFolderText.Length == 0 || string.IsNullOrEmpty(systemFolderText))
        {
            systemFolderText = $".\\roms\\{systemNameText}";
            SystemFolderTextBox.Text = systemFolderText;
        }

        // Create the directory if it doesn't exist
        if (!Directory.Exists(systemFolderText))
        {
            Directory.CreateDirectory(systemFolderText);
        }

        if (string.IsNullOrEmpty(systemFolderText))
        {
            MessageBox.Show("'System Folder' cannot be empty or contain only spaces.\n\n" +
                            "Please fix this field.", "Validation Error", MessageBoxButton.OK,
                MessageBoxImage.Information);
            return true;
        }
        return false;
    }

    private static bool ValidateSystemName(string systemNameText)
    {
        if (string.IsNullOrEmpty(systemNameText))
        {
            MessageBox.Show("'System Name' cannot be empty or contain only spaces.\n\n" +
                            "Please fix this field.",
                "Validation Error", MessageBoxButton.OK, MessageBoxImage.Information);
            return true;
        }
        return false;
    }

    private void HandleValidationAlerts(bool isSystemFolderValid, bool isSystemImageFolderValid,
        bool isEmulator1LocationValid, bool isEmulator2LocationValid, bool isEmulator3LocationValid,
        bool isEmulator4LocationValid, bool isEmulator5LocationValid, bool isEmulator1ParametersValid,
        bool isEmulator2ParametersValid, bool isEmulator3ParametersValid, bool isEmulator4ParametersValid,
        bool isEmulator5ParametersValid)
    {
        MarkInvalid(SystemFolderTextBox, isSystemFolderValid);
        MarkInvalid(SystemImageFolderTextBox, isSystemImageFolderValid);
        MarkInvalid(Emulator1LocationTextBox, isEmulator1LocationValid);
        MarkInvalid(Emulator2LocationTextBox, isEmulator2LocationValid);
        MarkInvalid(Emulator3LocationTextBox, isEmulator3LocationValid);
        MarkInvalid(Emulator4LocationTextBox, isEmulator4LocationValid);
        MarkInvalid(Emulator5LocationTextBox, isEmulator5LocationValid);
        MarkInvalid(Emulator1ParametersTextBox, isEmulator1ParametersValid);
        MarkInvalid(Emulator2ParametersTextBox, isEmulator2ParametersValid);
        MarkInvalid(Emulator3ParametersTextBox, isEmulator3ParametersValid);
        MarkInvalid(Emulator4ParametersTextBox, isEmulator4ParametersValid);
        MarkInvalid(Emulator5ParametersTextBox, isEmulator5ParametersValid);
    }

    private static void CreateFolders(string systemNameText)
    {
        string applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string[] folderNames = ["roms", "images", "title_snapshots", "gameplay_snapshots", "videos", "manuals", "walkthrough", "cabinets", "flyers", "pcbs", "carts"];

        foreach (var folderName in folderNames)
        {
            string parentDirectory = Path.Combine(applicationDirectory, folderName);
    
            // Ensure the parent directory exists
            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            // Use SystemName as the name for the new folder inside the parent directory
            string newFolderPath = Path.Combine(parentDirectory, systemNameText);

            try
            {
                // Check if the folder exists, and create it if it doesn't
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                    if (folderName == "images")
                    {
                        MessageBox.Show($"'Simple Launcher' created a image folder for this System at .\\images\\{systemNameText}.\n\n" +
                                        $"You may place the cover images for this System inside this folder.\n\n" +
                                        $"I also created folders for \"title_snapshots\", \"gameplay_snapshots\", \"videos\", \"manuals\", \"walkthrough\", \"cabinets\", \"flyers\", \"pcbs\" and \"carts\".", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                string formattedException = $"'Simple Launcher' failed to create the necessary folders for this system.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                    
                MessageBox.Show($"The application failed to create the necessary folders for this system.\n\n" +
                                $"Probably the application does not have enough privileges.\n\n" +
                                $"Try to run the application with administrative privileges.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
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
                
                MessageBox.Show($"System '{selectedSystemName}' has been deleted.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        else
        {
            MessageBox.Show("Selected system not found in the XML document!", "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
        
        // Clear existing content
        HelpUserRichTextBox.Document.Blocks.Clear();
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