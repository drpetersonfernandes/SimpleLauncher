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
        // CheckPath.IsValidPath is updated to handle %BASEFOLDER% and relative paths
        isSystemFolderValid = string.IsNullOrWhiteSpace(systemFolderText) || CheckPath.IsValidPath(systemFolderText);
        isSystemImageFolderValid = string.IsNullOrWhiteSpace(systemImageFolderText) || CheckPath.IsValidPath(systemImageFolderText);
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

    private static bool ValidateFormatToLaunch(string formatToLaunchText, bool extractFileBeforeLaunch, out List<string> formatsToLaunch)
    {
        formatsToLaunch = formatToLaunchText.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(static format => format.Trim())
            .Where(static format => !string.IsNullOrEmpty(format))
            .ToList();

        // When extractFileBeforeLaunch is true AND formatsToLaunch is empty, that's invalid
        // ReSharper disable once InvertIf
        if (extractFileBeforeLaunch && formatsToLaunch.Count == 0)
        {
            // Notify user
            MessageBoxLibrary.ExtensionToLaunchIsRequiredMessageBox();
            return true; // Return true to indicate validation failed
        }

        return false; // Return false to indicate validation passed
    }

    private static bool ValidateFormatToSearch(string formatToSearchText, bool extractFileBeforeLaunch, out List<string> formatsToSearch)
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

        // When extractFileBeforeLaunch is true, ALL formats must be zip, rar, or 7z
        // ReSharper disable once InvertIf
        if (extractFileBeforeLaunch && !formatsToSearch.All(static f => f is "zip" or "7z" or "rar"))
        {
            // Notify user
            MessageBoxLibrary.FileMustBeCompressedMessageBox();
            return true;
        }

        return false;
    }

    private bool ValidateSystemImageFolder(string systemNameText, ref string systemImageFolderText)
    {
        var defaultPattern = $".\\images\\{systemNameText}";
        var prefixedDefaultPattern = $"%BASEFOLDER%\\images\\{systemNameText}";

        if (string.IsNullOrEmpty(systemImageFolderText) || systemImageFolderText.Equals(defaultPattern, StringComparison.OrdinalIgnoreCase) || systemImageFolderText.Equals(prefixedDefaultPattern, StringComparison.OrdinalIgnoreCase))
        {
            systemImageFolderText = prefixedDefaultPattern;
            SystemImageFolderTextBox.Text = systemImageFolderText;

            // Create the directory if it doesn't exist (using the resolved path)
            var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(systemImageFolderText);
            if (!string.IsNullOrEmpty(resolvedPath) && !Directory.Exists(resolvedPath)) // Add null/empty check for resolvedPath
            {
                try
                {
                    Directory.CreateDirectory(resolvedPath);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, $"Error creating system image folder: {resolvedPath}");
                }
            }
        }

        if (!string.IsNullOrEmpty(systemImageFolderText)) return false;

        MessageBoxLibrary.SystemImageFolderCanNotBeEmptyMessageBox();
        return true;
    }

    private bool ValidateSystemFolder(string systemNameText, ref string systemFolderText)
    {
        var defaultPattern = $".\\roms\\{systemNameText}";
        var prefixedDefaultPattern = $"%BASEFOLDER%\\roms\\{systemNameText}";

        if (string.IsNullOrEmpty(systemFolderText) || systemFolderText.Equals(defaultPattern, StringComparison.OrdinalIgnoreCase) || systemFolderText.Equals(prefixedDefaultPattern, StringComparison.OrdinalIgnoreCase))
        {
            systemFolderText = prefixedDefaultPattern;
            SystemFolderTextBox.Text = systemFolderText;

            // Create the directory if it doesn't exist (using the resolved path)
            var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(systemFolderText);
            if (!string.IsNullOrEmpty(resolvedPath) && !Directory.Exists(resolvedPath)) // Add null/empty check for resolvedPath
            {
                try
                {
                    Directory.CreateDirectory(resolvedPath);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, $"Error creating system folder: {resolvedPath}");
                }
            }
        }

        if (!string.IsNullOrEmpty(systemFolderText)) return false;

        MessageBoxLibrary.SystemFolderCanNotBeEmptyMessageBox();
        return true;
    }

    private static bool ValidateSystemName(string systemNameText)
    {
        // First, sanitize the input (though this is primarily handled in SaveSystemButton_Click)
        systemNameText = SanitizeInputSystemName.SanitizeFolderName(systemNameText);

        if (!string.IsNullOrEmpty(systemNameText))
        {
            return false;
        }

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
        TextBox[] parameterTextBoxes =
        [
            Emulator1ParametersTextBox, Emulator2ParametersTextBox,
            Emulator3ParametersTextBox, Emulator4ParametersTextBox,
            Emulator5ParametersTextBox
        ];

        foreach (var paramTextBox in parameterTextBoxes)
        {
            MarkValid(paramTextBox);
        }
    }
}