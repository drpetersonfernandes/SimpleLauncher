using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.CheckPaths;

namespace SimpleLauncher.Services.CleanAndDeleteFiles;

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
                    App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, $"Failed to delete file '{longPath}' after {MaxDeleteRetries} retries.");

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
                    // Notify developer
                    App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, $"Failed to delete file '{longPath}' after {MaxDeleteRetries} retries (permissions).");

                    return;
                }

                // Wait before retrying
                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                // Notify developer
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, $"Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{longPath}'. Stopping retries.");

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
                    // Log final failure after retries
                    // Notify developer
                    App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, $"Failed to delete file '{longPath}' after {MaxDeleteRetries} retries.");

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
                    // Notify developer
                    App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, $"Failed to delete file '{longPath}' after {MaxDeleteRetries} retries (permissions).");

                    return;
                }

                // Wait before retrying
                await Task.Delay(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                // Notify developer
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, $"Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{longPath}'. Stopping retries.");

                return;
            }
        }
    }
}
