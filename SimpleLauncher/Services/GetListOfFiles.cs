using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SimpleLauncher.Services;

public abstract class GetListOfFiles
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
                    // Notify developer
                    var contextMessage = $"Directory does not exist: '{directoryPath}'.";
                    _ = LogErrors.LogErrorAsync(null, contextMessage);

                    return new List<string>(); // Return an empty list
                }

                var foundFiles = new List<string>();

                foreach (var ext in fileExtensions)
                {
                    try
                    {
                        // Construct the search pattern by prepending "*.
                        var searchPattern = $"*.{ext}";
                        foundFiles.AddRange(Directory.EnumerateFiles(directoryPath, searchPattern, SearchOption.AllDirectories));
                    }
                    catch (DirectoryNotFoundException dirEx)
                    {
                        // Notify developer
                        var contextMessage = $"Directory not found while processing extension '{ext}' in directory '{directoryPath}'.";
                        _ = LogErrors.LogErrorAsync(dirEx, contextMessage);

                        break; // Exit the loop since the directory is gone
                    }
                    catch (UnauthorizedAccessException authEx)
                    {
                        // Notify developer
                        var contextMessage = $"Access denied while processing extension '{ext}' in directory '{directoryPath}'.";
                        _ = LogErrors.LogErrorAsync(authEx, contextMessage);

                        // Continue with the next extension
                    }
                    catch (PathTooLongException pathEx)
                    {
                        // Notify developer
                        var contextMessage = $"Path too long while processing extension '{ext}' in directory '{directoryPath}'.";
                        _ = LogErrors.LogErrorAsync(pathEx, contextMessage);

                        // Continue with the next extension
                    }
                    catch (Exception innerEx)
                    {
                        // Notify developer
                        var contextMessage = $"Error processing extension '{ext}' in directory '{directoryPath}'.";
                        _ = LogErrors.LogErrorAsync(innerEx, contextMessage);

                        // Continue with the next extension
                    }
                }

                return foundFiles; // Return the full list of found files
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
}