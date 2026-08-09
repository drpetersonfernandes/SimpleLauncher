using System.Diagnostics;

namespace SimpleLauncher.Avalonia.Services.GameLauncher;

/// <summary>
/// Mounts CHD disc images as virtual drives using the bundled CHDMounter tool (Dokan/WinFsp).
/// Mount → Launch → Unmount pattern. Does NOT extract CHD files.
/// </summary>
public class ChdMountService
{
    private readonly string _chdMounterPath;

    /// <summary>
    /// Maps system.xml SystemName → CHDMounter console alias.
    /// See CHDMounter README § Console Type Reference.
    /// </summary>
    private static readonly Dictionary<string, string> ConsoleAliasMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PS1"] = "ps1", ["Sony PlayStation 1"] = "ps1", ["PlayStation"] = "ps1",
        ["PS2"] = "ps2", ["Sony PlayStation 2"] = "ps2",
        ["PS3"] = "ps3", ["Sony PlayStation 3"] = "ps3",
        ["PSP"] = "psp", ["Sony PSP"] = "psp",
        ["Sega Saturn"] = "segasaturn", ["Saturn"] = "segasaturn",
        ["Sega Dreamcast"] = "segadreamcast", ["Dreamcast"] = "segadreamcast",
        ["Sega Genesis CD"] = "segagenesis", ["Sega CD"] = "segagenesis", ["Sega Mega CD"] = "segagenesis",
        ["Sega Genesis 32X CD"] = "segagenesis",
        ["Xbox"] = "xbox", ["Microsoft Xbox"] = "xbox",
        ["Xbox 360"] = "xbox360", ["Microsoft Xbox 360"] = "xbox360",
        ["3DO"] = "3do", ["Panasonic 3DO"] = "3do",
        ["CD-i"] = "cdi", ["Philips CD-i"] = "cdi",
        ["Amiga CD32"] = "amigacd32", ["Commodore Amiga CD32"] = "amigacd32",
        ["Amiga CD"] = "amigacd", ["Commodore Amiga CD"] = "amigacd",
        ["Amiga CDTV"] = "amigacdtv",
        ["PC Engine CD"] = "pcengine", ["NEC PC Engine CD"] = "pcengine", ["TurboGrafx-CD"] = "pcengine", ["TurboGrafx-16 CD"] = "pcengine",
        ["PC-FX"] = "pcfx", ["NEC PC-FX"] = "pcfx",
        ["Neo Geo CD"] = "neogeocd", ["SNK Neo Geo CD"] = "neogeocd",
        ["FM Towns"] = "fmtowns",
        ["X68000"] = "x68000", ["Sharp X68000"] = "x68000",
        ["PC-98"] = "pc98", ["NEC PC-98"] = "pc98",
        ["Pippin"] = "pippin", ["Apple Pippin"] = "pippin",
        ["Pico"] = "pico", ["Sega Pico"] = "pico",
        ["Nuon"] = "nuon"
        // Generic fallback for unknown CD systems
        // iso9660 = Generic ISO 9660 (works for most standard CD/DVD formats)
    };

    public ChdMountService()
    {
        _chdMounterPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "tools", "CHDMounter", "CHDMounter.exe");
    }

    /// <summary>
    /// Whether the CHDMounter tool is available.
    /// </summary>
    public bool IsAvailable => File.Exists(_chdMounterPath);

    /// <summary>
    /// Mounts a CHD file and returns the drive path (e.g., "Z:\").
    /// Returns the original path if mounting fails.
    /// </summary>
    /// <param name="chdPath">The path to the CHD file.</param>
    /// <param name="systemName">The system.xml SystemName used to select the console alias.</param>
    /// <param name="emulatorName">The emulator name (used for emulator-specific overrides).</param>
    /// <param name="emulatorLocation">The emulator executable path (used for emulator-specific overrides).</param>
    public async Task<string> MountAsync(string chdPath, string systemName, string? emulatorName = null, string? emulatorLocation = null)
    {
        if (!IsAvailable) return chdPath;

        var consoleAlias = GetConsoleAlias(systemName, emulatorName, emulatorLocation);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _chdMounterPath,
                Arguments = $"/a /s:{consoleAlias} \"{chdPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null) return chdPath;

            var output = await Task.Run(() => process.StandardOutput.ReadToEnd());
            await Task.Run(() => process.WaitForExit());

            // CHDMounter outputs the mount path with trailing backslash
            var mountPath = output.Trim().TrimEnd('\\');
            if (mountPath is [_, ':', ..])
            {
                mountPath += "\\";
                if (Directory.Exists(mountPath))
                    return mountPath;
            }

            // Also try to find the drive letter from output
            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim().TrimEnd('\\');
                if (trimmed is [_, ':', ..] && Directory.Exists(trimmed + "\\"))
                    return trimmed + "\\";
            }
        }
        catch (Exception ex)
        {
            // Mount failed — return original path, let the emulator handle it
            Log.Warning(ex, "CHD mount failed for {ChdPath}", chdPath);
        }

        return chdPath;
    }

    /// <summary>
    /// Resolves the CHDMounter console alias for the given system and emulator.
    /// Emulator-specific overrides take precedence (Final Burn Alpha → CUE/ISO/WAV 2048,
    /// Final Burn Neo → CUE/BIN 2352, Nebula → CUE/ISO 2048, Raine → CUE/ISO/WAV 2352).
    /// </summary>
    private static string GetConsoleAlias(string systemName, string? emulatorName, string? emulatorLocation)
    {
        var emulatorMatch = emulatorName ?? string.Empty;
        var locationMatch = emulatorLocation ?? string.Empty;

        if (emulatorMatch.Contains("FBAlpha", StringComparison.OrdinalIgnoreCase) ||
            emulatorMatch.Contains("FB Alpha", StringComparison.OrdinalIgnoreCase) ||
            emulatorMatch.Contains("FinalBurnAlpha", StringComparison.OrdinalIgnoreCase) ||
            emulatorMatch.Contains("Final Burn Alpha", StringComparison.OrdinalIgnoreCase) ||
            emulatorMatch.Contains("FinalBurn Alpha", StringComparison.OrdinalIgnoreCase) ||
            locationMatch.Contains("fba64.exe", StringComparison.OrdinalIgnoreCase))
        {
            return "cuebinwav2352";
        }

        if (emulatorMatch.Contains("FBNeo", StringComparison.OrdinalIgnoreCase) ||
            emulatorMatch.Contains("FB Neo", StringComparison.OrdinalIgnoreCase) ||
            emulatorMatch.Contains("FinalBurnNeo", StringComparison.OrdinalIgnoreCase) ||
            emulatorMatch.Contains("Final Burn Neo", StringComparison.OrdinalIgnoreCase) ||
            emulatorMatch.Contains("FinalBurn Neo", StringComparison.OrdinalIgnoreCase) ||
            locationMatch.Contains("fbneo64.exe", StringComparison.OrdinalIgnoreCase))
        {
            return "cuebinwav2352";
        }

        if (emulatorMatch.Contains("Nebula", StringComparison.OrdinalIgnoreCase) ||
            locationMatch.Contains("nebula.exe", StringComparison.OrdinalIgnoreCase))
        {
            return "cuebinwav2352";
        }

        if (emulatorMatch.Contains("raine", StringComparison.OrdinalIgnoreCase) ||
            locationMatch.Contains("raine.exe", StringComparison.OrdinalIgnoreCase))
        {
            return "cueisowav2352";
        }

        return ConsoleAliasMap.GetValueOrDefault(systemName, "iso9660");
    }

    /// <summary>
    /// Unmounts a previously mounted CHD drive.
    /// </summary>
    public void Unmount(string mountPath)
    {
        if (!IsAvailable) return;

        try
        {
            var driveLetter = mountPath.TrimEnd('\\', ':');
            if (driveLetter.Length != 1) return;

            var psi = new ProcessStartInfo
            {
                FileName = _chdMounterPath,
                Arguments = $"/u {driveLetter}:",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process?.WaitForExit(5000);
        }
        catch (Exception ex)
        {
            // Best-effort unmount
            Log.Debug(ex, "CHD unmount failed for {MountPath}", mountPath);
        }
    }
}
