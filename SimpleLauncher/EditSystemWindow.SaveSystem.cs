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
                UpdateXml(existingSystem, systemFolderText, systemImageFolderText, systemIsMame, formatsToSearch, extractFileBeforeLaunch, formatsToLaunch, emulatorsElement);
            }
            else
            {
                // Create and add a new system element
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

            // Validate paths
            ValidatePaths(systemNameText, systemFolderText, systemImageFolderText, emulator1LocationText,
                emulator2LocationText, emulator3LocationText, emulator4LocationText, emulator5LocationText,
                out var isSystemFolderValid, out var isSystemImageFolderValid, out var isEmulator1LocationValid,
                out var isEmulator2LocationValid, out var isEmulator3LocationValid, out var isEmulator4LocationValid,
                out var isEmulator5LocationValid);

            // Handle validation alerts
            HandleValidationAlerts(isSystemFolderValid, isSystemImageFolderValid, isEmulator1LocationValid,
                isEmulator2LocationValid, isEmulator3LocationValid, isEmulator4LocationValid, isEmulator5LocationValid);

            // Validate SystemName (now with sanitized value)
            if (ValidateSystemName(systemNameText)) return; // This will use the sanitized value

            SystemNameTextBox.Text = systemNameText; // Update UI

            // Validate SystemFolder
            if (ValidateSystemFolder(systemNameText, ref systemFolderText)) return;

            // Validate SystemImageFolder
            if (ValidateSystemImageFolder(systemNameText, ref systemImageFolderText)) return;

            // Validate systemIsMame
            // Set to false if user does not choose
            var systemIsMame =
                ((SystemIsMameComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString())?.Equals("true",
                    StringComparison.OrdinalIgnoreCase) ?? false;

            // Validate extractFileBeforeLaunch
            // Set to false if user does not choose
            var extractFileBeforeLaunch = ExtractFileBeforeLaunchComboBox.SelectedItem != null &&
                                          bool.Parse((ExtractFileBeforeLaunchComboBox.SelectedItem as ComboBoxItem)
                                              ?.Content.ToString() ?? "false");

            // Validate FormatToSearch
            if (ValidateFormatToSearch(formatToSearchText, extractFileBeforeLaunch, out var formatsToSearch)) return;

            // Validate FormatToLaunch
            if (ValidateFormatToLaunch(formatToLaunchText, extractFileBeforeLaunch, out var formatsToLaunch)) return;

            // Validate Emulator1Name
            if (ValidateEmulator1Name(emulator1NameText)) return;

            // Check paths
            if (CheckPaths(isSystemFolderValid, isSystemImageFolderValid, isEmulator1LocationValid,
                    isEmulator2LocationValid, isEmulator3LocationValid, isEmulator4LocationValid,
                    isEmulator5LocationValid)) return;

            // Check parameter paths
            string[] parameterTexts =
            [
                emulator1ParametersText, emulator2ParametersText, emulator3ParametersText, emulator4ParametersText,
                emulator5ParametersText
            ];
            ValidateAndWarnAboutParameters(parameterTexts);

            // Get the notification settings, defaulting to true if not selected or null
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

            AddEmulatorToXml(emulatorsElement, emulator1NameText, emulator1LocationText, emulator1ParametersText,
                receiveNotification1);

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
                    MessageBoxLibrary.EmulatorNameMustBeUniqueMessageBox(emulatorName);

                    return;
                }

                ////////////////
                // XML factory//
                ////////////////
                AddEmulatorToXml(emulatorsElement, emulatorName, emulatorLocation, emulatorParameters,
                    receiveNotification);
            }

            // Check if we're updating an existing system
            var isUpdate = !string.IsNullOrEmpty(_originalSystemName) && SystemNameDropdown.SelectedItem != null;

            ////////////////
            // XML factory//
            ////////////////
            try
            {
                // Disable save button during save
                SaveSystemButton.IsEnabled = false;

                await SaveSystemConfigurationAsync(
                    systemNameText, systemFolderText, systemImageFolderText,
                    systemIsMame, formatsToSearch, extractFileBeforeLaunch,
                    formatsToLaunch, emulatorsElement, isUpdate,
                    _originalSystemName);

                // --- UI Updates and Post-Save Actions (only if save succeeds) ---
                // Repopulate the SystemNamesDropbox
                PopulateSystemNamesDropdown();

                // Select the saved/updated system in the Dropbox
                SystemNameDropdown.SelectedItem = systemNameText;

                // Notify user of success
                MessageBoxLibrary.SystemSavedSuccessfullyMessageBox();

                // Create folders if necessary (this might also benefit from being async if slow)
                CreateFolders(systemNameText);

                // Update the original system name to match the current name after save
                _originalSystemName = systemNameText;
            }
            catch (InvalidOperationException ex) // Catch the specific exception thrown by the helper
            {
                // Notify user about the save failure
                // The error is already logged by the helper method
                MessageBoxLibrary.SaveSystemFailedMessageBox(ex.InnerException
                    ?.Message); // Show inner exception message if available
            }
            catch (Exception ex) // Catch any other unexpected errors
            {
                // Notify developer
                const string contextMessage = "Unexpected error during system save process.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SaveSystemFailedMessageBox("An unexpected error occurred.");
            }
            finally
            {
                // Re-enable save button regardless of success or failure
                SaveSystemButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error saving system configuration.");
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

    private static void CreateFolders(string systemNameText)
    {
        var applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var folderNames = GetAdditionalFolders.GetFolders();

        foreach (var folderName in folderNames)
        {
            var parentDirectory = Path.Combine(applicationDirectory, folderName);

            // Ensure the parent directory exists
            if (!Directory.Exists(parentDirectory))
            {
                try
                {
                    Directory.CreateDirectory(parentDirectory);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, "Error creating additional folder.");
                }
            }

            // Use SystemName as the name for the new folder inside the parent directory
            var newFolderPath = Path.Combine(parentDirectory, systemNameText);

            try
            {
                // Check if the folder exists and create it if it doesn't
                if (!Directory.Exists(newFolderPath))
                {
                    try
                    {
                        Directory.CreateDirectory(newFolderPath);
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        _ = LogErrors.LogErrorAsync(ex, "Error creating additional folder.");
                    }

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
}