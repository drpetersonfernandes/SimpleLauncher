using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.LoadAppSettings;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.GetListOfFiles;

public abstract class GetListOfFiles
{
    private static readonly string LogPath = GetLogPath.Path();

    /// <summary>
    /// Asynchronously retrieves a list of file paths from the specified directory that match the given file extensions.
    /// </summary>
    /// <param name="directoryPath">The path of the directory to search for files.</param>
    /// <param name="fileExtensions">A list of file extensions to filter the search (e.g., "txt", "jpg").</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of file paths that match the specified criteria.</returns>
    public static Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    // Notify developer
                    var contextMessage = $"Directory does not exist: '{directoryPath}'.";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                    return []; // Return an empty list
                }

                var extensionsSet = new HashSet<string>(fileExtensions, StringComparer.OrdinalIgnoreCase);
                var foundFiles = new List<string>();
                var restrictedFolders = new List<string>();

                // Perform safe recursive enumeration
                EnumerateFilesRecursive(directoryPath, extensionsSet, foundFiles, restrictedFolders, cancellationToken);

                // Inform user if restricted folders were encountered
                if (restrictedFolders.Count > 0)
                {
                    NotifyUserOfRestrictedFolders(restrictedFolders);
                }

                return foundFiles; // Return the full list of found files
            }
            catch (OperationCanceledException)
            {
                DebugLogger.Log($"[GetFilesAsync] Search was canceled for directory: {directoryPath}");
                throw; // Re-throw the exception so the caller knows the operation was canceled.
            }
            catch (Exception ex)
            {
                // Notify developer
                var contextMessage = $"There was an error using the method GetFilesAsync.\n" +
                                     $"Directory path: {directoryPath}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorFindingGameFilesMessageBox(LogPath);

                return new List<string>(); // Return an empty list
            }
        }, cancellationToken);
    }

    private static void EnumerateFilesRecursive(string path, HashSet<string> extensions, List<string> results, List<string> restrictedFolders, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        try
        {
            // 1. Get files in the current directory and filter by extension
            foreach (var file in Directory.EnumerateFiles(path))
            {
                var ext = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();
                if (extensions.Contains(ext))
                {
                    results.Add(file);
                }
            }

            // 2. Get subdirectories and recurse
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                EnumerateFilesRecursive(dir, extensions, results, restrictedFolders, token);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip this folder and track it to inform the user later
            restrictedFolders.Add(path);
            DebugLogger.Log($"[GetListOfFiles] Access denied to folder: {path}. Skipping.");
        }
        catch (PathTooLongException ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Path too long during enumeration: {path}");
        }
        catch (DirectoryNotFoundException ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Directory disappeared during enumeration: {path}");
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Unexpected error accessing folder: {path}");
        }
    }

    private static void NotifyUserOfRestrictedFolders(List<string> restrictedFolders)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            // Log detailed list to Debug Logger
            DebugLogger.Log("The following folders were skipped due to access restrictions:");
            foreach (var folder in restrictedFolders)
            {
                DebugLogger.Log($" - {folder}");
            }

            // Update Status Bar with a summary
            var message = (string)Application.Current.TryFindResource("RestrictedFoldersSkipped")
                          ?? "Some folders were skipped due to access restrictions. Check the log for details.";

            UpdateStatusBar.UpdateStatusBar.UpdateContent(message, mainWindow);

            // Optionally show a one-time message box if many folders are restricted
            if (restrictedFolders.Count > 5)
            {
                DebugLogger.Log($"[GetListOfFiles] Warning: {restrictedFolders.Count} restricted folders encountered.");
            }
        });
    }
}