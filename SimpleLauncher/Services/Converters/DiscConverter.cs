using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleLauncher.Services.Converters;

using Interfaces;

/// <summary>
/// Converts disc image formats (CHD, PBP, and other disc images) to ISO or CUE/BIN using bundled conversion tools.
/// </summary>
public class DiscConverter : IDiscConverter
{
    private static readonly string TempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscConverter"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record conversion activity.</param>
    public DiscConverter(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Converts a CHD disc image to an ISO file using chdman.
    /// </summary>
    /// <param name="chdPath">The path of the CHD file to convert.</param>
    /// <returns>The path of the converted ISO file, or null if the conversion failed.</returns>
    public async Task<string?> ConvertChdToIsoAsync(string chdPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "chdman_arm64.exe" : "chdman.exe";
            var chdmanPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", exeName);

            if (!File.Exists(chdmanPath))
            {
                _logger.Debug($"[ConvertChdToIso] chdman not found at {chdmanPath}. Cannot convert CHD.");
                return null;
            }

            var chdmanDir = Path.GetDirectoryName(chdmanPath);
            Directory.CreateDirectory(TempFolder);

            var tempIsoPath = Path.Combine(TempFolder, $"{Guid.NewGuid()}.iso");

            var args = $"extractdvd -i \"{chdPath}\" -o \"{tempIsoPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = chdmanPath,
                Arguments = args,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = chdmanDir
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;

            _logger.Debug($"[ConvertChdToIso] Running chdman with args: {args}");
            _logger.Debug("[ConvertChdToIso] Converting from CHD to ISO.");

            var errorBuilder = new StringBuilder();
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("[ConvertChdToIso] Conversion timed out after 5 minutes.");
                try
                {
                    process.Kill();
                }
                catch
                {
                    /* ignored */
                }

                return null;
            }

            if (process.ExitCode == 0 && File.Exists(tempIsoPath))
            {
                _logger.Debug("[ConvertChdToIso] Conversion successful.");
                return tempIsoPath;
            }

            _logger.Debug($"[ConvertChdToIso] chdman failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[ConvertChdToIso] Error converting CHD to ISO.");
            _logger.Error(ex, "[ConvertChdToIso] Error converting CHD to ISO.");
            return null;
        }
    }

    /// <summary>
    /// Converts a CHD disc image to a CUE/BIN pair using chdman.
    /// </summary>
    /// <param name="chdPath">The path of the CHD file to convert.</param>
    /// <returns>The path of the converted CUE file, or null if the conversion failed.</returns>
    public async Task<string?> ConvertChdToCueBinAsync(string chdPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "chdman_arm64.exe" : "chdman.exe";
            var chdmanPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", exeName);

            if (!File.Exists(chdmanPath))
            {
                _logger.Debug($"[ConvertChdToCueBin] chdman not found at {chdmanPath}. Cannot convert CHD.");
                return null;
            }

            var chdmanDir = Path.GetDirectoryName(chdmanPath);
            Directory.CreateDirectory(TempFolder);

            var tempCuePath = Path.Combine(TempFolder, $"{Guid.NewGuid()}.cue");

            var args = $"extractcd -i \"{chdPath}\" -o \"{tempCuePath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = chdmanPath,
                Arguments = args,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = chdmanDir
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;

            _logger.Debug($"[ConvertChdToCueBin] Running chdman with args: {args}");
            _logger.Debug("[ConvertChdToCueBin] Converting from CHD to CUE/BIN.");

            var errorBuilder = new StringBuilder();
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("[ConvertChdToCueBin] Conversion timed out after 5 minutes. Killing process.");
                try
                {
                    process.Kill();
                }
                catch
                {
                    /* ignored */
                }

                return null;
            }

            if (process.ExitCode == 0 && File.Exists(tempCuePath))
            {
                _logger.Debug("[ConvertChdToCueBin] Conversion successful.");
                return tempCuePath;
            }

            _logger.Debug($"[ConvertChdToCueBin] chdman failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[ConvertChdToCueBin] Error converting CHD to CUE/BIN.");
            _logger.Error(ex, "[ConvertChdToCueBin] Error converting CHD to CUE/BIN.");
            return null;
        }
    }

    /// <summary>
    /// Converts a PBP disc image to a CUE/BIN pair using psxpackager.
    /// </summary>
    /// <param name="pbpPath">The path of the PBP file to convert.</param>
    /// <returns>The path of the converted CUE file, or null if the conversion failed.</returns>
    public async Task<string?> ConvertPbpToCueBinAsync(string pbpPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            if (arch == Architecture.Arm64)
            {
                _logger.Debug("[ConvertPbpToCueBin] PSXPackager is not available for ARM64 architecture.");
                return null;
            }

            var psxPackagerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "PSXPackager", "psxpackager.exe");

            if (!File.Exists(psxPackagerPath))
            {
                _logger.Debug($"[ConvertPbpToCueBin] psxpackager not found at {psxPackagerPath}. Cannot convert PBP.");
                return null;
            }

            var psxPackagerDir = Path.GetDirectoryName(psxPackagerPath);
            Directory.CreateDirectory(TempFolder);

            var tempFileName = Guid.NewGuid().ToString();
            var tempCuePath = Path.Combine(TempFolder, $"{tempFileName}.cue");
            var tempBinPath = Path.Combine(TempFolder, $"{tempFileName}.bin");

            var args = $"-i \"{pbpPath}\" -o \"{tempBinPath}\" -d 1";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = psxPackagerPath,
                Arguments = args,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = psxPackagerDir
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;

            _logger.Debug($"[ConvertPbpToCueBin] Running psxpackager with args: {args}");
            _logger.Debug("[ConvertPbpToCueBin] Converting from PBP to CUE/BIN.");

            var errorBuilder = new StringBuilder();
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("[ConvertPbpToCueBin] Conversion timed out after 5 minutes. Killing process.");
                try
                {
                    process.Kill();
                }
                catch
                {
                    /* ignored */
                }

                return null;
            }

            if (process.ExitCode == 0)
            {
                if (File.Exists(tempCuePath))
                {
                    _logger.Debug("[ConvertPbpToCueBin] Conversion successful.");
                    return tempCuePath;
                }

                var disc1CuePath = Path.Combine(TempFolder, $"{tempFileName}_disc1.cue");
                if (File.Exists(disc1CuePath))
                {
                    _logger.Debug("[ConvertPbpToCueBin] Conversion successful (disc 1 variant).");
                    return disc1CuePath;
                }
            }

            _logger.Debug($"[ConvertPbpToCueBin] psxpackager failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Debug($"[ConvertPbpToCueBin] Exception during conversion: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Converts a disc image file (such as RVZ) to an ISO file using DolphinTool.
    /// </summary>
    /// <param name="discImagePath">The path of the disc image file to convert.</param>
    /// <returns>The path of the converted ISO file, or null if the conversion failed.</returns>
    public async Task<string?> ConvertToIsoAsync(string discImagePath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "DolphinTool_arm64.exe" : "DolphinTool.exe";
            var dolphinToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToRVZ", exeName);

            if (!File.Exists(dolphinToolPath))
            {
                _logger.Debug($"[ConvertDiscImageToIso] DolphinTool not found at {dolphinToolPath}. Cannot convert disc image.");
                return null;
            }

            var dolphinDir = Path.GetDirectoryName(dolphinToolPath);

            var tempIsoPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.iso");

            var args = $"convert --format=iso --input=\"{discImagePath}\" --output=\"{tempIsoPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = dolphinToolPath,
                Arguments = args,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = dolphinDir
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;

            _logger.Debug($"[ConvertDiscImageToIso] Running DolphinTool with args: {args}");
            _logger.Debug($"[ConvertDiscImageToIso] Converting {Path.GetExtension(discImagePath)} to ISO.");

            var errorBuilder = new StringBuilder();
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                _logger.Debug("[ConvertDiscImageToIso] Conversion timed out after 5 minutes.");
                try
                {
                    process.Kill();
                }
                catch
                {
                    /* ignored */
                }

                return null;
            }

            if (process.ExitCode == 0 && File.Exists(tempIsoPath))
            {
                _logger.Debug("[ConvertDiscImageToIso] Conversion successful.");
                return tempIsoPath;
            }

            _logger.Debug($"[ConvertDiscImageToIso] DolphinTool failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "[ConvertDiscImageToIso] Error converting disc image to ISO.");
            _logger.Error(ex, "[ConvertDiscImageToIso] Error converting disc image to ISO.");
            return null;
        }
    }
}
