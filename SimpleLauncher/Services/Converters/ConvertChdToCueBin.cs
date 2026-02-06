using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.Converters;

public static class ConvertChdToCueBin
{
    private static readonly string TempFolder = Path.Combine(Path.GetTempPath(), "SimpleLauncher");

    /// <summary>
    /// Converts a CHD file to a temporary Cue/Bin using chdman.exe.
    /// Returns the path to the generated CUE file, or null if conversion failed.
    /// </summary>
    public static async Task<string> ConvertChdToCueBinAsync(string chdPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "chdman_arm64.exe" : "chdman.exe";
            var chdmanPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", exeName);

            if (!File.Exists(chdmanPath))
            {
                DebugLogger.Log($"[ConvertChdToCueBin] chdman not found at {chdmanPath}. Cannot convert CHD.");
                return null;
            }

            var chdmanDir = Path.GetDirectoryName(chdmanPath);
            Directory.CreateDirectory(TempFolder);

            // Create a unique temp path for the .cue file
            var tempCuePath = Path.Combine(TempFolder, $"{Guid.NewGuid()}.cue");

            // chdman extractcd -i "input.chd" -o "output.cue"
            // This will also create the .bin file automatically in the same temp folder.
            var args = $"extractcd -i \"{chdPath}\" -o \"{tempCuePath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = chdmanPath,
                Arguments = args,
                RedirectStandardOutput = false, // Not needed, prevents deadlock
                RedirectStandardError = true, // We want errors for logging
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = chdmanDir
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;

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
                DebugLogger.Log("[ConvertChdToCueBin] Conversion timed out after 5 minutes. Killing process.");
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

            if (process.ExitCode == 0 && File.Exists(tempCuePath))
            {
                return tempCuePath;
            }

            DebugLogger.Log($"[ConvertChdToCueBin] chdman failed. ExitCode: {process.ExitCode}. Error: {errorBuilder}");
            return null;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "[ConvertChdToCueBin] Error converting CHD to CUE/BIN.");
            return null;
        }
    }
}
