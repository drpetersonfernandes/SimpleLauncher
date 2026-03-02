using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.Converters;

public static class ConvertPbpToCueBin
{
    private static readonly string TempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

    /// <summary>
    /// Converts a PBP file to a temporary Cue/Bin using psxpackager.exe.
    /// Returns the path to the generated CUE file, or null if conversion failed.
    /// </summary>
    public static async Task<string> ConvertPbpToCueBinAsync(string pbpPath)
    {
        try
        {
            // PSXPackager only supports x64 architecture
            var arch = RuntimeInformation.ProcessArchitecture;
            if (arch == Architecture.Arm64)
            {
                DebugLogger.Log("[ConvertPbpToCueBin] PSXPackager is not available for ARM64 architecture.");
                return null;
            }

            var psxPackagerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "PSXPackager", "psxpackager.exe");

            if (!File.Exists(psxPackagerPath))
            {
                DebugLogger.Log($"[ConvertPbpToCueBin] psxpackager not found at {psxPackagerPath}. Cannot convert PBP.");
                return null;
            }

            var psxPackagerDir = Path.GetDirectoryName(psxPackagerPath);
            Directory.CreateDirectory(TempFolder);

            // Create a unique temp path for the output files
            var tempFileName = Guid.NewGuid().ToString();
            var tempCuePath = Path.Combine(TempFolder, $"{tempFileName}.cue");
            var tempBinPath = Path.Combine(TempFolder, $"{tempFileName}.bin");

            // Extract only disc 1 (using -d 1 flag) since we can only play one disc at a time.
            // PSXPackager creates both .bin and .cue files in the temp folder.
            // Without -d flag, all discs would be extracted, leaving orphaned files.
            var args = $"-i \"{pbpPath}\" -o \"{tempBinPath}\" -d 1";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = psxPackagerPath,
                Arguments = args,
                RedirectStandardOutput = false, // Not needed, prevents deadlock
                RedirectStandardError = true, // We want errors for logging
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = psxPackagerDir
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;

            DebugLogger.Log($"[ConvertPbpToCueBin] Running psxpackager with args: {args}");
            DebugLogger.Log("[ConvertPbpToCueBin] Converting from PBP to CUE/BIN.");

            var errorBuilder = new StringBuilder();
            process.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();

            // Add 5-minute timeout to prevent hanging forever
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            try
            {
                await process.WaitForExitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                DebugLogger.Log("[ConvertPbpToCueBin] Conversion timed out after 5 minutes. Killing process.");
                try
                {
                    process.Kill();
                }
                catch
                {
                    /* ignore */
                }

                return null;
            }

            if (process.ExitCode == 0)
            {
                // Check for the expected .cue file, or the _disc1 variant
                if (File.Exists(tempCuePath))
                {
                    DebugLogger.Log("[ConvertPbpToCueBin] Conversion successful.");
                    return tempCuePath;
                }

                // psxpackager may append _disc1 suffix when extracting specific disc
                var disc1CuePath = Path.Combine(TempFolder, $"{tempFileName}_disc1.cue");
                if (File.Exists(disc1CuePath))
                {
                    DebugLogger.Log("[ConvertPbpToCueBin] Conversion successful (disc 1 variant).");
                    return disc1CuePath;
                }
            }

            DebugLogger.Log($"[ConvertPbpToCueBin] psxpackager failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[ConvertPbpToCueBin] Exception during conversion: {ex.Message}");
            return null;
        }
    }
}
