using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.GetListOfFiles;

public abstract class GetListOfFiles
{
    public static Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions, SystemManager.SystemManager systemManager, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var logError = App.ServiceProvider.GetRequiredService<ILogErrors>();
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    // Notify developer
                    var contextMessage = $"Directory does not exist: '{directoryPath}'.";
                    logError.LogAndForget(null, contextMessage);

                    return []; // Return an empty list
                }

                var extensionsSet = new HashSet<string>(fileExtensions, StringComparer.OrdinalIgnoreCase);
                var foundFiles = new List<string>();
                var restrictedFolders = new List<string>();

                // Perform safe recursive enumeration
                var doRecurse = !(systemManager.DisableRecursiveSearch && !systemManager.GroupByFolder);
                EnumerateFilesRecursive(directoryPath, extensionsSet, foundFiles, restrictedFolders, doRecurse, cancellationToken);

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
                logError.LogAndForget(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ErrorFindingGameFilesMessageBox(CheckPaths.PathHelper.ResolveRelativeToAppDirectory(App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue("LogPath", "error_user.log")));

                return []; // Return an empty list
            }
        }, cancellationToken);
    }

    private static void EnumerateFilesRecursive(string path, HashSet<string> extensions, List<string> results, List<string> restrictedFolders, bool doRecurse, CancellationToken token)
    {
        var logError = App.ServiceProvider.GetRequiredService<ILogErrors>();
        token.ThrowIfCancellationRequested();

        // Proactive guard against the common race condition where a directory disappears
        // between the parent listing it and the recursive call entering it.
        if (!Directory.Exists(path))
        {
            DebugLogger.Log($"[GetListOfFiles] Directory no longer exists: {path}. Skipping.");
            return;
        }

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
            if (doRecurse)
            {
                foreach (var dir in Directory.EnumerateDirectories(path))
                {
                    EnumerateFilesRecursive(dir, extensions, results, restrictedFolders, doRecurse, token);
                }
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
            logError.LogAndForget(ex, $"Path too long during enumeration: {path}");
        }
        catch (DirectoryNotFoundException)
        {
            // Don't report transient filesystem races as bugs.
            DebugLogger.Log($"[GetListOfFiles] Directory disappeared during enumeration: {path}. Skipping.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logError.LogAndForget(ex, $"Unexpected error accessing folder: {path}");
        }
    }

    private static void NotifyUserOfRestrictedFolders(List<string> restrictedFolders)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
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