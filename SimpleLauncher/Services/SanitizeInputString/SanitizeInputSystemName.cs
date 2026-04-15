using System.IO;
using System.Text;

namespace SimpleLauncher.Services.SanitizeInputString;

public static class SanitizeInputSystemName
{
    // Windows reserved device names (case-insensitive)
    private static readonly string[] ReservedNames = ["CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"];

    /// <summary>
    /// Validates a system name for invalid characters.
    /// Returns true if the name contains invalid characters.
    /// </summary>
    /// <param name="name">The system name to validate.</param>
    /// <param name="invalidChars">The invalid characters found, if any.</param>
    /// <returns>True if invalid characters are found; otherwise false.</returns>
    public static bool ContainsInvalidCharacters(string name, out char[] invalidChars)
    {
        invalidChars = [];
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var invalidFileNameChars = Path.GetInvalidFileNameChars();
        var foundInvalidChars = new List<char>();

        foreach (var c in name)
        {
            if (invalidFileNameChars.Contains(c))
            {
                foundInvalidChars.Add(c);
            }
        }

        invalidChars = [.. foundInvalidChars];
        return invalidChars.Length > 0;
    }

    /// <summary>
    /// Sanitizes a string to be safe for use as a folder name.
    /// Removes invalid path characters and directory traversal sequences.
    /// </summary>
    /// <param name="name">The potential folder name.</param>
    /// <returns>A sanitized folder name.</returns>
    public static string SanitizeFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "_invalid_empty_name_"; // Return a placeholder if input is empty/whitespace
        }

        // Replace directory traversal sequences first
        var sanitizedName = name.Replace("..", "_"); // Replace ".." with underscore

        // Get invalid characters for file names (which also apply to directory names)
        var invalidChars = Path.GetInvalidFileNameChars();

        // Use StringBuilder for efficient replacements
        var sb = new StringBuilder(sanitizedName);
        foreach (var invalidChar in invalidChars)
        {
            sb.Replace(invalidChar, '_'); // Replace invalid chars with underscore
        }

        sanitizedName = sb.ToString();

        // Trim leading/trailing dots and spaces which can cause issues in some filesystems
        sanitizedName = sanitizedName.Trim('.', ' ');

        // Ensure the name isn't empty *after* sanitization
        if (string.IsNullOrWhiteSpace(sanitizedName))
        {
            // If sanitization resulted in an empty string (e.g., input was just "."),
            // return a placeholder.
            return "_invalid_sanitized_name_";
        }

        // Check for Windows reserved device names
        if (Enumerable.Contains(ReservedNames, sanitizedName, StringComparer.OrdinalIgnoreCase))
        {
            sanitizedName = $"_{sanitizedName}_"; // Prepend and append underscore to make it safe
        }

        return sanitizedName;
    }
}