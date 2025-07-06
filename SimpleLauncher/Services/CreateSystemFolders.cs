using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class CreateSystemFolders
{
    public static void CreateFolders(string systemName, string systemFolder, string systemImageFolder)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var additionalFolders = GetAdditionalFolders.GetFolders();
        var resolvedSystemFolder = PathHelper.ResolveRelativeToAppDirectory(systemFolder);
        var resolvedSystemImageFolder = PathHelper.ResolveRelativeToAppDirectory(systemImageFolder);

        try
        {
            if (!string.IsNullOrEmpty(resolvedSystemFolder) && !Directory.Exists(resolvedSystemFolder))
            {
                try
                {
                    Directory.CreateDirectory(resolvedSystemFolder);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, "Error creating the primary system folder.");
                }
            }

            if (!string.IsNullOrEmpty(resolvedSystemImageFolder) && !Directory.Exists(resolvedSystemImageFolder))
            {
                try
                {
                    Directory.CreateDirectory(resolvedSystemImageFolder);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, "Error creating the primary image folder.");
                }
            }

            foreach (var folder in additionalFolders)
            {
                var folderPath = Path.Combine(baseDirectory, folder, systemName);
                if (Directory.Exists(folderPath)) continue;

                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, $"Error creating the {folder} folder.");
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "The application failed to create the necessary folders for the newly added system.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FolderCreationFailedMessageBox();

            throw;
        }
    }
}