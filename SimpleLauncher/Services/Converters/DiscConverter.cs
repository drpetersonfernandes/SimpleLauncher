#nullable enable

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleLauncher.Services.Converters;

using Interfaces;

public class DiscConverter : IDiscConverter
{
    private static readonly string TempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

    private readonly IDebugLogger _debugLogger;
    private readonly ILogErrors _logErrors;

    public DiscConverter(IDebugLogger debugLogger, ILogErrors logErrors)
    {
        _debugLogger = debugLogger;
        _logErrors = logErrors;
    }

    public async Task<string?> ConvertChdToIsoAsync(string chdPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "chdman_arm64.exe" : "chdman.exe";
            var chdmanPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", exeName);

            if (!File.Exists(chdmanPath))
            {
                _debugLogger.Log($"[ConvertChdToIso] chdman not found at {chdmanPath}. Cannot convert CHD.");
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

            _debugLogger.Log($"[ConvertChdToIso] Running chdman with args: {args}");
            _debugLogger.Log("[ConvertChdToIso] Converting from CHD to ISO.");

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
                _debugLogger.Log("[ConvertChdToIso] Conversion timed out after 5 minutes.");
                try { process.Kill(); } catch { /* ignored */ }

                return null;
            }

            if (process.ExitCode == 0 && File.Exists(tempIsoPath))
            {
                _debugLogger.Log("[ConvertChdToIso] Conversion successful.");
                return tempIsoPath;
            }

            _debugLogger.Log($"[ConvertChdToIso] chdman failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            _debugLogger.LogException(ex, "[ConvertChdToIso] Error converting CHD to ISO.");
            _logErrors.LogAndForget(ex, "[ConvertChdToIso] Error converting CHD to ISO.");
            return null;
        }
    }

    public async Task<string?> ConvertChdToCueBinAsync(string chdPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "chdman_arm64.exe" : "chdman.exe";
            var chdmanPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", exeName);

            if (!File.Exists(chdmanPath))
            {
                _debugLogger.Log($"[ConvertChdToCueBin] chdman not found at {chdmanPath}. Cannot convert CHD.");
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

            _debugLogger.Log($"[ConvertChdToCueBin] Running chdman with args: {args}");
            _debugLogger.Log("[ConvertChdToCueBin] Converting from CHD to CUE/BIN.");

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
                _debugLogger.Log("[ConvertChdToCueBin] Conversion timed out after 5 minutes. Killing process.");
                try { process.Kill(); } catch { /* ignored */ }

                return null;
            }

            if (process.ExitCode == 0 && File.Exists(tempCuePath))
            {
                _debugLogger.Log("[ConvertChdToCueBin] Conversion successful.");
                return tempCuePath;
            }

            _debugLogger.Log($"[ConvertChdToCueBin] chdman failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            _debugLogger.LogException(ex, "[ConvertChdToCueBin] Error converting CHD to CUE/BIN.");
            _logErrors.LogAndForget(ex, "[ConvertChdToCueBin] Error converting CHD to CUE/BIN.");
            return null;
        }
    }

    public async Task<string?> ConvertPbpToCueBinAsync(string pbpPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            if (arch == Architecture.Arm64)
            {
                _debugLogger.Log("[ConvertPbpToCueBin] PSXPackager is not available for ARM64 architecture.");
                return null;
            }

            var psxPackagerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "PSXPackager", "psxpackager.exe");

            if (!File.Exists(psxPackagerPath))
            {
                _debugLogger.Log($"[ConvertPbpToCueBin] psxpackager not found at {psxPackagerPath}. Cannot convert PBP.");
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

            _debugLogger.Log($"[ConvertPbpToCueBin] Running psxpackager with args: {args}");
            _debugLogger.Log("[ConvertPbpToCueBin] Converting from PBP to CUE/BIN.");

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
                _debugLogger.Log("[ConvertPbpToCueBin] Conversion timed out after 5 minutes. Killing process.");
                try { process.Kill(); } catch { /* ignored */ }

                return null;
            }

            if (process.ExitCode == 0)
            {
                if (File.Exists(tempCuePath))
                {
                    _debugLogger.Log("[ConvertPbpToCueBin] Conversion successful.");
                    return tempCuePath;
                }

                var disc1CuePath = Path.Combine(TempFolder, $"{tempFileName}_disc1.cue");
                if (File.Exists(disc1CuePath))
                {
                    _debugLogger.Log("[ConvertPbpToCueBin] Conversion successful (disc 1 variant).");
                    return disc1CuePath;
                }
            }

            _debugLogger.Log($"[ConvertPbpToCueBin] psxpackager failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[ConvertPbpToCueBin] Exception during conversion: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> ConvertToIsoAsync(string discImagePath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "DolphinTool_arm64.exe" : "DolphinTool.exe";
            var dolphinToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToRVZ", exeName);

            if (!File.Exists(dolphinToolPath))
            {
                _debugLogger.Log($"[ConvertDiscImageToIso] DolphinTool not found at {dolphinToolPath}. Cannot convert disc image.");
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

            _debugLogger.Log($"[ConvertDiscImageToIso] Running DolphinTool with args: {args}");
            _debugLogger.Log($"[ConvertDiscImageToIso] Converting {Path.GetExtension(discImagePath)} to ISO.");

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
                _debugLogger.Log("[ConvertDiscImageToIso] Conversion timed out after 5 minutes.");
                try { process.Kill(); } catch { /* ignored */ }

                return null;
            }

            if (process.ExitCode == 0 && File.Exists(tempIsoPath))
            {
                _debugLogger.Log("[ConvertDiscImageToIso] Conversion successful.");
                return tempIsoPath;
            }

            _debugLogger.Log($"[ConvertDiscImageToIso] DolphinTool failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            _debugLogger.LogException(ex, "[ConvertDiscImageToIso] Error converting disc image to ISO.");
            _logErrors.LogAndForget(ex, "[ConvertDiscImageToIso] Error converting disc image to ISO.");
            return null;
        }
    }
}
