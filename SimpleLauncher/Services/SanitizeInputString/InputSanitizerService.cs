using System.Text;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.SanitizeInputString;

/// <summary>
/// Provides input sanitization for folder names and path validation to prevent invalid or dangerous file system operations.
/// </summary>
public class InputSanitizerService : IInputSanitizerService
{
    private static readonly string[] ReservedNames = ["CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"];

    /// <summary>
    /// Checks whether a name contains characters that are invalid for file names.
    /// </summary>
    public bool ContainsInvalidCharacters(string name, out char[] invalidChars)
    {
        invalidChars = [];
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var invalidFileNameChars = Path.GetInvalidFileNameChars();
        var foundInvalidChars = new List<char>();

        foreach (var c in name)
        {
            if (invalidFileNameChars.Contains(c))
                foundInvalidChars.Add(c);
        }

        invalidChars = [.. foundInvalidChars];
        return invalidChars.Length > 0;
    }

    /// <summary>
    /// Checks whether a path contains characters that are invalid for file system paths.
    /// </summary>
    public bool ContainsInvalidPathCharacters(string path, out char[] invalidChars)
    {
        invalidChars = [];
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var invalidPathChars = Path.GetInvalidPathChars();
        var foundInvalidChars = new List<char>();

        foreach (var c in path)
        {
            if (invalidPathChars.Contains(c) && !foundInvalidChars.Contains(c))
                foundInvalidChars.Add(c);
        }

        invalidChars = [.. foundInvalidChars];
        return invalidChars.Length > 0;
    }

    /// <summary>
    /// Sanitizes a string for use as a folder name by replacing invalid characters and directory traversal sequences.
    /// </summary>
    public string SanitizeFolderName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "_invalid_empty_name_";

        var sanitizedName = name.Replace("..", "_");
        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(sanitizedName);

        foreach (var invalidChar in invalidChars)
            sb.Replace(invalidChar, '_');

        sanitizedName = sb.ToString();
        sanitizedName = sanitizedName.Trim('.', ' ');

        if (string.IsNullOrWhiteSpace(sanitizedName))
            return "_invalid_sanitized_name_";

        if (Enumerable.Contains(ReservedNames, sanitizedName, StringComparer.OrdinalIgnoreCase))
        {
            sanitizedName = $"_{sanitizedName}_";
        }

        return sanitizedName;
    }
}
