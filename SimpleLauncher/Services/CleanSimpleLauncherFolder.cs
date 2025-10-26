using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SimpleLauncher.Services;

public static class CleanSimpleLauncherFolder
{
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

    private static readonly string[] DirectoriesToClean =
    [
        Path.Combine(AppDirectory, "temp"),
        Path.Combine(AppDirectory, "temp2"),
        Path.Combine(AppDirectory, "tools", "BatchVerifyCHDFiles"),
        Path.Combine(AppDirectory, "tools", "BatchVerifyCompressedFiles"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "de"),
        Path.Combine(AppDirectory, "cache"),
        Path.Combine(AppDirectory, "x64"),
        Path.Combine(AppDirectory, "x86"),
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "x64"),
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso", "x86"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCHD", "x64"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCHD", "x86"),
        Path.Combine(AppDirectory, "resources"),
        Path.Combine(AppDirectory, "de"),
        Path.Combine(Path.GetTempPath(), "SimpleZipDrive"),
        Path.Combine(Path.GetTempPath(), "SimpleLauncher")
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
        Path.Combine(AppDirectory, "7z_x86.dll"),
        Path.Combine(AppDirectory, "whatsnew.txt"),
        Path.Combine(AppDirectory, "Updater.exe"),

        Path.Combine(AppDirectory, "ControlzEx.dll"),
        Path.Combine(AppDirectory, "Hardcodet.NotifyIcon.Wpf.dll"),
        Path.Combine(AppDirectory, "ICSharpCode.SharpZipLib.dll"),
        Path.Combine(AppDirectory, "MahApps.Metro.dll"),
        Path.Combine(AppDirectory, "Markdig.dll"),
        Path.Combine(AppDirectory, "Markdig.Wpf.dll"),
        Path.Combine(AppDirectory, "MessagePack.Annotations.dll"),
        Path.Combine(AppDirectory, "MessagePack.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Configuration.Abstractions.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Configuration.Binder.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Configuration.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Configuration.FileExtensions.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Configuration.Json.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.DependencyInjection.Abstractions.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.DependencyInjection.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Diagnostics.Abstractions.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Diagnostics.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.FileProviders.Abstractions.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.FileProviders.Physical.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.FileSystemGlobbing.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Http.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Logging.Abstractions.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Logging.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Options.ConfigurationExtensions.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Options.dll"),
        Path.Combine(AppDirectory, "Microsoft.Extensions.Primitives.dll"),
        Path.Combine(AppDirectory, "Microsoft.NET.StringTools.dll"),
        Path.Combine(AppDirectory, "Microsoft.Xaml.Behaviors.dll"),
        Path.Combine(AppDirectory, "SevenZipSharp.dll"),
        Path.Combine(AppDirectory, "SharpDX.DirectInput.dll"),
        Path.Combine(AppDirectory, "SharpDX.dll"),
        Path.Combine(AppDirectory, "SharpDX.XInput.dll"),
        Path.Combine(AppDirectory, "SimpleLauncher.dll"),
        Path.Combine(AppDirectory, "WindowsInput.dll"),
        Path.Combine(AppDirectory, "SimpleLauncher.deps.json"),
        Path.Combine(AppDirectory, "SimpleLauncher.runtimeconfig.json"),
        Path.Combine(AppDirectory, "SevenZipExtractor.dll"),

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
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "7z.dll"),

        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "7z.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "7z.exe"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "BatchConvertToRVZ.deps.json"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "BatchConvertToRVZ.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "BatchConvertToRVZ.pdb"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "BatchConvertToRVZ.runtimeconfig.json"),

        Path.Combine(AppDirectory, "tools", "FindRomCover", "ControlzEx.dll"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "error_user.log"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover.deps.json"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover.dll"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover.pdb"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover.runtimeconfig.json"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "MahApps.Metro.dll"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "Microsoft.Xaml.Behaviors.dll"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "Newtonsoft.Json.dll"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "appsettings.json"),

        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.deps.json"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.dll"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.pdb"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.runtimeconfig.json"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "Microsoft.Extensions.DependencyInjection.Abstractions.dll"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "Microsoft.Extensions.DependencyInjection.dll"),

        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames.deps.json"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames.dll"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames.pdb"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames.runtimeconfig.json"),

        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames.deps.json"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames.dll"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames.pdb"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames.runtimeconfig.json"),

        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames.deps.json"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames.dll"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames.pdb"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames.runtimeconfig.json"),

        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games.deps.json"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games.dll"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games.pdb"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games.runtimeconfig.json")
    ];

    private static readonly string[] FilesToDeleteIfCurrentArchitectureIsX64 =
    [
        Path.Combine(AppDirectory, "7z_arm64.dll"),
        Path.Combine(AppDirectory, "easymode_arm64.xml"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "7z_arm64.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "BatchConvertToCompressedFile_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "7z_arm64.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "BatchConvertToRVZ_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "DolphinTool_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "RomValidator", "RomValidator_arm64.exe"),
        Path.Combine(AppDirectory, "tools", "SimpleZipDrive", "SimpleZipDrive_arm64.exe")
    ];

    private static readonly string[] FilesToDeleteIfCurrentArchitectureIsArm64 =
    [
        Path.Combine(AppDirectory, "7z_x64.dll"),
        Path.Combine(AppDirectory, "easymode.xml"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "7z_x64.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCompressedFile", "BatchConvertToCompressedFile.exe"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "7z_x64.dll"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "BatchConvertToRVZ.exe"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToRVZ", "DolphinTool.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames.exe"),
        Path.Combine(AppDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames.exe"),
        Path.Combine(AppDirectory, "tools", "FindRomCover", "FindRomCover.exe"),
        Path.Combine(AppDirectory, "tools", "RomValidator", "RomValidator.exe"),
        Path.Combine(AppDirectory, "tools", "SimpleZipDrive", "SimpleZipDrive.exe")
    ];

    private static readonly string[] DirectoriesToDeleteIfCurrentArchitectureIsX64 =
    [
        Path.Combine(AppDirectory, "tools", "GameCoverScraper", "arm64")
    ];

    private static readonly string[] DirectoriesToDeleteIfCurrentArchitectureIsArm64 =
    [
        Path.Combine(AppDirectory, "tools", "BatchConvertIsoToXiso"),
        Path.Combine(AppDirectory, "tools", "BatchConvertToCHD"),
        Path.Combine(AppDirectory, "tools", "xbox-iso-vfs"),
        Path.Combine(AppDirectory, "tools", "GameCoverScraper", "x64")
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

        CleanupArchitectureSpecificFiles();
        CleanupArchitectureSpecificFolders();
    }

    private static void CleanupArchitectureSpecificFiles()
    {
        var currentArchitecture = RuntimeInformation.OSArchitecture;

        string[] filesToDelete;

        switch (currentArchitecture)
        {
            case Architecture.X64:
                filesToDelete = FilesToDeleteIfCurrentArchitectureIsX64;
                break;
            case Architecture.Arm64:
                filesToDelete = FilesToDeleteIfCurrentArchitectureIsArm64;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        foreach (var file in filesToDelete)
        {
            DeleteFileSafely(file);
        }
    }

    private static void CleanupArchitectureSpecificFolders()
    {
        var currentArchitecture = RuntimeInformation.OSArchitecture;

        string[] foldersToDelete;

        switch (currentArchitecture)
        {
            case Architecture.X64:
                foldersToDelete = DirectoriesToDeleteIfCurrentArchitectureIsX64;
                break;
            case Architecture.Arm64:
                foldersToDelete = DirectoriesToDeleteIfCurrentArchitectureIsArm64;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        foreach (var folder in foldersToDelete)
        {
            DeleteDirectorySafely(folder);
        }
    }

    private static void DeleteDirectorySafely(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

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