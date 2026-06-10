using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.CreateFolders;

public static class CreateDefaultSystemFolders
{
    public static async Task CreateFolders(string systemName, string systemFolder, string systemImageFolder, IConfiguration configuration, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var additionalFolders = configuration.GetValue<string[]>("AdditionalFolders") ??
        [
            "roms", "images", "title_snapshots", "gameplay_snapshots", "videos", "manuals", "walkthrough", "cabinets", "carts", "flyers", "pcbs"
        ];
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
                    logErrors.LogAndForget(ex, "Error creating the primary system folder.");

                    // Notify user
                    await messageBox.FolderCreationFailedMessageBox();
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
                    logErrors.LogAndForget(ex, "Error creating the primary image folder.");

                    // Notify user
                    await messageBox.FolderCreationFailedMessageBox();
                }
            }

            foreach (var folder in additionalFolders)
            {
                var folderPath = Path.Combine(baseDirectory, folder, systemName);
                if (Directory.Exists(folderPath))
                {
                    continue;
                }

                try
                {
                    Directory.CreateDirectory(folderPath);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    logErrors.LogAndForget(ex, $"Error creating the {folder} folder.");

                    // Notify user
                    await messageBox.FolderCreationFailedMessageBox();
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "The application failed to create the necessary folders for the newly added system.";
            logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await messageBox.FolderCreationFailedMessageBox();

            throw;
        }
    }
}