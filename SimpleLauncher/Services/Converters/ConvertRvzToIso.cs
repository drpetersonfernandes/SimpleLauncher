using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.Converters;

public static class ConvertRvzToIso
{
    /// <summary>
    /// Converts an RVZ file to a temporary ISO using DolphinTool.exe.
    /// </summary>
    public static async Task<string> ConvertRvzToIsoAsync(string rvzPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "DolphinTool_arm64.exe" : "DolphinTool.exe";
            var dolphinToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToRVZ", exeName);

            if (!File.Exists(dolphinToolPath))
            {
                DebugLogger.Log($"[ConvertRvzToIso] DolphinTool not found at {dolphinToolPath}. Cannot convert RVZ.");
                return null;
            }

            var dolphinDir = Path.GetDirectoryName(dolphinToolPath);

            var tempIsoPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.iso");

            // Arguments: convert --format=iso --input="in.rvz" --output="out.iso"
            var args = $"convert --format=iso --input=\"{rvzPath}\" --output=\"{tempIsoPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = dolphinToolPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = dolphinDir
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
            DebugLogger.Log($"[ConvertRvzToIso] DolphinTool failed. ExitCode: {process.ExitCode}. Error: {error}");
            return null;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "[ConvertRvzToIso] Error converting RVZ to ISO.");
            return null;
        }
    }
}