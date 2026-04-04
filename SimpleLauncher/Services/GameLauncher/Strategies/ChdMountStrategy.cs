using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.MountFiles;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

/// <summary>
/// Strategy for mounting CHD files and launching them with compatible emulators.
/// </summary>
public class ChdMountStrategy : ILaunchStrategy
{
    private readonly IConfiguration _configuration;

    private bool _is4Do;
    private bool _isBlastem;
    private bool _isCxbxReloaded;
    private bool _isGenesisPlusGx;
    private bool _isGens;
    private bool _isMednafen;
    private bool _isPcsxRedux;
    private bool _isPicoDrive;
    private bool _isRetroArch;
    private bool _isRpcs3;
    private bool _isXemu;
    private bool _isXenia;
    private bool _isYabause;

    public ChdMountStrategy(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int Priority => 10;

    public bool IsMatch(LaunchContext context)
    {
        var isChd = Path.GetExtension(context.ResolvedFilePath).Equals(".chd", StringComparison.OrdinalIgnoreCase);
        if (!isChd) return false;

        ResolveEmulatorFlags(context);
        return _isGenesisPlusGx || _is4Do || _isBlastem || _isCxbxReloaded || _isGens || _isMednafen || _isPcsxRedux || _isPicoDrive || _isRetroArch || _isRpcs3 || _isXemu || _isXenia || _isYabause;
    }

    private void ResolveEmulatorFlags(LaunchContext context)
    {
        _is4Do = context.EmulatorName.Contains("4do", StringComparison.OrdinalIgnoreCase) ||
                 (context.EmulatorManager?.EmulatorLocation?.Contains("4do.exe", StringComparison.OrdinalIgnoreCase) ?? false);

        _isBlastem = context.EmulatorName.Contains("blastem", StringComparison.OrdinalIgnoreCase) ||
                     (context.EmulatorManager?.EmulatorLocation?.Contains("blastem.exe", StringComparison.OrdinalIgnoreCase) ?? false);

        _isCxbxReloaded = context.EmulatorName.Contains("Cxbx", StringComparison.OrdinalIgnoreCase) ||
                          (context.EmulatorManager?.EmulatorLocation?.Contains("cxbx", StringComparison.OrdinalIgnoreCase) ?? false);

        _isGenesisPlusGx = context.EmulatorName.Contains("genesis plux gx", StringComparison.OrdinalIgnoreCase) ||
                           (context.EmulatorManager?.EmulatorLocation?.Contains("gen_sdl.exe", StringComparison.OrdinalIgnoreCase) ?? false);

        _isGens = context.EmulatorName.Contains("Gens", StringComparison.OrdinalIgnoreCase) ||
                  (context.EmulatorManager?.EmulatorLocation?.Contains("gens.exe", StringComparison.OrdinalIgnoreCase) ?? false);

        _isMednafen = context.EmulatorName.Contains("Mednafen", StringComparison.OrdinalIgnoreCase) ||
                      (context.EmulatorManager?.EmulatorLocation?.Contains("mednafen", StringComparison.OrdinalIgnoreCase) ?? false);

        _isPcsxRedux = context.EmulatorName.Contains("PCSX-Redux", StringComparison.OrdinalIgnoreCase) ||
                       context.EmulatorName.Contains("PCSX Redux", StringComparison.OrdinalIgnoreCase) ||
                       (context.EmulatorManager?.EmulatorLocation?.Contains("pcsx-redux", StringComparison.OrdinalIgnoreCase) ?? false);

        _isPicoDrive = context.EmulatorName.Contains("PicoDrive", StringComparison.OrdinalIgnoreCase) ||
                       context.EmulatorName.Contains("Pico Drive", StringComparison.OrdinalIgnoreCase) ||
                       (context.EmulatorManager?.EmulatorLocation?.Contains("PicoDrive.exe", StringComparison.OrdinalIgnoreCase) ?? false);

        _isRetroArch = context.EmulatorName.Contains("retroarch", StringComparison.OrdinalIgnoreCase) ||
                       (context.EmulatorManager?.EmulatorLocation?.Contains("retroarch", StringComparison.OrdinalIgnoreCase) ?? false);

        _isRpcs3 = context.EmulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) ||
                   (context.EmulatorManager?.EmulatorLocation?.Contains("rpcs3", StringComparison.OrdinalIgnoreCase) ?? false);

        _isXemu = context.EmulatorName.Contains("Xemu", StringComparison.OrdinalIgnoreCase) ||
                  (context.EmulatorManager?.EmulatorLocation?.Contains("xemu", StringComparison.OrdinalIgnoreCase) ?? false);

        _isXenia = context.EmulatorName.Contains("Xenia", StringComparison.OrdinalIgnoreCase) ||
                   (context.EmulatorManager?.EmulatorLocation?.Contains("xenia", StringComparison.OrdinalIgnoreCase) ?? false);

        _isYabause = context.EmulatorName.Contains("Yabause", StringComparison.OrdinalIgnoreCase) ||
                     (context.EmulatorManager?.EmulatorLocation?.Contains("yabause.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        ResolveEmulatorFlags(context);

        var logPath = CheckPaths.PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");

        // Get the console index for CHDMounter based on system and emulator
        var consoleIndex = MountChdFiles.GetConsoleIndexFromSystemName(context.SystemName, context.EmulatorName);

        await using var mountedDrive = await MountChdFiles.MountAsync(context.ResolvedFilePath, logPath, consoleIndex);

        if (!mountedDrive.IsMounted)
        {
            // Mount failed - error message already shown by MountChdFiles
            return;
        }

        // Determine the correct game file to launch based on the emulator
        string gameFilePath;

        if (_isRpcs3)
        {
            // RPCS3 needs the path to EBOOT.BIN
            gameFilePath = FindEbootBin.FindEbootBinRecursive(mountedDrive.MountedPath);
        }
        else if (_isXenia)
        {
            // Xenia needs the path to default.xex
            gameFilePath = FindDefaultXex.Find(mountedDrive.MountedPath);
        }
        else if (_isXemu)
        {
            // Xemu needs the path to image.iso
            gameFilePath = FindImageIso.Find(mountedDrive.MountedPath);
        }
        else if (_isCxbxReloaded)
        {
            // Cxbx-Reloaded needs the path to default.xbe
            gameFilePath = FindDefaultXbe.Find(mountedDrive.MountedPath);
        }
        else if (_isGens)
        {
            // Path to a .bin file
            gameFilePath = FindBinFile.Find(mountedDrive.MountedPath);
        }
        else
        {
            // Path to a .cue file
            gameFilePath = FindCueFile.Find(mountedDrive.MountedPath);
        }

        if (string.IsNullOrEmpty(gameFilePath))
        {
            DebugLogger.Log($"[ChdMountStrategy] No suitable game file found in mounted CHD at {mountedDrive.MountedPath}");
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"No game file found in mounted CHD for emulator '{context.EmulatorName}'");
            MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(logPath);
            return;
        }

        // Launch the emulator with the found game file
        await launcher.LaunchRegularEmulatorAsync(
            gameFilePath,
            context.EmulatorName,
            context.SystemManager,
            context.EmulatorManager,
            context.Parameters,
            context.MainWindow,
            context.LoadingState);
    }
}
