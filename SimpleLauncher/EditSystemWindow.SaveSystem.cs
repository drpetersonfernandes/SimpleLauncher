using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Xml.XPath;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class EditSystemWindow
{
    private async Task SaveSystemConfigurationAsync(
        string systemNameText,
        string systemFolderText,
        string systemImageFolderText,
        bool systemIsMame,
        List<string> formatsToSearch,
        bool extractFileBeforeLaunch,
        List<string> formatsToLaunch,
        XElement emulatorsElement,
        bool isUpdate,
        string originalSystemName)
    {
        try
        {
            // Ensure the main XML document exists in memory
            _xmlDoc ??= new XDocument(new XElement("SystemConfigs"));

            // Determine the system identifier for finding/adding the element
            var systemIdentifier = isUpdate ? originalSystemName : systemNameText;

            // Find the existing system element or prepare to add a new one
            var existingSystem = _xmlDoc.Root?.XPathSelectElement($"//SystemConfig[SystemName='{systemIdentifier}']");

            if (existingSystem != null)
            {
                // Update the existing system
                // If the name is changing, update it first
                if (isUpdate && systemIdentifier != systemNameText)
                {
                    existingSystem.SetElementValue("SystemName", systemNameText);
                }

                // Update all other fields
                // Pass the potentially modified strings from SaveSystemButton_Click
                UpdateXml(existingSystem, systemFolderText, systemImageFolderText, systemIsMame, formatsToSearch, extractFileBeforeLaunch, formatsToLaunch, emulatorsElement);
            }
            else
            {
                // Create and add a new system element
                // Pass the potentially modified strings from SaveSystemButton_Click
                var newSystem = AddToXml(systemNameText, systemFolderText, systemImageFolderText, systemIsMame, formatsToSearch, extractFileBeforeLaunch, formatsToLaunch, emulatorsElement);
                _xmlDoc.Root?.Add(newSystem); // Add to the root
            }

            // Create a new document for sorting to avoid modifying _xmlDoc directly during sort
            var sortedDoc = new XDocument(new XElement("SystemConfigs",
                _xmlDoc.Root?.Elements("SystemConfig")
                    .OrderBy(static system => system.Element("SystemName")?.Value)
                    .Select(static el =>
                        new XElement(el)) // Create copies to avoid modifying original _xmlDoc elements during sort
                ?? Enumerable.Empty<XElement>() // Handle case where Root is null
            ));

            // Save the sorted document asynchronously with formatting
            // Use SaveOptions.None for default indentation
            await Task.Run(() => sortedDoc.Save(XmlFilePath, SaveOptions.None));
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error saving system configuration to XML.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Rethrow the exception so the calling method can handle UI feedback
            throw new InvalidOperationException("Failed to save system configuration.", ex);
        }
    }

    private async void SaveSystemButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Trim input values
            TrimInputValues(out var systemNameText, out var systemFolderText, out var systemImageFolderText,
                out var formatToSearchText, out var formatToLaunchText, out var emulator1NameText,
                out var emulator2NameText, out var emulator3NameText, out var emulator4NameText,
                out var emulator5NameText, out var emulator1LocationText, out var emulator2LocationText,
                out var emulator3LocationText, out var emulator4LocationText, out var emulator5LocationText,
                out var emulator1ParametersText, out var emulator2ParametersText, out var emulator3ParametersText,
                out var emulator4ParametersText, out var emulator5ParametersText);

            // Sanitize SystemNameTextBox.Text immediately
            systemNameText = SanitizePaths.SanitizeFolderName(systemNameText);

            // --- Apply %BASEFOLDER% prefix to relative paths before validation/saving ---
            systemFolderText = MaybeAddBaseFolderPrefix(systemFolderText);
            SystemFolderTextBox.Text = systemFolderText; // Update UI
            systemImageFolderText = MaybeAddBaseFolderPrefix(systemImageFolderText);
            SystemImageFolderTextBox.Text = systemImageFolderText; // Update UI
            emulator1LocationText = MaybeAddBaseFolderPrefix(emulator1LocationText);
            Emulator1PathTextBox.Text = emulator1LocationText; // Update UI
            emulator2LocationText = MaybeAddBaseFolderPrefix(emulator2LocationText);
            Emulator2PathTextBox.Text = emulator2LocationText; // Update UI
            emulator3LocationText = MaybeAddBaseFolderPrefix(emulator3LocationText);
            Emulator3PathTextBox.Text = emulator3LocationText; // Update UI
            emulator4LocationText = MaybeAddBaseFolderPrefix(emulator4LocationText);
            Emulator4PathTextBox.Text = emulator4LocationText; // Update UI
            emulator5LocationText = MaybeAddBaseFolderPrefix(emulator5LocationText);
            Emulator5PathTextBox.Text = emulator5LocationText; // Update UI

            // Note: Parameters are NOT automatically prefixed. User must type %BASEFOLDER% manually.
            // We will validate and warn about relative paths in parameters that *don't* have the prefix.

            // Validate paths (now using potentially prefixed paths)
            // The ValidatePaths method itself doesn't need to understand %BASEFOLDER%
            // because CheckPath.IsValidPath will be updated to handle it.
            ValidatePaths(systemNameText, systemFolderText, systemImageFolderText, emulator1LocationText,
                emulator2LocationText, emulator3LocationText, emulator4LocationText, emulator5LocationText,
                out var isSystemFolderValid, out var isSystemImageFolderValid, out var isEmulator1LocationValid,
                out var isEmulator2LocationValid, out var isEmulator3LocationValid, out var isEmulator4LocationValid,
                out var isEmulator5LocationValid);

            // Handle validation alerts (coloring)
            HandleValidationAlerts(isSystemFolderValid, isSystemImageFolderValid, isEmulator1LocationValid,
                isEmulator2LocationValid, isEmulator3LocationValid, isEmulator4LocationValid, isEmulator5LocationValid);

            // Validate SystemName (now with sanitized value)
            if (ValidateSystemName(systemNameText)) return;

            // Validate SystemFolder (uses the potentially prefixed value)
            if (ValidateSystemFolder(systemNameText, ref systemFolderText)) return;

            // Validate SystemImageFolder (uses the potentially prefixed value)
            if (ValidateSystemImageFolder(systemNameText, ref systemImageFolderText)) return;

            var systemIsMame =
                ((SystemIsMameComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString())?.Equals("true",
                    StringComparison.OrdinalIgnoreCase) ?? false;

            var extractFileBeforeLaunch = ExtractFileBeforeLaunchComboBox.SelectedItem != null &&
                                          bool.Parse((ExtractFileBeforeLaunchComboBox.SelectedItem as ComboBoxItem)
                                              ?.Content.ToString() ?? "false");

            if (ValidateFormatToSearch(formatToSearchText, extractFileBeforeLaunch, out var formatsToSearch)) return;
            if (ValidateFormatToLaunch(formatToLaunchText, extractFileBeforeLaunch, out var formatsToLaunch)) return;
            if (ValidateEmulator1Name(emulator1NameText)) return;

            // Check if any of the *location* paths are invalid after prefixing/validation
            if (CheckPaths(isSystemFolderValid, isSystemImageFolderValid, isEmulator1LocationValid,
                    isEmulator2LocationValid, isEmulator3LocationValid, isEmulator4LocationValid,
                    isEmulator5LocationValid)) return;

            string[] parameterTexts =
            [
                emulator1ParametersText, emulator2ParametersText, emulator3ParametersText, emulator4ParametersText,
                emulator5ParametersText
            ];

            // --- Existing validation and warning for *invalid* paths (paths that don't exist) ---
            // This will now use the updated ParameterValidator which understands %BASEFOLDER%
            ValidateAndWarnAboutParameters(parameterTexts);
            // Note: ValidateAndWarnAboutParameters shows a warning but *does not* return, allowing the user to proceed.

            // --- Check for *relative* paths *in parameters* that *don't* have %BASEFOLDER% ---
            var relativePathsWithoutPrefixInParameters = new List<string>();
            relativePathsWithoutPrefixInParameters.AddRange(ParameterValidator.GetRelativePathsInParameters(emulator1ParametersText));
            relativePathsWithoutPrefixInParameters.AddRange(ParameterValidator.GetRelativePathsInParameters(emulator2ParametersText));
            relativePathsWithoutPrefixInParameters.AddRange(ParameterValidator.GetRelativePathsInParameters(emulator3ParametersText));
            relativePathsWithoutPrefixInParameters.AddRange(ParameterValidator.GetRelativePathsInParameters(emulator4ParametersText));
            relativePathsWithoutPrefixInParameters.AddRange(ParameterValidator.GetRelativePathsInParameters(emulator5ParametersText));

            if (relativePathsWithoutPrefixInParameters.Count != 0)
            {
                // Update the message box to inform the user about manual %BASEFOLDER% for parameters
                var result = MessageBoxLibrary.RelativePathsWarningMessageBox(relativePathsWithoutPrefixInParameters.Distinct().ToList());
                if (result == MessageBoxResult.No)
                {
                    // User chose not to save because of relative paths without prefix in parameters
                    return;
                }
                // If result is Yes, continue with the save process (paths will be saved as entered,
                // and the user was warned they might not resolve correctly at launch without %BASEFOLDER%)
            }

            var receiveNotification1 =
                ReceiveANotificationOnEmulatorError1.SelectedItem is not ComboBoxItem { Content: not null } item1 ||
                item1.Content.ToString() == "true";
            var receiveNotification2 =
                ReceiveANotificationOnEmulatorError2.SelectedItem is not ComboBoxItem { Content: not null } item2 ||
                item2.Content.ToString() == "true";
            var receiveNotification3 =
                ReceiveANotificationOnEmulatorError3.SelectedItem is not ComboBoxItem { Content: not null } item3 ||
                item3.Content.ToString() == "true";
            var receiveNotification4 =
                ReceiveANotificationOnEmulatorError4.SelectedItem is not ComboBoxItem { Content: not null } item4 ||
                item4.Content.ToString() == "true";
            var receiveNotification5 =
                ReceiveANotificationOnEmulatorError5.SelectedItem is not ComboBoxItem { Content: not null } item5 ||
                item5.Content.ToString() == "true";

            var emulatorsElement = new XElement("Emulators");
            var emulatorNames = new HashSet<string>();

            // Add Emulator 1
            if (!string.IsNullOrEmpty(emulator1NameText)) // Only add if name is provided
            {
                if (!emulatorNames.Add(emulator1NameText))
                {
                    MessageBoxLibrary.EmulatorNameMustBeUniqueMessageBox(emulator1NameText);
                    return;
                }

                // Use the potentially prefixed location and original parameter text
                AddEmulatorToXml(emulatorsElement, emulator1NameText, emulator1LocationText, emulator1ParametersText, receiveNotification1);
            }

            string[] nameTexts = [emulator2NameText, emulator3NameText, emulator4NameText, emulator5NameText];
            string[] locationTexts = [emulator2LocationText, emulator3LocationText, emulator4LocationText, emulator5LocationText]; // Use potentially prefixed locations
            bool[] receiveNotifications = [receiveNotification2, receiveNotification3, receiveNotification4, receiveNotification5];

            for (var i = 0; i < nameTexts.Length; i++)
            {
                var currentEmulatorName = nameTexts[i];
                var currentEmulatorLocation = locationTexts[i]; // Use potentially prefixed location
                var currentEmulatorParameters = parameterTexts[i + 1]; // Use original parameter text
                var currentReceiveNotification = receiveNotifications[i];

                if (!string.IsNullOrEmpty(currentEmulatorLocation) || !string.IsNullOrEmpty(currentEmulatorParameters))
                {
                    if (string.IsNullOrEmpty(currentEmulatorName))
                    {
                        MessageBoxLibrary.EmulatorNameRequiredMessageBox(i + 2); // Pass emulator number (2-5)
                        return;
                    }
                }

                if (string.IsNullOrEmpty(currentEmulatorName)) continue;

                if (!emulatorNames.Add(currentEmulatorName))
                {
                    MessageBoxLibrary.EmulatorNameMustBeUniqueMessageBox(currentEmulatorName);
                    return;
                }

                // Use the potentially prefixed location and original parameter text
                AddEmulatorToXml(emulatorsElement, currentEmulatorName, currentEmulatorLocation, currentEmulatorParameters, currentReceiveNotification);
            }

            var isUpdate = !string.IsNullOrEmpty(_originalSystemName) && SystemNameDropdown.SelectedItem != null && _originalSystemName == SystemNameDropdown.SelectedItem.ToString();

            try
            {
                SaveSystemButton.IsEnabled = false;

                await SaveSystemConfigurationAsync(
                    systemNameText, systemFolderText, systemImageFolderText, // Pass potentially prefixed paths
                    systemIsMame, formatsToSearch, extractFileBeforeLaunch,
                    formatsToLaunch, emulatorsElement, isUpdate,
                    _originalSystemName ?? systemNameText); // Pass systemNameText if _originalSystemName is null (new system)


                PopulateSystemNamesDropdown();
                SystemNameDropdown.SelectedItem = systemNameText;
                LoadSystemDetails(systemNameText); // This will load the saved values (including %BASEFOLDER%) back into UI

                // Notify user
                MessageBoxLibrary.SystemSavedSuccessfullyMessageBox();

                // Create folders based on the *resolved* paths, not the saved strings
                // This is already handled by TryCreateDefaultFolder in LoadSystemDetails
                CreateFolders(systemNameText);

                _originalSystemName = systemNameText; // Update original name after successful save & UI refresh
            }
            catch (InvalidOperationException ex)
            {
                // Notify user
                MessageBoxLibrary.SaveSystemFailedMessageBox(ex.InnerException?.Message);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Unexpected error during system save process.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SaveSystemFailedMessageBox("An unexpected error occurred.");
            }
            finally
            {
                SaveSystemButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error saving system configuration.");
        }
    }

    // Helper method to add %BASEFOLDER% prefix if the path is relative and doesn't have it
    private static string MaybeAddBaseFolderPrefix(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        // Check if it's already rooted (absolute)
        if (Path.IsPathRooted(path))
        {
            return path;
        }

        // Check if it already starts with the placeholder
        if (path.StartsWith("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        // It's a relative path without the placeholder, add it
        // Handle cases like ".\roms" or "../images"
        var trimmedPath = path.TrimStart('.', '\\', '/');
        return $"%BASEFOLDER%\\{trimmedPath}";
    }


    private static void AddEmulatorToXml(XElement emulatorsElement, string name, string location, string parameters, bool receiveNotification = false)
    {
        if (string.IsNullOrEmpty(name)) return;

        var emulatorElement = new XElement("Emulator",
            new XElement("EmulatorName", name),
            new XElement("EmulatorLocation", location),
            new XElement("EmulatorParameters", parameters),
            new XElement("ReceiveANotificationOnEmulatorError", receiveNotification));
        emulatorsElement.Add(emulatorElement);
    }

    private static XElement AddToXml(string systemNameText, string systemFolderText, string systemImageFolderText,
        bool systemIsMame, List<string> formatsToSearch, bool extractFileBeforeLaunch, List<string> formatsToLaunch,
        XElement emulatorsElement)
    {
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
        existingSystem.SetElementValue("SystemFolder", systemFolderText);
        existingSystem.SetElementValue("SystemImageFolder", systemImageFolderText);
        existingSystem.SetElementValue("SystemIsMAME", systemIsMame);
        existingSystem.Element("FileFormatsToSearch")
            ?.ReplaceNodes(formatsToSearch.Select(static format => new XElement("FormatToSearch", format)));
        existingSystem.SetElementValue("ExtractFileBeforeLaunch", extractFileBeforeLaunch);
        existingSystem.Element("FileFormatsToLaunch")
            ?.ReplaceNodes(formatsToLaunch.Select(static format => new XElement("FormatToLaunch", format)));
        existingSystem.Element("Emulators")?.Remove();
        existingSystem.Add(emulatorsElement);
    }

    private static void CreateFolders(string systemNameText)
    {
        if (string.IsNullOrEmpty(systemNameText)) return;

        var applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var folderNames = GetAdditionalFolders.GetFolders();

        foreach (var folderName in folderNames)
        {
            var parentDirectory = Path.Combine(applicationDirectory, folderName);

            if (!Directory.Exists(parentDirectory))
            {
                try
                {
                    Directory.CreateDirectory(parentDirectory);
                }
                catch (Exception ex)
                {
                    _ = LogErrors.LogErrorAsync(ex, $"Error creating parent directory: {parentDirectory}");
                }
            }

            var newFolderPath = Path.Combine(parentDirectory, systemNameText);

            try
            {
                if (!Directory.Exists(newFolderPath))
                {
                    Directory.CreateDirectory(newFolderPath);
                    if (folderName == "images")
                    {
                        MessageBoxLibrary.FolderCreatedMessageBox(systemNameText);
                    }
                }
            }
            catch (Exception ex)
            {
                _ = LogErrors.LogErrorAsync(ex, $"Error creating system specific folder: {newFolderPath}");
                if (folderName == "images") // Only show failure for images as per original logic
                {
                    MessageBoxLibrary.FolderCreationFailedMessageBox();
                }
            }
        }
    }
}