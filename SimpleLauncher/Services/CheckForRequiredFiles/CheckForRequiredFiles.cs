using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.CheckForRequiredFiles;

public static class CheckForRequiredFiles
{
    public static void CheckFiles(IConfiguration configuration)
    {
        var baseDirectory = AppContext.BaseDirectory;
        var requiredFiles = configuration.GetValue<string[]>("RequiredFiles") ??
        [
            "images\\default.png",
            "images\\systems\\default.png",
            "audio\\click.mp3",
            "audio\\notification.mp3",
            "audio\\shutter.mp3",
            "audio\\trash.mp3",
            "appsettings.json",
            "mame.dat"
        ];
        try
        {
            var missingFiles = requiredFiles
                .Select(f => Path.Combine(baseDirectory, f))
                .Where(static f => !File.Exists(f))
                .ToList();

            if (missingFiles.Count == 0)
            {
                return;
            }

            var fileList = string.Join(Environment.NewLine, missingFiles);
            MessageBoxLibrary.HandleMissingRequiredFilesMessageBox(fileList);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to check for required files.");
        }
    }
}