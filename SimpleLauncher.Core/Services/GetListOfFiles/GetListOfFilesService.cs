using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Core.Services.GetListOfFiles;

public class GetListOfFilesService : IGetListOfFilesService
{
    private readonly ILogErrors _logErrors;

    public GetListOfFilesService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions, bool disableRecursiveSearch, bool groupByFolder, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    _logErrors?.LogAndForget(null, $"Directory does not exist: '{directoryPath}'.");
                    return new List<string>();
                }

                var extensionsSet = new HashSet<string>(fileExtensions, StringComparer.OrdinalIgnoreCase);
                var foundFiles = new List<string>();
                var restrictedFolders = new List<string>();

                var doRecurse = !(disableRecursiveSearch && !groupByFolder);
                EnumerateFilesRecursive(directoryPath, extensionsSet, foundFiles, restrictedFolders, doRecurse, cancellationToken);

                if (restrictedFolders.Count > 0)
                {
                    _logErrors?.LogAndForget(null, $"Skipped {restrictedFolders.Count} restricted folders during file scan.");
                }

                return foundFiles;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logErrors?.LogAndForget(ex, $"Error scanning directory: {directoryPath}");
                return new List<string>();
            }
        }, cancellationToken);
    }

    private void EnumerateFilesRecursive(string path, HashSet<string> extensions, List<string> results, List<string> restrictedFolders, bool doRecurse, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        if (!Directory.Exists(path))
            return;

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
        }
        catch (PathTooLongException ex)
        {
            _logErrors?.LogAndForget(ex, $"Path too long during enumeration: {path}");
        }
        catch (DirectoryNotFoundException)
        {
            // Directory disappeared during enumeration, skip silently
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logErrors?.LogAndForget(ex, $"Unexpected error accessing folder: {path}");
        }
    }
}
