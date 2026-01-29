using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.CleanAndDeleteFiles;

public static class DeleteFiles
{
    private const int MaxDeleteRetries = 5;
    private const int DeleteRetryDelayMs = 100;

    public static void TryDeleteFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;

        // Prepend long path prefix if not already present, assuming absolute path
        var longPath = filePath.StartsWith(@"\\?\", StringComparison.Ordinal) ? filePath : @"\\?\" + filePath;

        if (!File.Exists(longPath)) return;

        // Check for and remove the read-only attribute before attempting deletion
        try
        {
            var fileInfo = new FileInfo(longPath);
            if (fileInfo.IsReadOnly)
            {
                fileInfo.IsReadOnly = false;
            }
        }
        catch (Exception ex)
        {
            // If we can't even read/modify the attributes, log and exit
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to access or modify file attributes for '{longPath}'.");
            return;
        }

        for (var i = 0; i < MaxDeleteRetries; i++)
        {
            try
            {
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
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to delete file '{longPath}' after {MaxDeleteRetries} retries.");

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
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to delete file '{longPath}' after {MaxDeleteRetries} retries (permissions).");

                    return;
                }

                // Wait before retrying
                Thread.Sleep(DeleteRetryDelayMs);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Attempt {i + 1}/{MaxDeleteRetries}: Unexpected error deleting file '{longPath}'. Stopping retries.");

                return;
            }
        }
    }
}