using System.Diagnostics;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Core.Services.CleanAndDeleteFiles;

public class DeleteFilesService : IDeleteFilesService
{
    private const int MaxDeleteRetries = 15;
    private const int DeleteRetryDelayMs = 1000;

    private readonly IDebugLogger _debugLogger;

    public DeleteFilesService(IDebugLogger debugLogger)
    {
        _debugLogger = debugLogger;
    }

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
                    _debugLogger.Log($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries: {ex.Message}");
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
                    _debugLogger.Log($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries (permissions): {ex.Message}");
                    return;
                }

                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                _debugLogger.Log($"[DeleteFiles] Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{longPath}': {ex.Message}");
                return;
            }
        }
    }

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
                    _debugLogger.Log($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries: {ex.Message}");
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
                    _debugLogger.Log($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries (permissions): {ex.Message}");
                    return;
                }

                await Task.Delay(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                _debugLogger.Log($"[DeleteFiles] Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{longPath}': {ex.Message}");
                return;
            }
        }
    }

    // Static helper for backward compatibility during migration
    internal static void TryDeleteFileStatic(string filePath)
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
}
