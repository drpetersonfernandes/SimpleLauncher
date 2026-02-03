using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.Converters;

public static class ConvertChdToIso
{
    /// <summary>
    /// Converts a CHD file to a temporary ISO using chdman.exe.
    /// </summary>
    public static async Task<string> ConvertChdToIsoAsync(string chdPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "chdman_arm64.exe" : "chdman.exe";
            var chdmanPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", exeName);

            if (!File.Exists(chdmanPath))
            {
                DebugLogger.Log($"[ConvertChdToIso] chdman not found at {chdmanPath}. Cannot convert CHD.");
                return null;
            }

            var chdmanDir = Path.GetDirectoryName(chdmanPath);

            var tempIsoPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.iso");

            // chdman extractcd -i "input.chd" -o "output.iso"
            var args = $"extractcd -i \"{chdPath}\" -o \"{tempIsoPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = chdmanPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = chdmanDir
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && File.Exists(tempIsoPath))
            {
                return tempIsoPath;
            }

            var error = await process.StandardError.ReadToEndAsync();
            DebugLogger.Log($"[ConvertChdToIso] chdman failed. ExitCode: {process.ExitCode}. Error: {error}");
            return null;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "[ConvertChdToIso] Error converting CHD to ISO.");
            return null;
        }
    }
}
