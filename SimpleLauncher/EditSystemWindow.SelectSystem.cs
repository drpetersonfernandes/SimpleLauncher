using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml.Linq;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class EditSystemWindow
{
    private void PopulateSystemNamesDropdown()
    {
        if (_xmlDoc == null) return;

        var currentSelection = SystemNameDropdown.SelectedItem?.ToString();
        SystemNameDropdown.ItemsSource = _xmlDoc.Descendants("SystemConfig")
            .Select(static element => element.Element("SystemName")?.Value)
            .OrderBy(static name => name)
            .ToList();

        // Try to restore selection if it still exists
        if (currentSelection != null && SystemNameDropdown.Items.Contains(currentSelection))
        {
            SystemNameDropdown.SelectedItem = currentSelection;
        }
        else if (SystemNameDropdown.Items.Count > 0)
        {
            //SystemNameDropdown.SelectedIndex = 0; // Optionally select the first item
        }
        else
        {
            // No items, ensure UI reflects this (handled by SelectionChanged if selection becomes null)
        }
    }

    private void SystemNameDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentSelectedSystemName = SystemNameDropdown.SelectedItem?.ToString();
        _originalSystemName = currentSelectedSystemName; // Keep track of the name of the system being edited/viewed

        if (currentSelectedSystemName == null)
        {
            // No system is selected from the dropdown.
            ClearFieldsForNoSelection();
            DisableAllEditableFields();
            SaveSystemButton.IsEnabled = false;
            DeleteSystemButton.IsEnabled = false;
            HelpUserTextBlock.Document.Blocks.Clear();
        }
        else
        {
            // A system is selected. Load its details.
            LoadSystemDetails(currentSelectedSystemName);
        }
    }

    private void LoadSystemDetails(string systemNameToLoad)
    {
        // This method assumes systemNameToLoad is a valid, existing system name.
        // It will enable fields, populate them from _xmlDoc, and handle related UI updates.

        ClearAllEmulatorFieldsInternal(); // Clear all emulator fields first, including notifications
                                          // This also resets notification ComboBoxes.

        EnableFields();
        SaveSystemButton.IsEnabled = true;
        DeleteSystemButton.IsEnabled = true;

        if (_xmlDoc == null)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(new NullReferenceException("_xmlDoc is null in LoadSystemDetails."), "Critical error loading system details.");

            DisableAllEditableFields();
            SaveSystemButton.IsEnabled = false;
            DeleteSystemButton.IsEnabled = false;

            return;
        }

        var selectedSystem = _xmlDoc.Descendants("SystemConfig").FirstOrDefault(x => x.Element("SystemName")?.Value == systemNameToLoad);

        if (selectedSystem != null)
        {
            SystemNameTextBox.Text = selectedSystem.Element("SystemName")?.Value ?? string.Empty;

            // Load system folders
            SystemFolderTextBox.Text = string.Empty;
            AdditionalFoldersListBox.Items.Clear();

            var systemFoldersElement = selectedSystem.Element("SystemFolders");
            if (systemFoldersElement != null)
            {
                var folders = systemFoldersElement.Elements("SystemFolder").Select(static f => f.Value).ToList();
                if (folders.Count > 0)
                {
                    SystemFolderTextBox.Text = folders[0];
                }

                for (var i = 1; i < folders.Count; i++)
                {
                    AdditionalFoldersListBox.Items.Add(folders[i]);
                }
            }
            else
            {
                // Backward compatibility for the old <SystemFolder> tag
                SystemFolderTextBox.Text = selectedSystem.Element("SystemFolder")?.Value ?? string.Empty;
            }

            SystemImageFolderTextBox.Text = selectedSystem.Element("SystemImageFolder")?.Value ?? string.Empty;

            var systemIsMameValue = selectedSystem.Element("SystemIsMAME")?.Value == "true" ? "true" : "false";
            SystemIsMameComboBox.SelectedItem = SystemIsMameComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == systemIsMameValue);

            var formatToSearchValues = selectedSystem.Element("FileFormatsToSearch")?.Elements("FormatToSearch")
                .Select(static x => x.Value).ToArray();
            FormatToSearchTextBox.Text = formatToSearchValues != null ? string.Join(", ", formatToSearchValues) : string.Empty;

            var extractFileBeforeLaunchValue = selectedSystem.Element("ExtractFileBeforeLaunch")?.Value == "true" ? "true" : "false";
            ExtractFileBeforeLaunchComboBox.SelectedItem = ExtractFileBeforeLaunchComboBox.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == extractFileBeforeLaunchValue);

            var formatToLaunchValues = selectedSystem.Element("FileFormatsToLaunch")?.Elements("FormatToLaunch")
                .Select(static x => x.Value).ToArray();
            FormatToLaunchTextBox.Text = formatToLaunchValues != null ? string.Join(", ", formatToLaunchValues) : string.Empty;

            var emulators = selectedSystem.Element("Emulators")?.Elements("Emulator").ToList();
            if (emulators != null)
            {
                // Populate fields with saved strings (including %BASEFOLDER% for location)
                PopulateEmulatorFields(emulators.ElementAtOrDefault(0), Emulator1NameTextBox, Emulator1PathTextBox, Emulator1ParametersTextBox, ReceiveANotificationOnEmulatorError1);
                PopulateEmulatorFields(emulators.ElementAtOrDefault(1), Emulator2NameTextBox, Emulator2PathTextBox, Emulator2ParametersTextBox, ReceiveANotificationOnEmulatorError2);
                PopulateEmulatorFields(emulators.ElementAtOrDefault(2), Emulator3NameTextBox, Emulator3PathTextBox, Emulator3ParametersTextBox, ReceiveANotificationOnEmulatorError3);
                PopulateEmulatorFields(emulators.ElementAtOrDefault(3), Emulator4NameTextBox, Emulator4PathTextBox, Emulator4ParametersTextBox, ReceiveANotificationOnEmulatorError4);
                PopulateEmulatorFields(emulators.ElementAtOrDefault(4), Emulator5NameTextBox, Emulator5PathTextBox, Emulator5ParametersTextBox, ReceiveANotificationOnEmulatorError5);
            }
            // else: ClearAllEmulatorFieldsInternal() already handled this at the beginning.

            // Try creating default folders based on the *current UI text*, which might contain %BASEFOLDER%
            var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(SystemFolderTextBox.Text);
            TryCreateDefaultFolder(resolvedSystemFolder, $".\\roms\\{SystemNameTextBox.Text}", "SystemFolder"); // Pass resolved path

            var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(SystemImageFolderTextBox.Text);
            TryCreateDefaultFolder(resolvedSystemImageFolder, $".\\images\\{SystemNameTextBox.Text}", "SystemImageFolder"); // Pass resolved path


            // Mark validity. CheckPath.IsValidPath will now handle %BASEFOLDER% internally.
            MarkInvalid(SystemFolderTextBox, CheckPath.IsValidPath(SystemFolderTextBox.Text) || string.IsNullOrWhiteSpace(SystemFolderTextBox.Text));
            MarkInvalid(SystemImageFolderTextBox, CheckPath.IsValidPath(SystemImageFolderTextBox.Text) || string.IsNullOrWhiteSpace(SystemImageFolderTextBox.Text));
            MarkInvalid(Emulator1PathTextBox, string.IsNullOrWhiteSpace(Emulator1PathTextBox.Text) || CheckPath.IsValidPath(Emulator1PathTextBox.Text));
            MarkInvalid(Emulator2PathTextBox, string.IsNullOrWhiteSpace(Emulator2PathTextBox.Text) || CheckPath.IsValidPath(Emulator2PathTextBox.Text));
            MarkInvalid(Emulator3PathTextBox, string.IsNullOrWhiteSpace(Emulator3PathTextBox.Text) || CheckPath.IsValidPath(Emulator3PathTextBox.Text));
            MarkInvalid(Emulator4PathTextBox, string.IsNullOrWhiteSpace(Emulator4PathTextBox.Text) || CheckPath.IsValidPath(Emulator4PathTextBox.Text));
            MarkInvalid(Emulator5PathTextBox, string.IsNullOrWhiteSpace(Emulator5PathTextBox.Text) || CheckPath.IsValidPath(Emulator5PathTextBox.Text));

            // Validate parameter fields. This uses ParameterValidator which is updated to handle %BASEFOLDER% etc.
            ValidateParameterFields();

            HelpUserTextBlock.Document.Blocks.Clear();
            UiHelpers.HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
        }
        else
        {
            MessageBoxLibrary.SystemNotFoundInTheXmlMessageBox();
            ClearFieldsForNoSelection();
            DisableAllEditableFields();
            SaveSystemButton.IsEnabled = false;
            DeleteSystemButton.IsEnabled = false;
        }
    }

    private void PopulateEmulatorFields(XElement emulatorElement, TextBox nameTextBox, TextBox pathTextBox, TextBox paramsTextBox, ComboBox notificationComboBox)
    {
        if (emulatorElement != null)
        {
            nameTextBox.Text = emulatorElement.Element("EmulatorName")?.Value ?? string.Empty;
            // Load the saved string directly into the UI, including %BASEFOLDER% if present
            pathTextBox.Text = emulatorElement.Element("EmulatorLocation")?.Value ?? string.Empty;
            paramsTextBox.Text = emulatorElement.Element("EmulatorParameters")?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(nameTextBox.Text))
            {
                var receiveNotificationValue = emulatorElement.Element("ReceiveANotificationOnEmulatorError")?.Value == "false" ? "false" : "true";
                notificationComboBox.SelectedItem = notificationComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
            }
            else
            {
                notificationComboBox.SelectedItem = null; // Name is empty, clear notification
            }
        }
        else
        {
            // Fields are already cleared by ClearAllEmulatorFieldsInternal, but good to be explicit if this were standalone
            nameTextBox.Clear();
            MarkValid(nameTextBox);
            pathTextBox.Clear();
            MarkValid(pathTextBox);
            paramsTextBox.Clear();
            MarkValid(paramsTextBox);
            notificationComboBox.SelectedItem = null;
        }
    }

    // Accept a resolved path and compare against a default pattern
    private void TryCreateDefaultFolder(string resolvedCurrentPath, string defaultPatternPathWithSystemName, string folderTypeForLog)
    {
        // Ensure systemName is not empty before forming the pattern path
        var systemName = SystemNameTextBox.Text; // Get the current system name from UI
        if (string.IsNullOrEmpty(systemName)) return;

        // Resolve the default pattern path for comparison
        var resolvedDefaultPatternPath = PathHelper.ResolveRelativeToAppDirectory(defaultPatternPathWithSystemName);

        // Only create if the current path matches the default pattern AND the directory doesn't exist
        // Also check if the resolved path is valid before attempting creation
        if (string.IsNullOrEmpty(resolvedCurrentPath) ||
            !resolvedCurrentPath.Equals(resolvedDefaultPatternPath, StringComparison.OrdinalIgnoreCase) ||
            Directory.Exists(resolvedCurrentPath)) return;

        try
        {
            Directory.CreateDirectory(resolvedCurrentPath); // Create the resolved path
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, $"Unable to create default {folderTypeForLog}: {resolvedCurrentPath}");
        }
    }
}
