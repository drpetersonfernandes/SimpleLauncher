using System.Diagnostics;
using SimpleLauncher.Core.Services.CheckPaths;

namespace SimpleLauncher.Core.Services.CleanAndDeleteFiles;

public static class DeleteFiles
{
    private const int MaxDeleteRetries = 15;
    private const int DeleteRetryDelayMs = 1000;

    /// <summary>
    /// Synchronous version for backward compatibility. Use TryDeleteFileAsync in async contexts.
    /// </summary>
    public static void TryDeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var longPath = PathHelper.GetLongPath(filePath);

        if (!File.Exists(longPath)) return;

        for (var i = 0; i < MaxDeleteRetries; i++)
        {
            try
            {
                // Remove read-only attribute if needed. This is inside the retry
                // loop because modifying attributes on a locked file will throw —
                // if that happens we want to retry, not exit early.
                var fileInfo = new FileInfo(longPath);
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }

                File.Delete(longPath);
                // If deletion succeeds, return
                return;
            }
            catch (IOException ex) // Catch specific IOException for file locking
            {
                // If this is the last attempt, re-throw or log final failure
                if (i == MaxDeleteRetries - 1)
                {
                    // Log final failure after retries
                    // Notify developer
                    Debug.WriteLine($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries: {ex.Message}");

                    return; // Exit after logging final failure
                }

                // Wait before retrying
                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (UnauthorizedAccessException ex)
            {
                // If the file is Updater.exe and an Updater process is still running,
                // skip silently — the file is locked and will be cleaned up on next launch
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

                // If this is the last attempt, log final failure
                if (i == MaxDeleteRetries - 1)
                {
                    Debug.WriteLine($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries (permissions): {ex.Message}");

                    return;
                }

                // Wait before retrying
                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DeleteFiles] Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{longPath}': {ex.Message}");

                return;
            }
        }
    }

    /// <summary>
    /// Async version for use in async contexts to avoid blocking.
    /// </summary>
    public static async Task TryDeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        var longPath = PathHelper.GetLongPath(filePath);

        if (!File.Exists(longPath)) return;

        for (var i = 0; i < MaxDeleteRetries; i++)
        {
            try
            {
                // Remove read-only attribute if needed. This is inside the retry
                // loop because modifying attributes on a locked file will throw —
                // if that happens we want to retry, not exit early.
                var fileInfo = new FileInfo(longPath);
                if (fileInfo.IsReadOnly)
                {
                    fileInfo.IsReadOnly = false;
                }

                File.Delete(longPath);
                // If deletion succeeds, return
                return;
            }
            catch (IOException ex) // Catch specific IOException for file locking
            {
                // If this is the last attempt, re-throw or log final failure
                if (i == MaxDeleteRetries - 1)
                {
                    Debug.WriteLine($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries: {ex.Message}");

                    return; // Exit after logging final failure
                }

                // Wait before retrying
                await Task.Delay(DeleteRetryDelayMs);
            }
            catch (UnauthorizedAccessException ex)
            {
                // If the file is Updater.exe and an Updater process is still running,
                // skip silently — the file is locked and will be cleaned up on next launch
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

                // If this is the last attempt, log final failure
                if (i == MaxDeleteRetries - 1)
                {
                    Debug.WriteLine($"[DeleteFiles] Failed to delete file '{longPath}' after {MaxDeleteRetries} retries (permissions): {ex.Message}");

                    return;
                }

                // Wait before retrying
                await Task.Delay(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DeleteFiles] Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{longPath}': {ex.Message}");

                return;
            }
        }
    }
}
