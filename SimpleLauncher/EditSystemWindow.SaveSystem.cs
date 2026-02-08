using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.CreateFolders;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SanitizeInputString;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher;

internal partial class EditSystemWindow
{
    private async void SaveSystemButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            // Trim input values
            TrimInputValues(out var systemNameText, out var systemFolderText, out var varSystemImageFolderText,
                out var formatToSearchText, out var formatToLaunchText, out var emulator1NameText,
                out var emulator2NameText, out var emulator3NameText, out var emulator4NameText,
                out var emulator5NameText, out var emulator1LocationText, out var emulator2LocationText,
                out var emulator3LocationText, out var emulator4LocationText, out var emulator5LocationText,
                out var emulator1ParametersText, out var emulator2ParametersText, out var emulator3ParametersText,
                out var emulator4ParametersText, out var emulator5ParametersText);

            // Sanitize SystemNameTextBox.Text immediately
            systemNameText = SanitizeInputSystemName.SanitizeFolderName(systemNameText);
            SystemNameTextBox.Text = systemNameText;

            // --- Collect all system folders ---
            var allSystemFolders = new List<string> { systemFolderText };
            allSystemFolders.AddRange(AdditionalFoldersListBox.Items.Cast<string>().Select(static f => f.Trim()));
            allSystemFolders = allSystemFolders.Where(static f => !string.IsNullOrWhiteSpace(f)).ToList();

            // --- Apply %BASEFOLDER% prefix to relative paths before validation/saving ---
            allSystemFolders = allSystemFolders.Select(MaybeAddBaseFolderPrefix).ToList();
            varSystemImageFolderText = MaybeAddBaseFolderPrefix(varSystemImageFolderText);
            emulator1LocationText = MaybeAddBaseFolderPrefix(emulator1LocationText);
            emulator2LocationText = MaybeAddBaseFolderPrefix(emulator2LocationText);
            emulator3LocationText = MaybeAddBaseFolderPrefix(emulator3LocationText);
            emulator4LocationText = MaybeAddBaseFolderPrefix(emulator4LocationText);
            emulator5LocationText = MaybeAddBaseFolderPrefix(emulator5LocationText);

            // --- Update UI with processed values ---
            SystemFolderTextBox.Text = allSystemFolders.FirstOrDefault() ?? string.Empty;
            AdditionalFoldersListBox.Items.Clear();
            foreach (var folder in allSystemFolders.Skip(1))
            {
                AdditionalFoldersListBox.Items.Add(folder);
            }

            SystemImageFolderTextBox.Text = varSystemImageFolderText;
            Emulator1PathTextBox.Text = emulator1LocationText;
            Emulator2PathTextBox.Text = emulator2LocationText;
            Emulator3PathTextBox.Text = emulator3LocationText;
            Emulator4PathTextBox.Text = emulator4LocationText;
            Emulator5PathTextBox.Text = emulator5LocationText;

            // Validate paths (now using potentially prefixed paths)
            // The ValidatePaths method itself doesn't need to understand %BASEFOLDER%
            // because CheckPath.IsValidPath can handle it.
            ValidatePaths(systemNameText, allSystemFolders.FirstOrDefault(), varSystemImageFolderText, emulator1LocationText,
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
            var firstFolder = allSystemFolders.FirstOrDefault() ?? string.Empty;
            if (ValidateSystemFolder(systemNameText, ref firstFolder)) return;

            if (allSystemFolders.Count > 0)
            {
                allSystemFolders[0] = firstFolder;
            }
            else
            {
                allSystemFolders.Add(firstFolder);
            }

            // Validate SystemImageFolder (uses the potentially prefixed value)
            if (ValidateSystemImageFolder(systemNameText, ref varSystemImageFolderText)) return;

            var systemIsMame =
                (SystemIsMameComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.Equals("true",
                    StringComparison.OrdinalIgnoreCase) ?? false;

            var extractFileBeforeLaunch = ExtractFileBeforeLaunchComboBox.SelectedItem != null &&
                                          bool.Parse((ExtractFileBeforeLaunchComboBox.SelectedItem as ComboBoxItem)
                                              ?.Content.ToString() ?? "false");

            var groupByFolder = (GroupByFolderComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
                ?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

            if (ValidateFormatToSearch(formatToSearchText, extractFileBeforeLaunch, out var formatsToSearch))
            {
                MarkInvalid(FormatToSearchTextBox, false); // Invalid state
                return;
            }
            else
            {
                MarkValid(FormatToSearchTextBox); // Valid state
            }

            if (ValidateFormatToLaunch(formatToLaunchText, extractFileBeforeLaunch, out var formatsToLaunch))
            {
                return;
            }

            if (ValidateEmulator1Name(emulator1NameText))
            {
                return;
            }

            if (ValidateEmulator1Location(emulator1LocationText, formatsToSearch))
            {
                MarkInvalid(Emulator1PathTextBox, false);
                return;
            }

            if (ValidateEmulator2Location(emulator2NameText, emulator2LocationText, formatsToSearch))
            {
                MarkInvalid(Emulator2PathTextBox, false);
                return;
            }

            if (ValidateEmulator3Location(emulator3NameText, emulator3LocationText, formatsToSearch))
            {
                MarkInvalid(Emulator3PathTextBox, false);
                return;
            }

            if (ValidateEmulator4Location(emulator4NameText, emulator4LocationText, formatsToSearch))
            {
                MarkInvalid(Emulator4PathTextBox, false);
                return;
            }

            if (ValidateEmulator5Location(emulator5NameText, emulator5LocationText, formatsToSearch))
            {
                MarkInvalid(Emulator5PathTextBox, false);
                return;
            }

            // Check if any of the *location* paths are invalid after prefixing/validation
            if (CheckPaths(isSystemFolderValid, isSystemImageFolderValid, isEmulator1LocationValid,
                    isEmulator2LocationValid, isEmulator3LocationValid, isEmulator4LocationValid,
                    isEmulator5LocationValid)) return;

            // Warn user if GroupByFolder is true with a non-MAME emulator
            if (groupByFolder)
            {
                var emulatorsToCheck = new[]
                {
                    (Name: emulator1NameText, Location: emulator1LocationText),
                    (Name: emulator2NameText, Location: emulator2LocationText),
                    (Name: emulator3NameText, Location: emulator3LocationText),
                    (Name: emulator4NameText, Location: emulator4LocationText),
                    (Name: emulator5NameText, Location: emulator5LocationText)
                };

                var hasMameEmulator = emulatorsToCheck.Any(static emu =>
                    !string.IsNullOrEmpty(emu.Name) && (emu.Name.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
                                                        (emu.Location != null && (emu.Location.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ||
                                                                                  emu.Location.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase))))
                );

                if (!hasMameEmulator)
                {
                    var result = MessageBoxLibrary.GroupByFolderMameWarningMessageBox();
                    if (result == MessageBoxResult.No)
                    {
                        return; // User chose not to save, so abort.
                    }
                }
            }

            string[] parameterTexts =
            [
                emulator1ParametersText, emulator2ParametersText, emulator3ParametersText, emulator4ParametersText,
                emulator5ParametersText
            ];
            string[] allEmulatorLocationTexts = // Used for validating parameters
            [
                emulator1LocationText, emulator2LocationText, emulator3LocationText, emulator4LocationText, emulator5LocationText
            ];

            var receiveNotification1 = ReceiveANotificationOnEmulatorError1.SelectedItem is not ComboBoxItem { Content: not null } item1 || item1.Content.ToString() == "true";
            var receiveNotification2 = ReceiveANotificationOnEmulatorError2.SelectedItem is not ComboBoxItem { Content: not null } item2 || item2.Content.ToString() == "true";
            var receiveNotification3 = ReceiveANotificationOnEmulatorError3.SelectedItem is not ComboBoxItem { Content: not null } item3 || item3.Content.ToString() == "true";
            var receiveNotification4 = ReceiveANotificationOnEmulatorError4.SelectedItem is not ComboBoxItem { Content: not null } item4 || item4.Content.ToString() == "true";
            var receiveNotification5 = ReceiveANotificationOnEmulatorError5.SelectedItem is not ComboBoxItem { Content: not null } item5 || item5.Content.ToString() == "true";

            var emulators = new List<Services.SystemManager.SystemManager.Emulator>();
            var emulatorNames = new HashSet<string>();

            // Add Emulator 1
            if (!string.IsNullOrEmpty(emulator1NameText)) // Only add if name is provided
            {
                if (!emulatorNames.Add(emulator1NameText))
                {
                    MessageBoxLibrary.EmulatorNameMustBeUniqueMessageBox(emulator1NameText);
                    return;
                }

                emulators.Add(new Services.SystemManager.SystemManager.Emulator
                {
                    EmulatorName = emulator1NameText,
                    EmulatorLocation = emulator1LocationText,
                    EmulatorParameters = emulator1ParametersText,
                    ReceiveANotificationOnEmulatorError = receiveNotification1
                });
            }

            string[] nameTexts = [emulator2NameText, emulator3NameText, emulator4NameText, emulator5NameText];
            // locationTexts are already defined as allEmulatorLocationTexts
            bool[] receiveNotifications = [receiveNotification2, receiveNotification3, receiveNotification4, receiveNotification5];

            for (var i = 0; i < nameTexts.Length; i++)
            {
                var currentEmulatorName = nameTexts[i];
                var currentEmulatorLocation = allEmulatorLocationTexts[i + 1]; // Use potentially prefixed location (index i+1 for emulators 2-5)
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

                emulators.Add(new Services.SystemManager.SystemManager.Emulator
                {
                    EmulatorName = currentEmulatorName,
                    EmulatorLocation = currentEmulatorLocation,
                    EmulatorParameters = currentEmulatorParameters,
                    ReceiveANotificationOnEmulatorError = currentReceiveNotification
                });
            }

            var isUpdate = !string.IsNullOrEmpty(_originalSystemName) && SystemNameDropdown.SelectedItem != null && _originalSystemName == SystemNameDropdown.SelectedItem.ToString();
            var originalSystemNameToUse = isUpdate ? _originalSystemName : systemNameText;

            var systemToSave = new Services.SystemManager.SystemManager
            {
                SystemName = systemNameText,
                SystemFolders = allSystemFolders,
                SystemImageFolder = varSystemImageFolderText,
                SystemIsMame = systemIsMame,
                FileFormatsToSearch = formatsToSearch.ToList(),
                ExtractFileBeforeLaunch = extractFileBeforeLaunch,
                GroupByFolder = groupByFolder,
                FileFormatsToLaunch = formatsToLaunch.ToList(),
                Emulators = emulators
            };

            try
            {
                SaveSystemButton.IsEnabled = false;
                await Services.SystemManager.SystemManager.SaveSystemConfigurationAsync(systemToSave, originalSystemNameToUse);

                await LoadSystemsAsync();
                SystemNameDropdown.SelectedItem = systemNameText;
                LoadSystemDetails(systemNameText); // This will load the saved values (including %BASEFOLDER%) back into UI

                // Notify user
                MessageBoxLibrary.SystemSavedSuccessfullyMessageBox();

                // Create folders based on the resolved paths
                var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(allSystemFolders.FirstOrDefault() ?? string.Empty);
                var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(varSystemImageFolderText);
                CreateDefaultSystemFolders.CreateFolders(systemNameText, resolvedSystemFolder, resolvedSystemImageFolder, _configuration);

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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error saving system configuration.");
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
        return Path.Combine("%BASEFOLDER%", trimmedPath);
    }
}