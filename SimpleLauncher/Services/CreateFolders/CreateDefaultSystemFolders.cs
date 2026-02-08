using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.CreateFolders;

public static class CreateDefaultSystemFolders
{
    public static void CreateFolders(string systemName, string systemFolder, string systemImageFolder)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var additionalFolders = App.ServiceProvider.GetRequiredService<IConfiguration>().GetSection("AdditionalFolders").Get<string[]>();
        var resolvedSystemFolder = CheckPaths.PathHelper.ResolveRelativeToAppDirectory(systemFolder);
        var resolvedSystemImageFolder = CheckPaths.PathHelper.ResolveRelativeToAppDirectory(systemImageFolder);

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
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error creating the primary system folder.");

                    // Notify user
                    MessageBoxLibrary.FolderCreationFailedMessageBox();
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
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error creating the primary image folder.");

                    // Notify user
                    MessageBoxLibrary.FolderCreationFailedMessageBox();
                }
            }

            if (additionalFolders != null)
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
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error creating the {folder} folder.");

                        // Notify user
                        MessageBoxLibrary.FolderCreationFailedMessageBox();
                    }
                }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "The application failed to create the necessary folders for the newly added system.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FolderCreationFailedMessageBox();

            throw;
        }
    }
}