using System;
using System.IO;
using System.Threading;

namespace SimpleLauncher.Services;

public static class DeleteFiles
{
    private const int MaxDeleteRetries = 5; // Number of times to retry deletion
    private const int DeleteRetryDelayMs = 100; // Delay between retries in milliseconds

    public static void TryDeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

        // Check for and remove the read-only attribute before attempting deletion
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.IsReadOnly)
            {
                fileInfo.IsReadOnly = false;
            }
        }
        catch (Exception ex)
        {
            // If we can't even read/modify the attributes, log and exit
            _ = LogErrors.LogErrorAsync(ex, $"Failed to access or modify file attributes for '{filePath}'.");
            return;
        }

        for (var i = 0; i < MaxDeleteRetries; i++)
        {
            try
            {
                File.Delete(filePath);
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
                    _ = LogErrors.LogErrorAsync(ex, $"Failed to delete file '{filePath}' after {MaxDeleteRetries} retries.");

                    return; // Exit after logging final failure
                }

                // Wait before retrying
                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (UnauthorizedAccessException ex)
            {
                // If this is the last attempt, log final failure
                if (i == MaxDeleteRetries - 1)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, $"Failed to delete file '{filePath}' after {MaxDeleteRetries} retries (permissions).");

                    return;
                }

                // Wait before retrying
                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, $"Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{filePath}'. Stopping retries.");

                return;
            }
        }
    }
}