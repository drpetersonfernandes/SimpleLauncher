using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.SanitizeInputString;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher;

internal partial class EditSystemWindow
{
    private void MarkInvalid(Control textBox, bool isValid)
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

    private void MarkValid(Control textBox)
    {
        SetTextBoxForeground(textBox, true); // Always valid state
    }

    private void SetTextBoxForeground(Control textBox, bool isValid)
    {
        var baseTheme = _settings.BaseTheme;
        var actualTheme = baseTheme;

        // Resolve "Adaptive" to actual theme based on system setting
        if (baseTheme == "Adaptive")
        {
            actualTheme = IsSystemDarkMode() ? "Dark" : "Light";
        }

        if (actualTheme is "Dark" or "HighContrast" or "Midnight")
        {
            textBox.Foreground = isValid ? Brushes.White : Brushes.Red;
        }
        else
        {
            textBox.Foreground = isValid ? Brushes.Black : Brushes.Red;
        }
    }

    private static bool IsSystemDarkMode()
    {
        const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        const string valueName = "AppsUseLightTheme";

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(keyPath);
            var value = key?.GetValue(valueName);
            // 0 = Dark mode, 1 = Light mode
            return value is 0;
        }
        catch
        {
            // Default to light mode if registry access fails
            return false;
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

    // ReSharper disable once UnusedParameter.Local
    private static void ValidatePaths(string systemNameText, string systemFolderText, string systemImageFolderText, string emulator1LocationText,
        string emulator2LocationText, string emulator3LocationText, string emulator4LocationText,
        string emulator5LocationText, out bool isSystemFolderValid, out bool isSystemImageFolderValid,
        out bool isEmulator1LocationValid, out bool isEmulator2LocationValid, out bool isEmulator3LocationValid,
        out bool isEmulator4LocationValid, out bool isEmulator5LocationValid)
    {
        // CheckPath.IsValidPath is updated to handle %BASEFOLDER% and relative paths
        isSystemFolderValid = string.IsNullOrWhiteSpace(systemFolderText) || CheckPath.IsValidPath(systemFolderText);
        isSystemImageFolderValid = string.IsNullOrWhiteSpace(systemImageFolderText) || CheckPath.IsValidPath(systemImageFolderText);
        // Use stricter validation for emulator paths - must be an executable file, not a directory
        isEmulator1LocationValid = string.IsNullOrWhiteSpace(emulator1LocationText) || CheckPath.IsValidEmulatorExecutablePath(emulator1LocationText);
        isEmulator2LocationValid = string.IsNullOrWhiteSpace(emulator2LocationText) || CheckPath.IsValidEmulatorExecutablePath(emulator2LocationText);
        isEmulator3LocationValid = string.IsNullOrWhiteSpace(emulator3LocationText) || CheckPath.IsValidEmulatorExecutablePath(emulator3LocationText);
        isEmulator4LocationValid = string.IsNullOrWhiteSpace(emulator4LocationText) || CheckPath.IsValidEmulatorExecutablePath(emulator4LocationText);
        isEmulator5LocationValid = string.IsNullOrWhiteSpace(emulator5LocationText) || CheckPath.IsValidEmulatorExecutablePath(emulator5LocationText);
    }

    private async Task<bool> CheckPaths(bool isSystemFolderValid, bool isSystemImageFolderValid, bool isEmulator1LocationValid,
        bool isEmulator2LocationValid, bool isEmulator3LocationValid, bool isEmulator4LocationValid,
        bool isEmulator5LocationValid)
    {
        if (isSystemFolderValid && isSystemImageFolderValid && isEmulator1LocationValid && isEmulator2LocationValid &&
            isEmulator3LocationValid && isEmulator4LocationValid && isEmulator5LocationValid) return false;

        // Notify user
        await _messageBox.PathOrParameterInvalidMessageBox();

        return true;
    }

    private async Task<bool> ValidateEmulator1Name(string emulator1NameText)
    {
        if (!string.IsNullOrEmpty(emulator1NameText)) return false;

        // Notify user
        await _messageBox.Emulator1RequiredMessageBox();

        return true;
    }

    private async Task<bool> ValidateEmulator1Location(string emulator1LocationText, IEnumerable<string> formatsToSearch)
    {
        // If formatsToSearch contains bat, exe, lnk, or url, the emulator path is not required.
        var requiresEmulatorPath = !formatsToSearch.Any(static f =>
            f.Equals("bat", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("exe", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("lnk", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("url", StringComparison.OrdinalIgnoreCase));

        // If an emulator path is required but not provided, show an error.
        if (requiresEmulatorPath && string.IsNullOrWhiteSpace(emulator1LocationText))
        {
            // Notify user
            await _messageBox.Emulator1LocationRequiredMessageBox();
            return true; // Validation failed
        }

        return false; // Validation passed
    }

    private async Task<bool> ValidateEmulator2Location(string emulator2NameText, string emulator2LocationText, IEnumerable<string> formatsToSearch)
    {
        if (string.IsNullOrEmpty(emulator2NameText))
        {
            return false;
        }

        // If formatsToSearch contains bat, exe, lnk, or url, the emulator path is not required.
        var requiresEmulatorPath = !formatsToSearch.Any(static f =>
            f.Equals("bat", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("exe", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("lnk", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("url", StringComparison.OrdinalIgnoreCase));

        // If an emulator path is required but not provided, show an error.
        if (requiresEmulatorPath && string.IsNullOrWhiteSpace(emulator2LocationText))
        {
            // Notify user
            await _messageBox.Emulator2LocationRequiredMessageBox();
            return true; // Validation failed
        }

        return false; // Validation passed
    }

    private async Task<bool> ValidateEmulator3Location(string emulator3NameText, string emulator3LocationText, IEnumerable<string> formatsToSearch)
    {
        if (string.IsNullOrEmpty(emulator3NameText))
        {
            return false;
        }

        // If formatsToSearch contains bat, exe, lnk, or url, the emulator path is not required.
        var requiresEmulatorPath = !formatsToSearch.Any(static f =>
            f.Equals("bat", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("exe", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("lnk", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("url", StringComparison.OrdinalIgnoreCase));

        // If an emulator path is required but not provided, show an error.
        if (requiresEmulatorPath && string.IsNullOrWhiteSpace(emulator3LocationText))
        {
            // Notify user
            await _messageBox.Emulator3LocationRequiredMessageBox();
            return true; // Validation failed
        }

        return false; // Validation passed
    }

    private async Task<bool> ValidateEmulator4Location(string emulator4NameText, string emulator4LocationText, IEnumerable<string> formatsToSearch)
    {
        if (string.IsNullOrEmpty(emulator4NameText))
        {
            return false;
        }

        // If formatsToSearch contains bat, exe, lnk, or url, the emulator path is not required.
        var requiresEmulatorPath = !formatsToSearch.Any(static f =>
            f.Equals("bat", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("exe", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("lnk", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("url", StringComparison.OrdinalIgnoreCase));

        // If an emulator path is required but not provided, show an error.
        if (requiresEmulatorPath && string.IsNullOrWhiteSpace(emulator4LocationText))
        {
            // Notify user
            await _messageBox.Emulator4LocationRequiredMessageBox();
            return true; // Validation failed
        }

        return false; // Validation passed
    }

    private async Task<bool> ValidateEmulator5Location(string emulator5NameText, string emulator5LocationText, IEnumerable<string> formatsToSearch)
    {
        if (string.IsNullOrEmpty(emulator5NameText))
        {
            return false;
        }

        // If formatsToSearch contains bat, exe, lnk, or url, the emulator path is not required.
        var requiresEmulatorPath = !formatsToSearch.Any(static f =>
            f.Equals("bat", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("exe", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("lnk", StringComparison.OrdinalIgnoreCase) ||
            f.Equals("url", StringComparison.OrdinalIgnoreCase));

        // If an emulator path is required but not provided, show an error.
        if (requiresEmulatorPath && string.IsNullOrWhiteSpace(emulator5LocationText))
        {
            // Notify user
            await _messageBox.Emulator5LocationRequiredMessageBox();
            return true; // Validation failed
        }

        return false; // Validation passed
    }

    private async Task<(bool IsFailed, List<string> Formats)> ValidateFormatToLaunch(string formatToLaunchText, bool extractFileBeforeLaunch)
    {
        var formatsToLaunch = formatToLaunchText.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(static format => format.Trim())
            .Where(static format => !string.IsNullOrEmpty(format))
            .ToList();

        // When extractFileBeforeLaunch is true AND formatsToLaunch is empty, that's invalid
        // ReSharper disable once InvertIf
        if (extractFileBeforeLaunch && formatsToLaunch.Count == 0)
        {
            // Notify user
            await _messageBox.ExtensionToLaunchIsRequiredMessageBox();
            return (true, formatsToLaunch); // Return true to indicate validation failed
        }

        return (false, formatsToLaunch); // Return false to indicate validation passed
    }

    private async Task<(bool IsFailed, List<string> Formats)> ValidateFormatToSearch(string formatToSearchText, bool extractFileBeforeLaunch)
    {
        var formatsToSearch = formatToSearchText.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(static format => format.Trim())
            .Where(static format => !string.IsNullOrEmpty(format))
            .ToList();

        if (formatsToSearch.Count == 0)
        {
            // Notify user
            await _messageBox.ExtensionToSearchIsRequiredMessageBox();
            return (true, formatsToSearch);
        }

        // When extractFileBeforeLaunch is true, ALL formats must be zip, rar, or 7z
        // ReSharper disable once InvertIf
        if (extractFileBeforeLaunch && !formatsToSearch.All(static f => f is "zip" or "7z" or "rar"))
        {
            // Notify user
            await _messageBox.FileMustBeCompressedMessageBox();
            return (true, formatsToSearch);
        }

        return (false, formatsToSearch);
    }

    private async Task<(bool IsFailed, string FolderText)> ValidateSystemImageFolder(string systemNameText, string systemImageFolderText)
    {
        var defaultPattern = Path.Combine(".", "images", systemNameText);
        var prefixedDefaultPattern = Path.Combine("%BASEFOLDER%", "images", systemNameText);

        if (string.IsNullOrEmpty(systemImageFolderText) || systemImageFolderText.Equals(defaultPattern, StringComparison.OrdinalIgnoreCase) || systemImageFolderText.Equals(prefixedDefaultPattern, StringComparison.OrdinalIgnoreCase))
        {
            systemImageFolderText = prefixedDefaultPattern;
            SystemImageFolderTextBox.Text = systemImageFolderText;
        }

        if (string.IsNullOrEmpty(systemImageFolderText))
        {
            await _messageBox.SystemImageFolderCanNotBeEmptyMessageBox();
            return (true, systemImageFolderText);
        }

        // Auto-create the image folder if it doesn't exist
        var resolvedImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemImageFolderText);
        if (!string.IsNullOrEmpty(resolvedImageFolder) && !Directory.Exists(resolvedImageFolder))
        {
            // Check for invalid path characters before attempting directory creation
            if (SanitizeInputSystemName.ContainsInvalidPathCharacters(resolvedImageFolder, out var invalidChars))
            {
                var invalidCharsStr = string.Join(", ", invalidChars.Select(static c => $"'{c}'"));
                await _messageBox.InvalidFolderCharactersMessageBox(invalidCharsStr);
                return (true, systemImageFolderText);
            }

            try
            {
                Directory.CreateDirectory(resolvedImageFolder);
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, $"Error creating the system image folder: {resolvedImageFolder}");
                await _messageBox.FolderCreationFailedMessageBox();
                return (true, systemImageFolderText);
            }
        }

        return (false, systemImageFolderText);
    }

    private async Task<(bool IsFailed, string FolderText)> ValidateSystemFolder(string systemNameText, string systemFolderText)
    {
        var defaultPattern = Path.Combine(".", "roms", systemNameText);
        var prefixedDefaultPattern = Path.Combine("%BASEFOLDER%", "roms", systemNameText);

        if (string.IsNullOrEmpty(systemFolderText) || systemFolderText.Equals(defaultPattern, StringComparison.OrdinalIgnoreCase) || systemFolderText.Equals(prefixedDefaultPattern, StringComparison.OrdinalIgnoreCase))
        {
            systemFolderText = prefixedDefaultPattern;
            SystemFolderTextBox.Text = systemFolderText;
        }

        if (string.IsNullOrEmpty(systemFolderText))
        {
            await _messageBox.SystemFolderCanNotBeEmptyMessageBox();
            return (true, systemFolderText);
        }

        // Auto-create the system folder if it doesn't exist
        var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(systemFolderText);
        if (!string.IsNullOrEmpty(resolvedSystemFolder) && !Directory.Exists(resolvedSystemFolder))
        {
            // Check for invalid path characters before attempting directory creation
            if (SanitizeInputSystemName.ContainsInvalidPathCharacters(resolvedSystemFolder, out var invalidChars))
            {
                var invalidCharsStr = string.Join(", ", invalidChars.Select(static c => $"'{c}'"));
                await _messageBox.InvalidFolderCharactersMessageBox(invalidCharsStr);
                return (true, systemFolderText);
            }

            try
            {
                Directory.CreateDirectory(resolvedSystemFolder);
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, $"Error creating the system folder: {resolvedSystemFolder}");
                await _messageBox.FolderCreationFailedMessageBox();
                return (true, systemFolderText);
            }
        }

        return (false, systemFolderText);
    }

    private async Task<bool> ValidateSystemName(string systemNameText)
    {
        // First, sanitize the input (though this is primarily handled in SaveSystemButtonClickAsync)
        systemNameText = SanitizeInputSystemName.SanitizeFolderName(systemNameText);

        if (!string.IsNullOrEmpty(systemNameText))
        {
            return false;
        }

        // Notify user
        await _messageBox.SystemNameCanNotBeEmptyMessageBox();

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