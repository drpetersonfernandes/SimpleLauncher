using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleLauncher.Services;

/// <summary>
/// A helper class to execute the external RAHasher.exe tool for generating game file hashes.
/// </summary>
public static class RetroAchievementsHasherTool
{
    private static readonly string HasherPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "RAHasher", "RAHasher.exe");

    /// <summary>
    /// Gets the hash for a given file using the external RAHasher.exe tool.
    /// </summary>
    /// <param name="filePath">The full path to the game file to be hashed.</param>
    /// <param name="systemId">The RetroAchievements console ID.</param>
    /// <returns>The calculated hash as a string, or null if an error occurs.</returns>
    public static async Task<string> GetHashAsync(string filePath, int systemId)
    {
        if (!File.Exists(HasherPath))
        {
            DebugLogger.Log($"[RAHasher] RAHasher.exe not found at {HasherPath}");
            MessageBoxLibrary.RaHasherNotFoundMessageBox();
            return null;
        }

        if (!File.Exists(filePath))
        {
            DebugLogger.Log($"[RAHasher] File to hash not found: {filePath}");
            return null;
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = HasherPath,
            Arguments = $"{systemId} \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(HasherPath) ?? string.Empty
        };

        try
        {
            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            // Read output and error streams asynchronously
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // Wait for the process to exit and for streams to be read
            await process.WaitForExitAsync();
            var output = await outputTask;
            var error = await errorTask;

            // --- MODIFIED LOGIC ---
            // Prioritize parsing the output for a hash, as RAHasher might return a non-zero
            // exit code even when it successfully produces a hash.
            var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var potentialHash = line.Trim();
                if (potentialHash.Length == 32 && potentialHash.All(static c => "0123456789abcdefABCDEF".Contains(c)))
                {
                    DebugLogger.Log($"[RAHasher] Successfully parsed hash '{potentialHash}' for '{Path.GetFileName(filePath)}' (System ID: {systemId}). Exit code was {process.ExitCode}.");
                    return potentialHash.ToLowerInvariant();
                }
            }

            // If no hash was found in the output, then a non-zero exit code is a genuine error.
            if (process.ExitCode != 0)
            {
                DebugLogger.Log($"[RAHasher] Error executing RAHasher.exe. No hash found in output. Exit code: {process.ExitCode}");
                DebugLogger.Log($"[RAHasher] Stderr: {error}");
                DebugLogger.Log($"[RAHasher] Stdout: {output}");
                return null;
            }

            // This case handles when exit code is 0 but output is empty or unparseable.
            DebugLogger.Log($"[RAHasher] Could not parse a valid hash from RAHasher output, despite exit code 0: {output}");
            return null;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RAHasher] An exception occurred while running RAHasher.exe for {filePath}");
            return null;
        }
    }
}
