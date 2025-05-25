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
                // Notify developer
                // Log the attempt and the reason (file locked)
                // _ = LogErrors.LogErrorAsync(ex, $"Attempt {i + 1}/{MaxDeleteRetries}: Failed to delete file '{filePath}' due to lock.");

                // If this is the last attempt, re-throw or log final failure
                if (i == MaxDeleteRetries - 1)
                {
                    // Log final failure after retries
                    _ = LogErrors.LogErrorAsync(ex, $"Failed to delete file '{filePath}' after {MaxDeleteRetries} retries.");
                    // Optionally, you could re-throw the exception here if cleanup failure is critical,
                    // but the original code ignored it, so we'll stick to ignoring after logging.
                    return; // Exit after logging final failure
                }

                // Wait before retrying
                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (UnauthorizedAccessException ex) // Catch UnauthorizedAccessException
            {
                // Notify developer
                 // Log the attempt and the reason (permissions)
                 // _ = LogErrors.LogErrorAsync(ex, $"Attempt {i + 1}/{MaxDeleteRetries}: Failed to delete file '{filePath}' due to permissions.");

                 // If this is the last attempt, log final failure
                 if (i == MaxDeleteRetries - 1)
                 {
                     _ = LogErrors.LogErrorAsync(ex, $"Failed to delete file '{filePath}' after {MaxDeleteRetries} retries (permissions).");
                     return; // Exit after logging final failure
                 }

                 // Wait before retrying
                 Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (Exception ex) // Catch any other unexpected exceptions
            {
                // Log the unexpected error and stop retrying
                _ = LogErrors.LogErrorAsync(ex, $"Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{filePath}'. Stopping retries.");
                return; // Exit on unexpected error
            }
        }
    }
}