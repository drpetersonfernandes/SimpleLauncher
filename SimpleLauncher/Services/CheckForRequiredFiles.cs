using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services;

public static class CheckForRequiredFiles
{
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string GlobalDefaultImagePath = Path.Combine(BaseDirectory, "images", "default.png");
    private static readonly string SystemDefaultImagePath = Path.Combine(BaseDirectory, "images", "systems", "default.png");
    private static readonly string ClickSoundFile = Path.Combine(BaseDirectory, "audio", "click.mp3");
    private static readonly string NotificationSoundFile = Path.Combine(BaseDirectory, "audio", "notification.mp3");
    private static readonly string ShutterSoundFile = Path.Combine(BaseDirectory, "audio", "shutter.mp3");
    private static readonly string TrashSoundFile = Path.Combine(BaseDirectory, "audio", "trash.mp3");
    private static readonly string AppSettings = Path.Combine(BaseDirectory, "appsettings.json");
    private static readonly string ZipDllX64 = Path.Combine(BaseDirectory, "7z_x64.dll");
    private static readonly string ZipDllArm64 = Path.Combine(BaseDirectory, "7z_arm64.dll");
    private static readonly string MameDat = Path.Combine(BaseDirectory, "mame.dat");

    public static void CheckFiles()
    {
        var requiredFiles = new[]
        {
            GlobalDefaultImagePath,
            SystemDefaultImagePath,
            ClickSoundFile,
            NotificationSoundFile,
            ShutterSoundFile,
            TrashSoundFile,
            AppSettings,
            MameDat
        };

        try
        {
            var missingFiles = requiredFiles.Where(static f => !File.Exists(f)).ToList();

            // Check for architecture-specific ZIP DLL
            var architecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture;
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (architecture)
            {
                case System.Runtime.InteropServices.Architecture.X64:
                {
                    if (!File.Exists(ZipDllX64))
                    {
                        missingFiles.Add(ZipDllX64);
                    }

                    break;
                }
                case System.Runtime.InteropServices.Architecture.Arm64:
                {
                    if (!File.Exists(ZipDllArm64))
                    {
                        missingFiles.Add(ZipDllArm64);
                    }

                    break;
                }
            }

            if (missingFiles.Count == 0) return;

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