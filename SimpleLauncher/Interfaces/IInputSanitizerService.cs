namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides input sanitization for folder names and path validation to prevent invalid or dangerous file system operations.
/// </summary>
public interface IInputSanitizerService
{
    /// <summary>
    /// Checks whether a name contains characters that are invalid for file names.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <param name="invalidChars">The array of invalid characters found, if any.</param>
    /// <returns>True if the name contains invalid characters; otherwise, false.</returns>
    bool ContainsInvalidCharacters(string name, out char[] invalidChars);

    /// <summary>
    /// Checks whether a path contains characters that are invalid for file system paths.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="invalidChars">The array of invalid characters found, if any.</param>
    /// <returns>True if the path contains invalid characters; otherwise, false.</returns>
    bool ContainsInvalidPathCharacters(string path, out char[] invalidChars);

    /// <summary>
    /// Sanitizes a string for use as a folder name by replacing invalid characters and directory traversal sequences.
    /// </summary>
    /// <param name="name">The folder name to sanitize.</param>
    /// <returns>The sanitized folder name.</returns>
    string SanitizeFolderName(string name);
}
