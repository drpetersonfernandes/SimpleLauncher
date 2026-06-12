using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.CheckForRequiredFiles;

public class CheckForRequiredFiles
{
    private readonly IMessageBoxLibraryService _messageBoxLibrary;

    public CheckForRequiredFiles(IMessageBoxLibraryService messageBoxLibrary)
    {
        _messageBoxLibrary = messageBoxLibrary;
    }

    public async Task CheckFiles(IConfiguration configuration, ILogErrors logErrors)
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
            await _messageBoxLibrary.HandleMissingRequiredFilesMessageBox(fileList);
        }
        catch (Exception ex)
        {
            logErrors.LogAndForget(ex, "Failed to check for required files.");
        }
    }
}
