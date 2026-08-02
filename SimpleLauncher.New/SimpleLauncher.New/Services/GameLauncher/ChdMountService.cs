using System.Diagnostics;

namespace SimpleLauncher.New.Services.GameLauncher;

/// <summary>
/// Mounts CHD disc images as virtual drives using the bundled CHDMounter tool (Dokan/WinFsp).
/// Mount → Launch → Unmount pattern. Does NOT extract CHD files.
/// </summary>
public class ChdMountService
{
    private readonly string _chdMounterPath;

    /// <summary>
    /// Maps system.xml SystemName → CHDMounter console index (1–31).
    /// See CHDMounter README § Console Type Reference.
    /// </summary>
    private static readonly Dictionary<string, int> ConsoleIndexMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PS1"] = 8, ["Sony PlayStation 1"] = 8, ["PlayStation"] = 8,
        ["PS2"] = 9, ["Sony PlayStation 2"] = 9,
        ["PS3"] = 10, ["Sony PlayStation 3"] = 10,
        ["PSP"] = 12, ["Sony PSP"] = 12,
        ["Sega Saturn"] = 13, ["Saturn"] = 13,
        ["Sega Dreamcast"] = 4, ["Dreamcast"] = 4,
        ["Sega Genesis CD"] = 14, ["Sega CD"] = 14, ["Sega Mega CD"] = 14,
        ["Sega Genesis 32X CD"] = 14,
        ["Xbox"] = 16, ["Microsoft Xbox"] = 16,
        ["Xbox 360"] = 17, ["Microsoft Xbox 360"] = 17,
        ["3DO"] = 15, ["Panasonic 3DO"] = 15,
        ["CD-i"] = 3, ["Philips CD-i"] = 3,
        ["Amiga CD32"] = 2, ["Commodore Amiga CD32"] = 2,
        ["Amiga CD"] = 1, ["Commodore Amiga CD"] = 1,
        ["Amiga CDTV"] = 1,
        ["PC Engine CD"] = 6, ["NEC PC Engine CD"] = 6, ["TurboGrafx-CD"] = 6, ["TurboGrafx-16 CD"] = 6,
        ["PC-FX"] = 7, ["NEC PC-FX"] = 7,
        ["Neo Geo CD"] = 5, ["SNK Neo Geo CD"] = 5,
        ["FM Towns"] = 25,
        ["X68000"] = 27, ["Sharp X68000"] = 27,
        ["PC-98"] = 29, ["NEC PC-98"] = 29,
        ["Pippin"] = 31, ["Apple Pippin"] = 31,
        ["Pico"] = 28, ["Sega Pico"] = 28,
        ["Nuon"] = 30
        // Generic fallback for unknown CD systems
        // Index 19 = Generic ISO 9660 (works for most standard CD/DVD formats)
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
    public async Task<string> MountAsync(string chdPath, string systemName)
    {
        if (!IsAvailable) return chdPath;

        var consoleIndex = ConsoleIndexMap.GetValueOrDefault(systemName, 19);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _chdMounterPath,
                Arguments = $"/a /s:{consoleIndex} \"{chdPath}\"",
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
