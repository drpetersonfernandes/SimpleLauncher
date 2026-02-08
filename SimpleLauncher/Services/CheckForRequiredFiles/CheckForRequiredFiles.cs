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
            "images\\system\\default.png",
            "audio\\click.mp3",
            "audio\\notification.mp3",
            "audio\\shutter.mp3",
            "audio\\trash.mp3",
            "appsettings.json",
            "mame.dat"
        ];
        var requiredFilesForX64 = configuration.GetValue<string[]>("RequiredFilesForX64") ??
        [
            "7z_x64.dll"
        ];
        var requiredFilesForArm64 = configuration.GetValue<string[]>("RequiredFilesForArm64") ??
        [
            "7z_arm64.dll"
        ];
        try
        {
            var missingFiles = requiredFiles
                .Select(f => Path.Combine(baseDirectory, f))
                .Where(static f => !File.Exists(f))
                .ToList();

            var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
            var architectureSpecificFiles = architecture switch
            {
                System.Runtime.InteropServices.Architecture.X64 => requiredFilesForX64,
                System.Runtime.InteropServices.Architecture.Arm64 => requiredFilesForArm64,
                _ => []
            };
            missingFiles.AddRange(architectureSpecificFiles
                .Select(f => Path.Combine(baseDirectory, f))
                .Where(static f => !File.Exists(f)));

            if (missingFiles.Count == 0)
            {
                return;
            }

            var fileList = string.Join(Environment.NewLine, missingFiles);
            // Notify user
            MessageBoxLibrary.HandleMissingRequiredFilesMessageBox(fileList);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to check for required files.");
        }
    }
}