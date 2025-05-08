using System;
using System.IO;
using System.Linq;

namespace SimpleLauncher.Services;

public static class CheckForRequiredFiles
{
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string GlobalDefaultImagePath = Path.Combine(BaseDirectory, "images", "default.png");
    private static readonly string ClickSoundFile = Path.Combine(BaseDirectory, "audio", "click.mp3");
    private static readonly string ShutterSoundFile = Path.Combine(BaseDirectory, "audio", "shutter.mp3");
    private static readonly string TrashSoundFile = Path.Combine(BaseDirectory, "audio", "trash.mp3");
    private static readonly string AppSettings = Path.Combine(BaseDirectory, "appsettings.json");

    public static void CheckFiles()
    {
        var requiredFiles = new[]
        {
            GlobalDefaultImagePath,
            ClickSoundFile,
            ShutterSoundFile,
            TrashSoundFile,
            AppSettings
        };

        try
        {
            var missingFiles = requiredFiles.Where(static f => !File.Exists(f)).ToList();
            if (missingFiles.Count == 0) return;

            var fileList = string.Join(Environment.NewLine, missingFiles);
            MessageBoxLibrary.HandleMissingRequiredFilesMessageBox(fileList);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Failed to check for required files.");
        }
    }
}