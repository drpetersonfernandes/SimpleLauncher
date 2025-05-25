using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleLauncher.Services;

public abstract class GetFilePaths
{
    private static readonly string LogPath = GetLogPath.Path();

    /// <summary>
    /// Asynchronously retrieves a list of file paths from the specified directory that match the given file extensions.
    /// </summary>
    /// <param name="directoryPath">The path of the directory to search for files.</param>
    /// <param name="fileExtensions">A list of file extensions to filter the search (e.g., "txt", "jpg").</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of file paths that match the specified criteria.</returns>
    public static Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions)
    {
        return Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    return new List<string>(); // Return an empty list
                }

                var foundFiles = new List<string>();

                foreach (var ext in fileExtensions)
                {
                    try
                    {
                        // Construct the search pattern by prepending "*.
                        var searchPattern = $"*.{ext}";
                        foundFiles.AddRange(Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly)); // Added SearchOption.TopDirectoryOnly for consistency
                    }
                    catch (Exception innerEx)
                    {
                        // Notify developer
                        // Log the specific extension that caused the problem
                        // Note: 'ext' is now just the extension, not the pattern
                        var contextMessage = $"Error processing extension '{ext}' in directory '{directoryPath}'.";
                        _ = LogErrors.LogErrorAsync(innerEx, contextMessage);

                        // Continue with the next extension rather than failing the entire operation
                    }
                }

                return foundFiles; // Return the list directly from Task.Run
            }
            catch (Exception ex)
            {
                // Notify developer
                var contextMessage = $"There was an error using the method GetFilesAsync.\n" +
                                     $"Directory path: {directoryPath}";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorFindingGameFilesMessageBox(LogPath);

                return new List<string>(); // Return an empty list
            }
        });
    }

    public static Task<List<string>> FilterFilesAsync(List<string> files, string startLetter)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrEmpty(startLetter))
                return files; // If no startLetter is provided, no filtering is required

            if (startLetter == "#")
            {
                return files.Where(static file => !string.IsNullOrEmpty(file) &&
                                                  file.Length > 0 &&
                                                  char.IsDigit(Path.GetFileName(file)[0])).ToList();
            }

            return files.Where(file => !string.IsNullOrEmpty(file) &&
                                       Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
        });
    }
}