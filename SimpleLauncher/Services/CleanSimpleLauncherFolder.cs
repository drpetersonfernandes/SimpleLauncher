using System;
using System.IO;

namespace SimpleLauncher.Services;

public static class CleanSimpleLauncherFolder
{
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

    private static readonly string[] DirectoriesToClean =
    [
        Path.Combine(AppDirectory, "temp"),
        Path.Combine(AppDirectory, "temp2"),
        Path.Combine(Path.GetTempPath(), "SimpleLauncher"),
        Path.Combine(AppDirectory, "tools", "BatchVerifyCHDFiles"),
        Path.Combine(AppDirectory, "tools", "BatchVerifyCompressedFiles"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "de"),
        Path.Combine(AppDirectory, "cache"),
        Path.Combine(AppDirectory, "x64"),
        Path.Combine(AppDirectory, "x86"),
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "x64"),
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "x86")
    ];

    private static readonly string[] FilesToClean =
    [
        Path.Combine(AppDirectory, "update.zip"),
        Path.Combine(AppDirectory, "mame.xml"),
        Path.Combine(AppDirectory, "Updater.deps.json"),
        Path.Combine(AppDirectory, "Updater.dll"),
        Path.Combine(AppDirectory, "Updater.pdb"),
        Path.Combine(AppDirectory, "Updater.runtimeconfig.json"),
        Path.Combine(AppDirectory, "SimpleLauncher.pdb"),

        Path.Combine(AppDirectory, "7z.dll"),

        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "7z.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "7z.exe"),
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "BatchConvertIsoToXiso.deps.json"),
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "BatchConvertIsoToXiso.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "BatchConvertIsoToXiso.pdb"),
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "BatchConvertIsoToXiso.runtimeconfig.json"),

        Path.Combine(AppDirectory, "tools", "BatchConvertToCHD", "7z.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCHD", "7z.exe"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCHD", "BatchConvertToCHD.deps.json"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCHD", "BatchConvertToCHD.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCHD", "BatchConvertToCHD.pdb"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCHD", "BatchConvertToCHD.runtimeconfig.json"),

        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "7z.exe"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "7z_x86.exe"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "BatchConvertToCompressedFile.deps.json"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "BatchConvertToCompressedFile.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "BatchConvertToCompressedFile.pdb"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "BatchConvertToCompressedFile.runtimeconfig.json"),

        Path.Combine(AppDirectory, "tools", "FindRomCover", "ControlzEx.dll"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "error_user.log"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover.deps.json"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover.dll"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover.pdb"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover.runtimeconfig.json"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "MahApps.Metro.dll"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "Microsoft.Xaml.Behaviors.dll"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "Newtonsoft.Json.dll"),

        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.deps.json"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.dll"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.pdb"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.runtimeconfig.json"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "Microsoft.Extensions.DependencyInjection.Abstractions.dll"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "Microsoft.Extensions.DependencyInjection.dll")
    ];

    public static void CleanupTrash()
    {
        // Clean directories
        foreach (var directory in DirectoriesToClean)
        {
            DeleteDirectorySafely(directory);
        }

        // Clean files
        foreach (var file in FilesToClean)
        {
            DeleteFileSafely(file);
        }
    }

    private static void DeleteDirectorySafely(string path)
    {
        if (!Directory.Exists(path)) return;

        try
        {
            Directory.Delete(path, true);
        }
        catch (Exception)
        {
            // Ignore
        }
    }

    private static void DeleteFileSafely(string path)
    {
        if (!File.Exists(path)) return;

        try
        {
            DeleteFiles.TryDeleteFile(path);
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}