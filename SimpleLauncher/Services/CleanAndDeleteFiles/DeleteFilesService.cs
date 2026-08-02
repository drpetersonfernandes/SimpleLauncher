using System.Diagnostics;
using SimpleLauncher.Services.CheckPaths;

namespace SimpleLauncher.Services.CleanAndDeleteFiles;

using Interfaces;

/// <summary>
/// Provides file deletion with retry logic and permission handling.
/// </summary>
public class DeleteFilesService : IDeleteFilesService
{
    private const int MaxDeleteRetries = 15;
    private const int DeleteRetryDelayMs = 1000;

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteFilesService"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record debug information.</param>
    public DeleteFilesService(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Attempts to delete a file synchronously with retry logic for locked files.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    public void TryDeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var longPath = PathHelper.GetLongPath(filePath);

        if (!File.Exists(longPath)) return;

        for (var i = 0; i < MaxDeleteRetries; i++)
        {
            try
            {
                var fileInfo = new FileInfo(longPath);
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }

                File.Delete(longPath);
                return;
            }
            catch (IOException ex)
            {
                if (i == MaxDeleteRetries - 1)
                {
                    _logger.Debug($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries: {ex.Message}");
                    return;
                }

                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (UnauthorizedAccessException ex)
            {
                if (Path.GetFileName(filePath).Equals("Updater.exe", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (Process.GetProcessesByName("Updater").Length != 0)
                        {
                            return;
                        }
                    }
                    catch
                    {
                        // Process check failed, proceed with normal retry logic
                    }
                }

                if (i == MaxDeleteRetries - 1)
                {
                    _logger.Debug($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries (permissions): {ex.Message}");
                    return;
                }

                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                _logger.Debug($"[DeleteFiles] Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{longPath}': {ex.Message}");
                return;
            }
        }
    }

    /// <summary>
    /// Attempts to delete a file asynchronously with retry logic for locked files.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    public async Task TryDeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var longPath = PathHelper.GetLongPath(filePath);

        if (!File.Exists(longPath)) return;

        for (var i = 0; i < MaxDeleteRetries; i++)
        {
            try
            {
                var fileInfo = new FileInfo(longPath);
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }

                File.Delete(longPath);
                return;
            }
            catch (IOException ex)
            {
                if (i == MaxDeleteRetries - 1)
                {
                    _logger.Debug($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries: {ex.Message}");
                    return;
                }

                await Task.Delay(DeleteRetryDelayMs);
            }
            catch (UnauthorizedAccessException ex)
            {
                if (Path.GetFileName(filePath).Equals("Updater.exe", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (Process.GetProcessesByName("Updater").Length != 0)
                        {
                            return;
                        }
                    }
                    catch
                    {
                        // Process check failed, proceed with normal retry logic
                    }
                }

                if (i == MaxDeleteRetries - 1)
                {
                    _logger.Debug($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries (permissions): {ex.Message}");
                    return;
                }

                await Task.Delay(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                _logger.Debug($"[DeleteFiles] Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{longPath}': {ex.Message}");
                return;
            }
        }
    }

    /// <summary>
    /// Static helper for backward compatibility that attempts to delete a file with retry logic.
    /// </summary>
    /// <param name="filePath">The path of the file to delete.</param>
    private static void TryDeleteFileStatic(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var longPath = PathHelper.GetLongPath(filePath);

        if (!File.Exists(longPath)) return;

        for (var i = 0; i < MaxDeleteRetries; i++)
        {
            try
            {
                var fileInfo = new FileInfo(longPath);
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }

                File.Delete(longPath);
                return;
            }
            catch (IOException)
            {
                if (i == MaxDeleteRetries - 1) return;

                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (UnauthorizedAccessException)
            {
                if (Path.GetFileName(filePath).Equals("Updater.exe", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (Process.GetProcessesByName("Updater").Length != 0) return;
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (i == MaxDeleteRetries - 1) return;

                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch
            {
                return;
            }
        }
    }

    /// <summary>
    /// Attempts to delete the given directory recursively, ignoring failures.
    /// </summary>
    /// <param name="directoryPath">The path of the directory to delete.</param>
    public void TryDeleteDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            return;

        try
        {
            Directory.Delete(directoryPath, true);
        }
        catch (Exception ex)
        {
            _logger.Debug($"[DeleteFiles] Failed to delete directory '{directoryPath}': {ex.Message}");
        }
    }
}
