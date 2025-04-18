﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Win32;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace SimpleLauncher;

public partial class EditSystemWindow
{
    private XDocument _xmlDoc;
    private const string XmlFilePath = "system.xml";
    private static readonly char[] SplitSeparators = [',', '|', ';'];
    private readonly SettingsManager _settings;
    private string _originalSystemName;

    public EditSystemWindow(SettingsManager settings)
    {
        InitializeComponent();

        // Load Settings
        _settings = settings;
        LoadXml();

        PopulateSystemNamesDropdown();
        App.ApplyThemeToWindow(this);
        Closing += EditSystem_Closing;

        SaveSystemButton.IsEnabled = false;
        DeleteSystemButton.IsEnabled = false;
    }

    private void LoadXml()
    {
        if (!File.Exists(XmlFilePath))
        {
            // Notify user
            MessageBoxLibrary.SystemXmlNotFoundMessageBox();

            // Shutdown SimpleLauncher
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
        else
        {
            _xmlDoc = XDocument.Load(XmlFilePath);
        }
    }

    private void PopulateSystemNamesDropdown()
    {
        if (_xmlDoc == null) return;

        SystemNameDropdown.ItemsSource = _xmlDoc.Descendants("SystemConfig")
            .Select(static element => element.Element("SystemName")?.Value)
            .OrderBy(static name => name)
            .ToList();
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ReceiveANotificationOnEmulatorError1.SelectedItem = null;
        ReceiveANotificationOnEmulatorError2.SelectedItem = null;
        ReceiveANotificationOnEmulatorError3.SelectedItem = null;
        ReceiveANotificationOnEmulatorError4.SelectedItem = null;
        ReceiveANotificationOnEmulatorError5.SelectedItem = null;

        // Store the original system name for later use
        _originalSystemName = SystemNameDropdown.SelectedItem?.ToString();

        EnableFields();
        SaveSystemButton.IsEnabled = true;
        DeleteSystemButton.IsEnabled = true;

        if (SystemNameDropdown.SelectedItem == null) return;
        if (_xmlDoc == null) return;

        var selectedSystemName = SystemNameDropdown.SelectedItem.ToString();
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
                .Select(static x => x.Value)
                .ToArray();
            FormatToSearchTextBox.Text = formatToSearchValues != null
                ? string.Join(", ", formatToSearchValues)
                : string.Empty;

            var extractFileBeforeLaunchValue = selectedSystem.Element("ExtractFileBeforeLaunch")?.Value == "true"
                ? "true"
                : "false";
            ExtractFileBeforeLaunchComboBox.SelectedItem = ExtractFileBeforeLaunchComboBox.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == extractFileBeforeLaunchValue);

            // Handle multiple FormatToLaunch values
            var formatToLaunchValues = selectedSystem.Element("FileFormatsToLaunch")?.Elements("FormatToLaunch")
                .Select(static x => x.Value)
                .ToArray();
            FormatToLaunchTextBox.Text = formatToLaunchValues != null
                ? string.Join(", ", formatToLaunchValues)
                : string.Empty;

            var emulators = selectedSystem.Element("Emulators")?.Elements("Emulator").ToList();
            if (emulators != null)
            {
                var emulator1 = emulators.ElementAtOrDefault(0);
                if (emulator1 != null)
                {
                    Emulator1NameTextBox.Text = emulator1.Element("EmulatorName")?.Value ?? string.Empty;
                    Emulator1LocationTextBox.Text = emulator1.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator1ParametersTextBox.Text = emulator1.Element("EmulatorParameters")?.Value ?? string.Empty;

                    // Get the notification value, default to "false" if not found
                    var receiveNotificationValue = emulator1.Element("ReceiveANotificationOnEmulatorError")?.Value == "true"
                        ? "true"
                        : "false";
                    ReceiveANotificationOnEmulatorError1.SelectedItem = ReceiveANotificationOnEmulatorError1.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator1NameTextBox.Clear();
                    Emulator1LocationTextBox.Clear();
                    Emulator1ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError1.SelectedItem = -1;
                }

                var emulator2 = emulators.ElementAtOrDefault(1);
                if (emulator2 != null)
                {
                    Emulator2NameTextBox.Text = emulator2.Element("EmulatorName")?.Value ?? string.Empty;
                    Emulator2LocationTextBox.Text = emulator2.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator2ParametersTextBox.Text = emulator2.Element("EmulatorParameters")?.Value ?? string.Empty;

                    var receiveNotificationValue = emulator2.Element("ReceiveANotificationOnEmulatorError")?.Value == "true"
                        ? "true"
                        : "false";
                    ReceiveANotificationOnEmulatorError2.SelectedItem = ReceiveANotificationOnEmulatorError2.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator2NameTextBox.Clear();
                    Emulator2LocationTextBox.Clear();
                    Emulator2ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError2.SelectedItem = -1;
                }

                var emulator3 = emulators.ElementAtOrDefault(2);
                if (emulator3 != null)
                {
                    Emulator3NameTextBox.Text = emulator3.Element("EmulatorName")?.Value ?? string.Empty;
                    Emulator3LocationTextBox.Text = emulator3.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator3ParametersTextBox.Text = emulator3.Element("EmulatorParameters")?.Value ?? string.Empty;

                    var receiveNotificationValue = emulator3.Element("ReceiveANotificationOnEmulatorError")?.Value == "true"
                        ? "true"
                        : "false";
                    ReceiveANotificationOnEmulatorError3.SelectedItem = ReceiveANotificationOnEmulatorError3.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator3NameTextBox.Clear();
                    Emulator3LocationTextBox.Clear();
                    Emulator3ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError3.SelectedItem = -1;
                }

                var emulator4 = emulators.ElementAtOrDefault(3);
                if (emulator4 != null)
                {
                    Emulator4NameTextBox.Text = emulator4.Element("EmulatorName")?.Value ?? string.Empty;
                    Emulator4LocationTextBox.Text = emulator4.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator4ParametersTextBox.Text = emulator4.Element("EmulatorParameters")?.Value ?? string.Empty;

                    var receiveNotificationValue = emulator4.Element("ReceiveANotificationOnEmulatorError")?.Value == "true"
                        ? "true"
                        : "false";
                    ReceiveANotificationOnEmulatorError4.SelectedItem = ReceiveANotificationOnEmulatorError4.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator4NameTextBox.Clear();
                    Emulator4LocationTextBox.Clear();
                    Emulator4ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError4.SelectedItem = -1;
                }

                var emulator5 = emulators.ElementAtOrDefault(4);
                if (emulator5 != null)
                {
                    Emulator5NameTextBox.Text = emulator5.Element("EmulatorName")?.Value ?? string.Empty;
                    Emulator5LocationTextBox.Text = emulator5.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator5ParametersTextBox.Text = emulator5.Element("EmulatorParameters")?.Value ?? string.Empty;

                    var receiveNotificationValue = emulator5.Element("ReceiveANotificationOnEmulatorError")?.Value == "true"
                        ? "true"
                        : "false";
                    ReceiveANotificationOnEmulatorError5.SelectedItem = ReceiveANotificationOnEmulatorError5.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator5NameTextBox.Clear();
                    Emulator5LocationTextBox.Clear();
                    Emulator5ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError5.SelectedItem = -1;
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
        MarkInvalid(SystemFolderTextBox, ParameterValidator.IsValidPath(SystemFolderTextBox.Text));
        MarkInvalid(SystemImageFolderTextBox, ParameterValidator.IsValidPath(SystemImageFolderTextBox.Text));

        // Validate Emulator Location Text Boxes (considered valid if empty)
        MarkInvalid(Emulator1LocationTextBox, string.IsNullOrWhiteSpace(Emulator1LocationTextBox.Text) || ParameterValidator.IsValidPath(Emulator1LocationTextBox.Text));
        MarkInvalid(Emulator2LocationTextBox, string.IsNullOrWhiteSpace(Emulator2LocationTextBox.Text) || ParameterValidator.IsValidPath(Emulator2LocationTextBox.Text));
        MarkInvalid(Emulator3LocationTextBox, string.IsNullOrWhiteSpace(Emulator3LocationTextBox.Text) || ParameterValidator.IsValidPath(Emulator3LocationTextBox.Text));
        MarkInvalid(Emulator4LocationTextBox, string.IsNullOrWhiteSpace(Emulator4LocationTextBox.Text) || ParameterValidator.IsValidPath(Emulator4LocationTextBox.Text));
        MarkInvalid(Emulator5LocationTextBox, string.IsNullOrWhiteSpace(Emulator5LocationTextBox.Text) || ParameterValidator.IsValidPath(Emulator5LocationTextBox.Text));

        // Validate Parameter fields
        ValidateParameterFields();

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Text = string.Empty;
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);

        // Update ReceiveANotificationOnEmulatorError
        if (string.IsNullOrEmpty(Emulator1NameTextBox.Text) ||
            string.IsNullOrEmpty(Emulator1LocationTextBox.Text) ||
            string.IsNullOrEmpty(Emulator1ParametersTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError1.SelectedItem = null;
        }

        if (string.IsNullOrEmpty(Emulator2NameTextBox.Text) ||
            string.IsNullOrEmpty(Emulator2LocationTextBox.Text) ||
            string.IsNullOrEmpty(Emulator2ParametersTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError2.SelectedItem = null;
        }

        if (string.IsNullOrEmpty(Emulator3NameTextBox.Text) ||
            string.IsNullOrEmpty(Emulator3LocationTextBox.Text) ||
            string.IsNullOrEmpty(Emulator3ParametersTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError3.SelectedItem = null;
        }

        if (string.IsNullOrEmpty(Emulator4NameTextBox.Text) ||
            string.IsNullOrEmpty(Emulator4LocationTextBox.Text) ||
            string.IsNullOrEmpty(Emulator4ParametersTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError4.SelectedItem = null;
        }

        if (string.IsNullOrEmpty(Emulator5NameTextBox.Text) ||
            string.IsNullOrEmpty(Emulator5LocationTextBox.Text) ||
            string.IsNullOrEmpty(Emulator5ParametersTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError5.SelectedItem = null;
        }
    }

    private void MarkInvalid(TextBox textBox, bool isValid)
    {
        if (isValid)
        {
            SetTextBoxForeground(textBox, true); // Valid state
        }
        else
        {
            textBox.Foreground = Brushes.Red; // Invalid state
        }
    }

    private void MarkValid(TextBox textBox)
    {
        SetTextBoxForeground(textBox, true); // Always valid state
    }

    private void SetTextBoxForeground(TextBox textBox, bool isValid)
    {
        var baseTheme = _settings.BaseTheme;
        if (baseTheme == "Dark")
        {
            textBox.Foreground = isValid ? Brushes.White : Brushes.Red;
        }
        else
        {
            textBox.Foreground = isValid ? Brushes.Black : Brushes.Red;
        }
    }

    private void ChooseSystemFolder(object sender, RoutedEventArgs e)
    {
        var pleaseselecttheSystemFolder2 = (string)Application.Current.TryFindResource("PleaseselecttheSystemFolder") ?? "Please select the System Folder";

        // Create new OpenFolderDialog
        var openFolderDialog = new OpenFolderDialog
        {
            Title = pleaseselecttheSystemFolder2
        };

        // Show dialog and handle result
        if (openFolderDialog.ShowDialog() != true) return;

        var foldername = openFolderDialog.FolderName;
        SystemFolderTextBox.Text = foldername;

        MarkValid(SystemFolderTextBox);
    }

    private void ChooseSystemImageFolder(object sender, RoutedEventArgs e)
    {
        var pleaseselecttheSystemImage2 = (string)Application.Current.TryFindResource("PleaseselecttheSystemImage") ?? "Please select the System Image Folder";

        // Create new OpenFolderDialog
        var openFolderDialog = new OpenFolderDialog
        {
            Title = pleaseselecttheSystemImage2
        };

        // Show dialog and handle result
        var result = openFolderDialog.ShowDialog();

        if (result != true) return;

        var foldername = openFolderDialog.FolderName.Trim();
        SystemImageFolderTextBox.Text = foldername;
        MarkValid(SystemImageFolderTextBox);
    }

    private void ChooseEmulator1Location(object sender, RoutedEventArgs e)
    {
        var selectEmulator12 = (string)Application.Current.TryFindResource("SelectEmulator1") ?? "Select Emulator 1";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "Exe File (.exe)|*.exe",
            Title = selectEmulator12
        };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator1LocationTextBox.Text = filename;
            MarkValid(Emulator1LocationTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Text = string.Empty;
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void ChooseEmulator2Location(object sender, RoutedEventArgs e)
    {
        var selectEmulator22 = (string)Application.Current.TryFindResource("SelectEmulator2") ?? "Select Emulator 2";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "EXE File (.exe)|*.exe",
            Title = selectEmulator22
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator2LocationTextBox.Text = filename;
            MarkValid(Emulator2LocationTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Text = string.Empty;
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void ChooseEmulator3Location(object sender, RoutedEventArgs e)
    {
        var selectEmulator32 = (string)Application.Current.TryFindResource("SelectEmulator3") ?? "Select Emulator 3";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "EXE File (.exe)|*.exe",
            Title = selectEmulator32
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator3LocationTextBox.Text = filename;
            MarkValid(Emulator3LocationTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Text = string.Empty;
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void ChooseEmulator4Location(object sender, RoutedEventArgs e)
    {
        var selectEmulator42 = (string)Application.Current.TryFindResource("SelectEmulator4") ?? "Select Emulator 4";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "EXE File (.exe)|*.exe",
            Title = selectEmulator42
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator4LocationTextBox.Text = filename;
            MarkValid(Emulator4LocationTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Text = string.Empty;
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void ChooseEmulator5Location(object sender, RoutedEventArgs e)
    {
        var selectEmulator52 = (string)Application.Current.TryFindResource("SelectEmulator5") ?? "Select Emulator 5";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "EXE File (.exe)|*.exe",
            Title = selectEmulator52
        };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator5LocationTextBox.Text = filename;
            MarkValid(Emulator5LocationTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Text = string.Empty;
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void AddSystemButton_Click(object sender, RoutedEventArgs e)
    {
        _originalSystemName = null;

        EnableFields();
        ClearFields();
        HelpUserTextBlock.Text = string.Empty;

        SaveSystemButton.IsEnabled = true;
        DeleteSystemButton.IsEnabled = false;

        // Notify user
        MessageBoxLibrary.YouCanAddANewSystemMessageBox();
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

        ReceiveANotificationOnEmulatorError1.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError1.IsEnabled = true;

        Emulator2NameTextBox.IsReadOnly = false;
        Emulator2NameTextBox.IsEnabled = true;

        Emulator2LocationTextBox.IsReadOnly = false;
        Emulator2LocationTextBox.IsEnabled = true;

        Emulator2ParametersTextBox.IsReadOnly = false;
        Emulator2ParametersTextBox.IsEnabled = true;

        ReceiveANotificationOnEmulatorError2.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError2.IsEnabled = true;

        Emulator3NameTextBox.IsReadOnly = false;
        Emulator3NameTextBox.IsEnabled = true;

        Emulator3LocationTextBox.IsReadOnly = false;
        Emulator3LocationTextBox.IsEnabled = true;

        Emulator3ParametersTextBox.IsReadOnly = false;
        Emulator3ParametersTextBox.IsEnabled = true;

        ReceiveANotificationOnEmulatorError3.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError3.IsEnabled = true;

        Emulator4NameTextBox.IsReadOnly = false;
        Emulator4NameTextBox.IsEnabled = true;

        Emulator4LocationTextBox.IsReadOnly = false;
        Emulator4LocationTextBox.IsEnabled = true;

        Emulator4ParametersTextBox.IsReadOnly = false;
        Emulator4ParametersTextBox.IsEnabled = true;

        ReceiveANotificationOnEmulatorError4.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError4.IsEnabled = true;

        Emulator5NameTextBox.IsReadOnly = false;
        Emulator5NameTextBox.IsEnabled = true;

        Emulator5LocationTextBox.IsReadOnly = false;
        Emulator5LocationTextBox.IsEnabled = true;

        Emulator5ParametersTextBox.IsReadOnly = false;
        Emulator5ParametersTextBox.IsEnabled = true;

        ReceiveANotificationOnEmulatorError5.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError5.IsEnabled = true;

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
        ReceiveANotificationOnEmulatorError1.SelectedItem = null;
        Emulator2NameTextBox.Text = string.Empty;
        Emulator2LocationTextBox.Text = string.Empty;
        Emulator2ParametersTextBox.Text = string.Empty;
        ReceiveANotificationOnEmulatorError2.SelectedItem = null;
        Emulator3NameTextBox.Text = string.Empty;
        Emulator3LocationTextBox.Text = string.Empty;
        Emulator3ParametersTextBox.Text = string.Empty;
        ReceiveANotificationOnEmulatorError3.SelectedItem = null;
        Emulator4NameTextBox.Text = string.Empty;
        Emulator4LocationTextBox.Text = string.Empty;
        Emulator4ParametersTextBox.Text = string.Empty;
        ReceiveANotificationOnEmulatorError4.SelectedItem = null;
        Emulator5NameTextBox.Text = string.Empty;
        Emulator5LocationTextBox.Text = string.Empty;
        Emulator5ParametersTextBox.Text = string.Empty;
        ReceiveANotificationOnEmulatorError5.SelectedItem = null;
    }

    private void SaveSystemButton_Click(object sender, RoutedEventArgs e)
    {
        // Trim input values
        TrimInputValues(out var systemNameText, out var systemFolderText, out var systemImageFolderText, out var formatToSearchText, out var formatToLaunchText, out var emulator1NameText, out var emulator2NameText, out var emulator3NameText, out var emulator4NameText, out var emulator5NameText, out var emulator1LocationText, out var emulator2LocationText, out var emulator3LocationText, out var emulator4LocationText, out var emulator5LocationText, out var emulator1ParametersText, out var emulator2ParametersText, out var emulator3ParametersText, out var emulator4ParametersText, out var emulator5ParametersText);

        // Validate paths
        ValidatePaths(systemNameText, systemFolderText, systemImageFolderText, emulator1LocationText, emulator2LocationText, emulator3LocationText, emulator4LocationText, emulator5LocationText, out var isSystemFolderValid, out var isSystemImageFolderValid, out var isEmulator1LocationValid, out var isEmulator2LocationValid, out var isEmulator3LocationValid, out var isEmulator4LocationValid, out var isEmulator5LocationValid);

        // Handle validation alerts
        HandleValidationAlerts(isSystemFolderValid, isSystemImageFolderValid, isEmulator1LocationValid, isEmulator2LocationValid, isEmulator3LocationValid, isEmulator4LocationValid, isEmulator5LocationValid);

        // Validate SystemName
        if (ValidateSystemName(systemNameText)) return;

        // Validate SystemFolder
        if (ValidateSystemFolder(systemNameText, ref systemFolderText)) return;

        // Validate SystemImageFolder
        if (ValidateSystemImageFolder(systemNameText, ref systemImageFolderText)) return;

        // Validate systemIsMame
        // Set to false if user does not choose
        var systemIsMame = SystemIsMameComboBox.SelectedItem != null && bool.Parse((SystemIsMameComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "false");

        // Validate extractFileBeforeLaunch
        // Set to false if user does not choose
        var extractFileBeforeLaunch = ExtractFileBeforeLaunchComboBox.SelectedItem != null && bool.Parse((ExtractFileBeforeLaunchComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "false");

        // Validate FormatToSearch
        if (ValidateFormatToSearch(formatToSearchText, extractFileBeforeLaunch, out var formatsToSearch)) return;

        // Validate FormatToLaunch
        if (ValidateFormatToLaunch(formatToLaunchText, extractFileBeforeLaunch, out var formatsToLaunch)) return;

        // Validate Emulator1Name
        if (ValidateEmulator1Name(emulator1NameText)) return;

        // Check paths
        if (CheckPaths(isSystemFolderValid, isSystemImageFolderValid, isEmulator1LocationValid, isEmulator2LocationValid, isEmulator3LocationValid, isEmulator4LocationValid,
                isEmulator5LocationValid)) return;

        // Check parameter paths
        string[] parameterTexts =
        [
            emulator1ParametersText, emulator2ParametersText, emulator3ParametersText, emulator4ParametersText, emulator5ParametersText
        ];
        ValidateAndWarnAboutParameters(parameterTexts);

        // Get the notification settings, defaulting to false if not selected or null
        var receiveNotification1 = ReceiveANotificationOnEmulatorError1.SelectedItem is ComboBoxItem { Content: not null } item1 &&
                                   item1.Content.ToString() == "true"; // Default to false

        var receiveNotification2 = ReceiveANotificationOnEmulatorError2.SelectedItem is ComboBoxItem { Content: not null } item2 &&
                                   item2.Content.ToString() == "true"; // Default to false

        var receiveNotification3 = ReceiveANotificationOnEmulatorError3.SelectedItem is ComboBoxItem { Content: not null } item3 &&
                                   item3.Content.ToString() == "true"; // Default to false

        var receiveNotification4 = ReceiveANotificationOnEmulatorError4.SelectedItem is ComboBoxItem { Content: not null } item4 &&
                                   item4.Content.ToString() == "true"; // Default to false

        var receiveNotification5 = ReceiveANotificationOnEmulatorError5.SelectedItem is ComboBoxItem { Content: not null } item5 &&
                                   item5.Content.ToString() == "true"; // Default to false

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
            // Notify user
            MessageBoxLibrary.EmulatorNameMustBeUniqueMessageBox(emulator1NameText);

            return;
        }

        AddEmulatorToXml(emulatorsElement, emulator1NameText, emulator1LocationText, emulator1ParametersText, receiveNotification1);

        // Validate Emulators 2-5
        // Arrays for emulator names, locations, and parameters TextBoxes
        string[] nameText = [emulator2NameText, emulator3NameText, emulator4NameText, emulator5NameText];
        string[] locationText = [emulator2LocationText, emulator3LocationText, emulator4LocationText, emulator5LocationText];
        string[] parametersText = [emulator2ParametersText, emulator3ParametersText, emulator4ParametersText, emulator5ParametersText];
        bool[] receiveNotifications = [receiveNotification2, receiveNotification3, receiveNotification4, receiveNotification5];

        // Loop over the emulators 2 through 5 to validate and add their details
        for (var i = 0; i < nameText.Length; i++)
        {
            var emulatorName = nameText[i];
            var emulatorLocation = locationText[i];
            var emulatorParameters = parametersText[i];
            var receiveNotification = receiveNotifications[i];

            // Check if any data related to the emulator is provided
            if (!string.IsNullOrEmpty(emulatorLocation) || !string.IsNullOrEmpty(emulatorParameters))
            {
                // Validate EmulatorName for Emulators 2-5
                // Make the emulator name required if related data is provided
                if (string.IsNullOrEmpty(emulatorName))
                {
                    // Notify user
                    MessageBoxLibrary.EmulatorNameRequiredMessageBox(i);

                    return;
                }
            }

            // If the emulator name is provided, check for uniqueness and add the emulator details to XML
            if (string.IsNullOrEmpty(emulatorName)) continue;

            // Check for uniqueness
            if (!emulatorNames.Add(emulatorName))
            {
                // Notify user
                MessageBoxLibrary.EmulatorNameMustBeUniqueMessageBox2(emulatorName);

                return;
            }

            ////////////////
            // XML factory//
            ////////////////
            AddEmulatorToXml(emulatorsElement, emulatorName, emulatorLocation, emulatorParameters, receiveNotification);
        }

        // Check if we're updating an existing system
        var isUpdate = !string.IsNullOrEmpty(_originalSystemName) && SystemNameDropdown.SelectedItem != null;

        ////////////////
        // XML factory//
        ////////////////
        _xmlDoc ??= new XDocument(new XElement("SystemConfigs"));

        // Use the original name to find the system if we're updating, otherwise use the current name
        var systemIdentifier = isUpdate ? _originalSystemName : systemNameText;
        var existingSystem = _xmlDoc.XPathSelectElement($"//SystemConfigs/SystemConfig[SystemName='{systemIdentifier}']");

        if (existingSystem != null)
        {
            // If we're changing the name, update that first
            if (systemIdentifier != systemNameText)
            {
                existingSystem.Element("SystemName").Value = systemNameText;
            }

            // Then update all other fields
            UpdateXml(existingSystem, systemFolderText, systemImageFolderText, systemIsMame, formatsToSearch, extractFileBeforeLaunch, formatsToLaunch, emulatorsElement);
        }
        else
        {
            // Create a new system
            var newSystem = AddToXml(systemNameText, systemFolderText, systemImageFolderText, systemIsMame, formatsToSearch, extractFileBeforeLaunch, formatsToLaunch, emulatorsElement);
            _xmlDoc.Element("SystemConfigs")?.Add(newSystem);
        }

        // Sort the XML elements by "SystemName" before saving
        var sortedDoc = new XDocument(new XElement("SystemConfigs",
            from system in _xmlDoc.Descendants("SystemConfig")
            orderby system.Element("SystemName")?.Value
            select system));

        // Save
        sortedDoc.Save(XmlFilePath);

        // Repopulate the SystemNamesDropbox
        PopulateSystemNamesDropdown();

        // Select a value from Dropbox
        SystemNameDropdown.SelectedItem = systemNameText;

        // Notify user
        MessageBoxLibrary.SystemSavedSuccessfullyMessageBox();

        CreateFolders(systemNameText);

        // Update the original system name to match the current name after save
        _originalSystemName = systemNameText;
    }

    private static void ValidatePaths(string systemNameText, string systemFolderText, string systemImageFolderText, string emulator1LocationText,
        string emulator2LocationText, string emulator3LocationText, string emulator4LocationText,
        string emulator5LocationText, out bool isSystemFolderValid, out bool isSystemImageFolderValid,
        out bool isEmulator1LocationValid, out bool isEmulator2LocationValid, out bool isEmulator3LocationValid,
        out bool isEmulator4LocationValid, out bool isEmulator5LocationValid)
    {
        // Define the valid patterns
        var validSystemFolderPattern = $".\\roms\\{systemNameText}";
        var validSystemImageFolderPattern = $".\\images\\{systemNameText}";

        // Perform validation
        isSystemFolderValid = string.IsNullOrWhiteSpace(systemFolderText) || ParameterValidator.IsValidPath(systemFolderText) || systemFolderText == validSystemFolderPattern;
        isSystemImageFolderValid = string.IsNullOrWhiteSpace(systemImageFolderText) || ParameterValidator.IsValidPath(systemImageFolderText) || systemImageFolderText == validSystemImageFolderPattern;
        isEmulator1LocationValid = string.IsNullOrWhiteSpace(emulator1LocationText) || ParameterValidator.IsValidPath(emulator1LocationText);
        isEmulator2LocationValid = string.IsNullOrWhiteSpace(emulator2LocationText) || ParameterValidator.IsValidPath(emulator2LocationText);
        isEmulator3LocationValid = string.IsNullOrWhiteSpace(emulator3LocationText) || ParameterValidator.IsValidPath(emulator3LocationText);
        isEmulator4LocationValid = string.IsNullOrWhiteSpace(emulator4LocationText) || ParameterValidator.IsValidPath(emulator4LocationText);
        isEmulator5LocationValid = string.IsNullOrWhiteSpace(emulator5LocationText) || ParameterValidator.IsValidPath(emulator5LocationText);
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
                formatsToSearch.Select(static format => new XElement("FormatToSearch", format))),
            new XElement("ExtractFileBeforeLaunch", extractFileBeforeLaunch),
            new XElement("FileFormatsToLaunch",
                formatsToLaunch.Select(static format => new XElement("FormatToLaunch", format))),
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
            ?.ReplaceNodes(formatsToSearch.Select(static format => new XElement("FormatToSearch", format)));
        existingSystem.SetElementValue("ExtractFileBeforeLaunch", extractFileBeforeLaunch);
        existingSystem.Element("FileFormatsToLaunch")
            ?.ReplaceNodes(formatsToLaunch.Select(static format => new XElement("FormatToLaunch", format)));
        existingSystem.Element("Emulators")
            ?.Remove(); // Remove the existing emulators section before adding updated one
        existingSystem.Add(emulatorsElement);
    }

    private static bool CheckPaths(bool isSystemFolderValid, bool isSystemImageFolderValid, bool isEmulator1LocationValid,
        bool isEmulator2LocationValid, bool isEmulator3LocationValid, bool isEmulator4LocationValid,
        bool isEmulator5LocationValid)
    {
        if (isSystemFolderValid && isSystemImageFolderValid && isEmulator1LocationValid && isEmulator2LocationValid &&
            isEmulator3LocationValid && isEmulator4LocationValid && isEmulator5LocationValid) return false;

        // Notify user
        MessageBoxLibrary.PathOrParameterInvalidMessageBox();

        return true;
    }

    private static bool ValidateEmulator1Name(string emulator1NameText)
    {
        if (!string.IsNullOrEmpty(emulator1NameText)) return false;

        // Notify user
        MessageBoxLibrary.Emulator1RequiredMessageBox();

        return true;
    }

    private static bool ValidateFormatToLaunch(string formatToLaunchText, bool extractFileBeforeLaunch,
        out List<string> formatsToLaunch)
    {
        formatsToLaunch = formatToLaunchText.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(static format => format.Trim())
            .Where(static format => !string.IsNullOrEmpty(format))
            .ToList();

        if (formatsToLaunch.Count != 0 || !extractFileBeforeLaunch) return false;

        // Notify user
        MessageBoxLibrary.ExtensionToLaunchIsRequiredMessageBox();

        return true;
    }

    private static bool ValidateFormatToSearch(string formatToSearchText, bool extractFileBeforeLaunch,
        out List<string> formatsToSearch)
    {
        formatsToSearch = formatToSearchText.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(static format => format.Trim())
            .Where(static format => !string.IsNullOrEmpty(format))
            .ToList();
        if (formatsToSearch.Count == 0)
        {
            // Notify user
            MessageBoxLibrary.ExtensionToSearchIsRequiredMessageBox();

            return true;
        }

        if (!extractFileBeforeLaunch || formatsToSearch.All(static f => f is "zip" or "7z" or "rar")) return false;

        // Notify user
        MessageBoxLibrary.FileMustBeCompressedMessageBox();

        return true;
    }

    private bool ValidateSystemImageFolder(string systemNameText, ref string systemImageFolderText)
    {
        // Add the default System Image Folder if not provided by user
        if (systemImageFolderText.Length == 0 || string.IsNullOrEmpty(systemImageFolderText))
        {
            systemImageFolderText = $".\\images\\{systemNameText}";
            SystemImageFolderTextBox.Text = systemImageFolderText;

            // Create the directory if it doesn't exist
            if (!Directory.Exists(systemImageFolderText))
            {
                try
                {
                    Directory.CreateDirectory(systemImageFolderText);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "'Failed to create the default systemImageFolder.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);
                }
            }
        }

        if (!string.IsNullOrEmpty(systemImageFolderText)) return false;

        // Notify user
        MessageBoxLibrary.SystemImageFolderCanNotBeEmptyMessageBox();

        return true;
    }

    private bool ValidateSystemFolder(string systemNameText, ref string systemFolderText)
    {
        // Add the default System Folder if not provided by user
        if (systemFolderText.Length == 0 || string.IsNullOrEmpty(systemFolderText))
        {
            systemFolderText = $".\\roms\\{systemNameText}";
            SystemFolderTextBox.Text = systemFolderText;

            // Create the directory if it doesn't exist
            if (!Directory.Exists(systemFolderText))
            {
                try
                {
                    Directory.CreateDirectory(systemFolderText);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Failed to create the default systemFolder.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);
                }
            }
        }

        if (!string.IsNullOrEmpty(systemFolderText)) return false;

        // Notify user
        MessageBoxLibrary.SystemFolderCanNotBeEmptyMessageBox();

        return true;
    }

    private static bool ValidateSystemName(string systemNameText)
    {
        if (!string.IsNullOrEmpty(systemNameText)) return false;

        // Notify user
        MessageBoxLibrary.SystemNameCanNotBeEmptyMessageBox();

        return true;
    }

    private void HandleValidationAlerts(bool isSystemFolderValid, bool isSystemImageFolderValid,
        bool isEmulator1LocationValid, bool isEmulator2LocationValid, bool isEmulator3LocationValid,
        bool isEmulator4LocationValid, bool isEmulator5LocationValid)
    {
        MarkInvalid(SystemFolderTextBox, isSystemFolderValid);
        MarkInvalid(SystemImageFolderTextBox, isSystemImageFolderValid);
        MarkInvalid(Emulator1LocationTextBox, isEmulator1LocationValid);
        MarkInvalid(Emulator2LocationTextBox, isEmulator2LocationValid);
        MarkInvalid(Emulator3LocationTextBox, isEmulator3LocationValid);
        MarkInvalid(Emulator4LocationTextBox, isEmulator4LocationValid);
        MarkInvalid(Emulator5LocationTextBox, isEmulator5LocationValid);
    }

    private static void CreateFolders(string systemNameText)
    {
        var applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string[] folderNames = ["roms", "images", "title_snapshots", "gameplay_snapshots", "videos", "manuals", "walkthrough", "cabinets", "flyers", "pcbs", "carts"];

        foreach (var folderName in folderNames)
        {
            var parentDirectory = Path.Combine(applicationDirectory, folderName);

            // Ensure the parent directory exists
            if (!Directory.Exists(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            // Use SystemName as the name for the new folder inside the parent directory
            var newFolderPath = Path.Combine(parentDirectory, systemNameText);

            try
            {
                // Check if the folder exists, and create it if it doesn't
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                    if (folderName == "images")
                    {
                        // Notify user
                        MessageBoxLibrary.FolderCreatedMessageBox(systemNameText);
                    }
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "'Simple Launcher' failed to create the necessary folders for this system.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.FolderCreationFailedMessageBox();
            }
        }
    }

    private static void AddEmulatorToXml(XElement emulatorsElement, string name, string location, string parameters, bool receiveNotification = false)
    {
        if (string.IsNullOrEmpty(name)) return; // Check if the emulator name is not empty

        var emulatorElement = new XElement("Emulator",
            new XElement("EmulatorName", name),
            new XElement("EmulatorLocation", location),
            new XElement("EmulatorParameters", parameters),
            new XElement("ReceiveANotificationOnEmulatorError", receiveNotification));
        emulatorsElement.Add(emulatorElement);
    }

    private void DeleteSystemButton_Click(object sender, RoutedEventArgs e)
    {
        HelpUserTextBlock.Text = string.Empty;

        if (SystemNameDropdown.SelectedItem == null)
        {
            // Notify user
            MessageBoxLibrary.SelectASystemToDeleteMessageBox();

            return;
        }

        var selectedSystemName = SystemNameDropdown.SelectedItem.ToString();

        if (_xmlDoc == null) return;

        var systemNode = _xmlDoc.Descendants("SystemConfig")
            .FirstOrDefault(element => element.Element("SystemName")?.Value == selectedSystemName);

        if (systemNode != null)
        {
            //Ask user if he really wants to delete the system
            DoYouWanToDeleteSystemMessageBox();

            void DoYouWanToDeleteSystemMessageBox()
            {
                var result = MessageBoxLibrary.AreYouSureDoYouWantToDeleteThisSystemMessageBox();
                if (result != MessageBoxResult.Yes) return;

                systemNode.Remove();
                _xmlDoc.Save(XmlFilePath);
                PopulateSystemNamesDropdown();
                ClearFields();

                MessageBoxLibrary.SystemHasBeenDeletedMessageBox(selectedSystemName);
            }
        }
        else
        {
            // Notify user
            MessageBoxLibrary.SystemNotFoundInTheXmlMessageBox();
        }
    }

    private static void EditSystem_Closing(object sender, CancelEventArgs e)
    {
        // Create a backup file
        var appFolderPath = AppDomain.CurrentDomain.BaseDirectory;
        var sourceFilePath = Path.Combine(appFolderPath, "system.xml");
        var backupFileName = $"system_backup{DateTime.Now:yyyyMMdd_HHmmss}.xml";
        var backupFilePath = Path.Combine(appFolderPath, backupFileName);

        if (File.Exists(sourceFilePath))
        {
            File.Copy(sourceFilePath, backupFilePath);
        }
    }

    private void HelpLink_Click(object sender, RoutedEventArgs e)
    {
        PlayClick.PlayClickSound();
        const string searchUrl = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters";
        Process.Start(new ProcessStartInfo
        {
            FileName = searchUrl,
            UseShellExecute = true
        });
    }

    // Validate parameter fields in the UI
    private void ValidateParameterFields()
    {
        // Validate Emulator Parameter Text Boxes
        TextBox[] parameterTextBoxes =
        [
            Emulator1ParametersTextBox, Emulator2ParametersTextBox,
            Emulator3ParametersTextBox, Emulator4ParametersTextBox,
            Emulator5ParametersTextBox
        ];

        foreach (var textBox in parameterTextBoxes)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text)) continue;

            // Check if this is a MAME emulator
            var isMameSystem = SystemIsMameComboBox.SelectedItem != null &&
                               ((ComboBoxItem)SystemIsMameComboBox.SelectedItem).Content.ToString() == "true";

            // We only care about the boolean result here, not the specific paths
            var systemFolder = SystemFolderTextBox.Text;
            var areParametersValid = ParameterValidator.ValidateParameterPaths(textBox.Text, out _, systemFolder, isMameSystem);
            MarkInvalid(textBox, areParametersValid);
        }
    }

    // Validate and warn about parameters before saving
    private void ValidateAndWarnAboutParameters(string[] parameterTexts)
    {
        var hasInvalidParameters = false;
        var allInvalidPaths = new List<string>();
        string[] emulatorNames =
        [
            Emulator1NameTextBox.Text, Emulator2NameTextBox.Text,
            Emulator3NameTextBox.Text, Emulator4NameTextBox.Text,
            Emulator5NameTextBox.Text
        ];

        TextBox[] parameterTextBoxes =
        [
            Emulator1ParametersTextBox, Emulator2ParametersTextBox,
            Emulator3ParametersTextBox, Emulator4ParametersTextBox,
            Emulator5ParametersTextBox
        ];

        // Check if this is a MAME emulator
        var isMameSystem = SystemIsMameComboBox.SelectedItem != null &&
                           ((ComboBoxItem)SystemIsMameComboBox.SelectedItem).Content.ToString() == "true";

        for (var i = 0; i < parameterTextBoxes.Length; i++)
        {
            if (string.IsNullOrEmpty(parameterTexts[i]) || string.IsNullOrEmpty(emulatorNames[i])) continue;

            var systemFolder = SystemFolderTextBox.Text;
            var areParametersValid = ParameterValidator.ValidateParameterPaths(parameterTexts[i], out var invalidPaths, systemFolder, isMameSystem);

            MarkInvalid(parameterTextBoxes[i], areParametersValid);
            if (areParametersValid) continue;

            hasInvalidParameters = true;
            allInvalidPaths.AddRange(invalidPaths.Select(path => $"{emulatorNames[i]}: {path}"));
        }

        // Show detailed warning if invalid parameters found, but still continue with save
        if (hasInvalidParameters)
        {
            MessageBoxLibrary.ParameterPathsInvalidWarningMessageBox(allInvalidPaths);
        }
    }
}