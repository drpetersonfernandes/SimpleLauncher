using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.GetListOfFiles;

/// <summary>
/// Enumerates game files in a directory, filtering by configured file extensions
/// and skipping restricted folders.
/// </summary>
public class GetListOfFilesService : IGetListOfFilesService
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetListOfFilesService"/> class.
    /// </summary>
    /// <param name="logErrors">The error logger.</param>
    public GetListOfFilesService(ILogger logErrors)
    {
        _logger = logErrors;
    }

    /// <summary>
    /// Gets the list of files in the given directory that match the configured file extensions.
    /// </summary>
    /// <param name="directoryPath">The directory to scan for files.</param>
    /// <param name="fileExtensions">The list of file extensions (without dots) to include.</param>
    /// <param name="disableRecursiveSearch">Whether recursive folder search is disabled.</param>
    /// <param name="groupByFolder">Whether the system groups games by folder.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The list of matching file paths.</returns>
    public Task<IList<string>> GetFilesAsync(string directoryPath, IList<string> fileExtensions, bool disableRecursiveSearch, bool groupByFolder, CancellationToken cancellationToken = default)
    {
        return Task.Run<IList<string>>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    // Expected condition (missing directory): not a bug, keep it out of the bug report service.
                    _logger?.Information($"Directory does not exist: '{directoryPath}'.");
                    return new List<string>();
                }

                var extensionsSet = new HashSet<string>(fileExtensions, StringComparer.OrdinalIgnoreCase);
                var foundFiles = new List<string>();
                var restrictedFolders = new List<string>();

                var doRecurse = !(disableRecursiveSearch && !groupByFolder);
                EnumerateFilesRecursive(directoryPath, extensionsSet, foundFiles, restrictedFolders, doRecurse, cancellationToken);

                if (restrictedFolders.Count > 0)
                {
                    // Expected condition (access denied on some folders during scan): not a bug,
                    // keep it out of the bug report service.
                    _logger?.Information($"Skipped {restrictedFolders.Count} restricted folders during file scan.");
                }

                return foundFiles;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, $"Error scanning directory: {directoryPath}");
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
            _logger?.Error(ex, $"Path too long during enumeration: {path}");
        }
        catch (DirectoryNotFoundException)
        {
            // Directory disappeared during enumeration, skip silently
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger?.Error(ex, $"Unexpected error accessing folder: {path}");
        }
    }
}
