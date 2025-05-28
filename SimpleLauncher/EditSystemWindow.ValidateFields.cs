using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class EditSystemWindow
{
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
        emulator1LocationText = Emulator1PathTextBox.Text.Trim();
        emulator2LocationText = Emulator2PathTextBox.Text.Trim();
        emulator3LocationText = Emulator3PathTextBox.Text.Trim();
        emulator4LocationText = Emulator4PathTextBox.Text.Trim();
        emulator5LocationText = Emulator5PathTextBox.Text.Trim();
        emulator1ParametersText = Emulator1ParametersTextBox.Text.Trim();
        emulator2ParametersText = Emulator2ParametersTextBox.Text.Trim();
        emulator3ParametersText = Emulator3ParametersTextBox.Text.Trim();
        emulator4ParametersText = Emulator4ParametersTextBox.Text.Trim();
        emulator5ParametersText = Emulator5ParametersTextBox.Text.Trim();
    }

    private static void ValidatePaths(string systemNameText, string systemFolderText, string systemImageFolderText, string emulator1LocationText,
        string emulator2LocationText, string emulator3LocationText, string emulator4LocationText,
        string emulator5LocationText, out bool isSystemFolderValid, out bool isSystemImageFolderValid,
        out bool isEmulator1LocationValid, out bool isEmulator2LocationValid, out bool isEmulator3LocationValid,
        out bool isEmulator4LocationValid, out bool isEmulator5LocationValid)
    {
        // Define the valid patterns (using the sanitized systemNameText)
        var validSystemFolderPattern = $".\\roms\\{systemNameText}";
        var validSystemImageFolderPattern = $".\\images\\{systemNameText}";

        // Perform validation
        isSystemFolderValid = string.IsNullOrWhiteSpace(systemFolderText) || CheckPath.IsValidPath(systemFolderText) || systemFolderText == validSystemFolderPattern;
        isSystemImageFolderValid = string.IsNullOrWhiteSpace(systemImageFolderText) || CheckPath.IsValidPath(systemImageFolderText) || systemImageFolderText == validSystemImageFolderPattern;
        isEmulator1LocationValid = string.IsNullOrWhiteSpace(emulator1LocationText) || CheckPath.IsValidPath(emulator1LocationText);
        isEmulator2LocationValid = string.IsNullOrWhiteSpace(emulator2LocationText) || CheckPath.IsValidPath(emulator2LocationText);
        isEmulator3LocationValid = string.IsNullOrWhiteSpace(emulator3LocationText) || CheckPath.IsValidPath(emulator3LocationText);
        isEmulator4LocationValid = string.IsNullOrWhiteSpace(emulator4LocationText) || CheckPath.IsValidPath(emulator4LocationText);
        isEmulator5LocationValid = string.IsNullOrWhiteSpace(emulator5LocationText) || CheckPath.IsValidPath(emulator5LocationText);
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
                    _ = LogErrors.LogErrorAsync(ex, "Error creating system image folder.");
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
                    _ = LogErrors.LogErrorAsync(ex, "Error creating system folder.");
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
        // First, sanitize the input (though this is primarily handled in SaveSystemButton_Click)
        systemNameText = SanitizePaths.SanitizeFolderName(systemNameText);

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
        MarkInvalid(Emulator1PathTextBox, isEmulator1LocationValid);
        MarkInvalid(Emulator2PathTextBox, isEmulator2LocationValid);
        MarkInvalid(Emulator3PathTextBox, isEmulator3LocationValid);
        MarkInvalid(Emulator4PathTextBox, isEmulator4LocationValid);
        MarkInvalid(Emulator5PathTextBox, isEmulator5LocationValid);
    }

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
            var (areParametersValid, _) = ParameterValidator.ValidateParameterPaths(textBox.Text, systemFolder, isMameSystem);
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
            var (areParametersValid, invalidPaths) = ParameterValidator.ValidateParameterPaths(parameterTexts[i], systemFolder, isMameSystem);

            MarkInvalid(parameterTextBoxes[i], areParametersValid);
            if (areParametersValid) continue;

            hasInvalidParameters = true;
            allInvalidPaths.AddRange(invalidPaths.Select(path => $"{emulatorNames[i]}: {path}"));
        }

        // Show detailed warning if invalid parameters found, but still continue with save
        if (!hasInvalidParameters) return;

        {
            MessageBoxLibrary.ParameterPathsInvalidWarningMessageBox(allInvalidPaths);
        }
    }
}