using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class EditSystemWindow
{
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
                    Emulator1PathTextBox.Text = emulator1.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator1ParametersTextBox.Text = emulator1.Element("EmulatorParameters")?.Value ?? string.Empty;

                    // Get the notification value, default to "true" if not found or not "false"
                    var receiveNotificationValue =
                        emulator1.Element("ReceiveANotificationOnEmulatorError")?.Value == "false"
                            ? "false"
                            : "true";
                    ReceiveANotificationOnEmulatorError1.SelectedItem = ReceiveANotificationOnEmulatorError1.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator1NameTextBox.Clear();
                    Emulator1PathTextBox.Clear();
                    Emulator1ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError1.SelectedIndex = -1;
                }

                var emulator2 = emulators.ElementAtOrDefault(1);
                if (emulator2 != null)
                {
                    Emulator2NameTextBox.Text = emulator2.Element("EmulatorName")?.Value ?? string.Empty;
                    Emulator2PathTextBox.Text = emulator2.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator2ParametersTextBox.Text = emulator2.Element("EmulatorParameters")?.Value ?? string.Empty;

                    // Get the notification value, default to "true" if not found or not "false"
                    var receiveNotificationValue =
                        emulator2.Element("ReceiveANotificationOnEmulatorError")?.Value == "false"
                            ? "false"
                            : "true";
                    ReceiveANotificationOnEmulatorError2.SelectedItem = ReceiveANotificationOnEmulatorError2.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator2NameTextBox.Clear();
                    Emulator2PathTextBox.Clear();
                    Emulator2ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError2.SelectedIndex = -1;
                }

                var emulator3 = emulators.ElementAtOrDefault(2);
                if (emulator3 != null)
                {
                    Emulator3NameTextBox.Text = emulator3.Element("EmulatorName")?.Value ?? string.Empty;
                    Emulator3PathTextBox.Text = emulator3.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator3ParametersTextBox.Text = emulator3.Element("EmulatorParameters")?.Value ?? string.Empty;

                    // Get the notification value, default to "true" if not found or not "false"
                    var receiveNotificationValue =
                        emulator3.Element("ReceiveANotificationOnEmulatorError")?.Value == "false"
                            ? "false"
                            : "true";
                    ReceiveANotificationOnEmulatorError3.SelectedItem = ReceiveANotificationOnEmulatorError3.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator3NameTextBox.Clear();
                    Emulator3PathTextBox.Clear();
                    Emulator3ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError3.SelectedIndex = -1;
                }

                var emulator4 = emulators.ElementAtOrDefault(3);
                if (emulator4 != null)
                {
                    Emulator4NameTextBox.Text = emulator4.Element("EmulatorName")?.Value ?? string.Empty;
                    Emulator4PathTextBox.Text = emulator4.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator4ParametersTextBox.Text = emulator4.Element("EmulatorParameters")?.Value ?? string.Empty;

                    // Get the notification value, default to "true" if not found or not "false"
                    var receiveNotificationValue =
                        emulator4.Element("ReceiveANotificationOnEmulatorError")?.Value == "false"
                            ? "false"
                            : "true";
                    ReceiveANotificationOnEmulatorError4.SelectedItem = ReceiveANotificationOnEmulatorError4.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator4NameTextBox.Clear();
                    Emulator4PathTextBox.Clear();
                    Emulator4ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError4.SelectedIndex = -1;
                }

                var emulator5 = emulators.ElementAtOrDefault(4);
                if (emulator5 != null)
                {
                    Emulator5NameTextBox.Text = emulator5.Element("EmulatorName")?.Value ?? string.Empty;
                    Emulator5PathTextBox.Text = emulator5.Element("EmulatorLocation")?.Value ?? string.Empty;
                    Emulator5ParametersTextBox.Text = emulator5.Element("EmulatorParameters")?.Value ?? string.Empty;

                    // Get the notification value, default to "true" if not found or not "false"
                    var receiveNotificationValue =
                        emulator5.Element("ReceiveANotificationOnEmulatorError")?.Value == "false"
                            ? "false"
                            : "true";
                    ReceiveANotificationOnEmulatorError5.SelectedItem = ReceiveANotificationOnEmulatorError5.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == receiveNotificationValue);
                }
                else
                {
                    Emulator5NameTextBox.Clear();
                    Emulator5PathTextBox.Clear();
                    Emulator5ParametersTextBox.Clear();
                    ReceiveANotificationOnEmulatorError5.SelectedIndex = -1;
                }
            }
        }

        // Create SystemFolder default
        if (SystemFolderTextBox.Text == $".\\roms\\{SystemNameTextBox.Text}" &&
            !Directory.Exists(SystemFolderTextBox.Text))
        {
            try
            {
                Directory.CreateDirectory(SystemFolderTextBox.Text);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Unable to create SystemFolder default");
            }
        }

        // Create SystemImageFolder default
        if (SystemImageFolderTextBox.Text == $".\\images\\{SystemNameTextBox.Text}" &&
            !Directory.Exists(SystemImageFolderTextBox.Text))
        {
            try
            {
                Directory.CreateDirectory(SystemImageFolderTextBox.Text);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Unable to create SystemImageFolder default");
            }
        }

        // Validate System Folder and System Image Folder
        MarkInvalid(SystemFolderTextBox, CheckPath.IsValidPath(SystemFolderTextBox.Text));
        MarkInvalid(SystemImageFolderTextBox, CheckPath.IsValidPath(SystemImageFolderTextBox.Text));

        // Validate Emulator Location Text Boxes (considered valid if empty)
        MarkInvalid(Emulator1PathTextBox, string.IsNullOrWhiteSpace(Emulator1PathTextBox.Text) || CheckPath.IsValidPath(Emulator1PathTextBox.Text));
        MarkInvalid(Emulator2PathTextBox, string.IsNullOrWhiteSpace(Emulator2PathTextBox.Text) || CheckPath.IsValidPath(Emulator2PathTextBox.Text));
        MarkInvalid(Emulator3PathTextBox, string.IsNullOrWhiteSpace(Emulator3PathTextBox.Text) || CheckPath.IsValidPath(Emulator3PathTextBox.Text));
        MarkInvalid(Emulator4PathTextBox, string.IsNullOrWhiteSpace(Emulator4PathTextBox.Text) || CheckPath.IsValidPath(Emulator4PathTextBox.Text));
        MarkInvalid(Emulator5PathTextBox, string.IsNullOrWhiteSpace(Emulator5PathTextBox.Text) || CheckPath.IsValidPath(Emulator5PathTextBox.Text));

        // Validate Parameter fields
        ValidateParameterFields();

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Text = string.Empty;
        UiHelpers.HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);

        // Update ReceiveANotificationOnEmulatorError
        if (string.IsNullOrEmpty(Emulator1NameTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError1.SelectedItem = null;
        }

        if (string.IsNullOrEmpty(Emulator2NameTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError2.SelectedItem = null;
        }

        if (string.IsNullOrEmpty(Emulator3NameTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError3.SelectedItem = null;
        }

        if (string.IsNullOrEmpty(Emulator4NameTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError4.SelectedItem = null;
        }

        if (string.IsNullOrEmpty(Emulator5NameTextBox.Text))
        {
            ReceiveANotificationOnEmulatorError5.SelectedItem = null;
        }
    }
}