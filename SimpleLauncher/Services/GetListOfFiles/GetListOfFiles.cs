#nullable enable
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.GetListOfFiles;

public class GetListOfFilesService : IGetListOfFiles
{
    private readonly ILogErrors _logError;
    private readonly IConfiguration _configuration;

    public GetListOfFilesService(ILogErrors logError, IConfiguration configuration)
    {
        _logError = logError;
        _configuration = configuration;
    }

    public Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions, SystemManager.SystemManager systemManager, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    var contextMessage = $"Directory does not exist: '{directoryPath}'.";
                    _logError.LogAndForget(null, contextMessage);

                    return new List<string>();
                }

                var extensionsSet = new HashSet<string>(fileExtensions, StringComparer.OrdinalIgnoreCase);
                var foundFiles = new List<string>();
                var restrictedFolders = new List<string>();

                var doRecurse = systemManager is not { DisableRecursiveSearch: true, GroupByFolder: false };
                EnumerateFilesRecursive(directoryPath, extensionsSet, foundFiles, restrictedFolders, doRecurse, cancellationToken);

                if (restrictedFolders.Count > 0)
                {
                    NotifyUserOfRestrictedFolders(restrictedFolders);
                }

                return foundFiles;
            }
            catch (OperationCanceledException)
            {
                DebugLogger.Log($"[GetFilesAsync] Search was canceled for directory: {directoryPath}");
                throw;
            }
            catch (Exception ex)
            {
                var contextMessage = $"There was an error using the method GetFilesAsync.\n" +
                                     $"Directory path: {directoryPath}";
                _logError.LogAndForget(ex, contextMessage);

                var logPath = _configuration.GetValue("LogPath", "error_user.log");
                MessageBoxLibrary.ErrorFindingGameFilesMessageBox(CheckPaths.PathHelper.ResolveRelativeToAppDirectory(logPath));

                return new List<string>();
            }
        }, cancellationToken);
    }

    private void EnumerateFilesRecursive(string path, HashSet<string> extensions, List<string> results, List<string> restrictedFolders, bool doRecurse, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (!Directory.Exists(path))
        {
            DebugLogger.Log($"[GetListOfFiles] Directory no longer exists: {path}. Skipping.");
            return;
        }

        try
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                var ext = Path.GetExtension(file).TrimStart('.').ToLowerInvariant();
                if (extensions.Contains(ext))
                {
                    results.Add(file);
                }
            }

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
            restrictedFolders.Add(path);
            DebugLogger.Log($"[GetListOfFiles] Access denied to folder: {path}. Skipping.");
        }
        catch (PathTooLongException ex)
        {
            _logError.LogAndForget(ex, $"Path too long during enumeration: {path}");
        }
        catch (DirectoryNotFoundException)
        {
            DebugLogger.Log($"[GetListOfFiles] Directory disappeared during enumeration: {path}. Skipping.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logError.LogAndForget(ex, $"Unexpected error accessing folder: {path}");
        }
    }

    private static void NotifyUserOfRestrictedFolders(List<string> restrictedFolders)
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;

            DebugLogger.Log("The following folders were skipped due to access restrictions:");
            foreach (var folder in restrictedFolders)
            {
                DebugLogger.Log($" - {folder}");
            }

            var message = (string)Application.Current.TryFindResource("RestrictedFoldersSkipped")
                          ?? "Some folders were skipped due to access restrictions. Check the log for details.";

            mainWindow?.UpdateStatusBarService.UpdateContent(message);

            if (restrictedFolders.Count > 5)
            {
                DebugLogger.Log($"[GetListOfFiles] Warning: {restrictedFolders.Count} restricted folders encountered.");
            }
        });
    }
}
