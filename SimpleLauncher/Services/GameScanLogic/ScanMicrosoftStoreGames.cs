using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanMicrosoftStoreGames
{
    // Whitelist for Microsoft Store games to avoid adding Calculator/Photos etc.
    internal static readonly string[] MicrosoftStoreKeywords =
    {
        "Minecraft", "Solitaire", "Forza", "Halo", "Gears of War", "Sea of Thieves", "Flight Simulator", "Age of Empires", "Among Us", "Roblox", "Xbox"
    };

    public static async Task ScanMicrosoftStoreGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath)
    {
        try
        {
            // Most apps uses PackageManager API, but we stick to PowerShell for portability in SimpleLauncher.
            // We combine Get-StartApps (for AppID) with Get-AppxPackage (for InstallLocation).

            const string script = """

                                  $apps = Get-StartApps
                                  $packages = Get-AppxPackage
                                  $results = @()

                                  foreach ($app in $apps) {
                                      $pkg = $packages | Where-Object { $_.PackageFamilyName -and $app.AppID -like "$($_.PackageFamilyName)*" }
                                      if ($pkg) {
                                          $results += @{
                                              Name = $app.Name
                                              AppID = $app.AppID
                                              InstallLocation = $pkg.InstallLocation
                                              Logo = $pkg.Logo
                                          }
                                      }
                                  }
                                  $results | ConvertTo-Json

                                  """;

            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var powerShellPath = Path.Combine(systemPath, "WindowsPowerShell", "v1.0", "powershell.exe");

            if (!File.Exists(powerShellPath))
            {
                powerShellPath = "powershell.exe";
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = powerShellPath,
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return;

            // Capture both standard output and standard error to diagnose script failures
            var output = await process.StandardOutput.ReadToEndAsync();
            var errorOutput = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // If the script failed or produced any error output, log it and abort.
            if (process.ExitCode != 0 || !string.IsNullOrWhiteSpace(errorOutput))
            {
                var errorMessage = $"PowerShell script for Microsoft Store games failed. ExitCode: {process.ExitCode}, Error: {errorOutput}";
                DebugLogger.Log($"[ScanMicrosoftStoreGames] {errorMessage}");
                await logErrors.LogErrorAsync(new InvalidOperationException(errorOutput), "PowerShell script for scanning Microsoft Store games failed.");
                return;
            }

            if (string.IsNullOrWhiteSpace(output)) return;

            // Handle case where single object is returned (not array)
            var jsonStr = output.Trim();

            // Safeguard against non-JSON output that might have slipped through error checks
            if (!jsonStr.StartsWith('[') && !jsonStr.StartsWith('{')) return;

            if (jsonStr.StartsWith('{')) // Single object returned
            {
                jsonStr = $"[{jsonStr}]";
            }

            using var doc = JsonDocument.Parse(jsonStr);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return;

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                try
                {
                    var name = element.GetProperty("Name").GetString();
                    var appId = element.GetProperty("AppID").GetString();
                    var installLocation = element.TryGetProperty("InstallLocation", out var il) ? il.GetString() : null;

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(appId)) continue;

                    // Filter: Must match whitelist
                    var isMatch = MicrosoftStoreKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
                    if (!isMatch) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(name);
                    var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.bat");

                    // Use 'start shell:AppsFolder\...' for reliable launching
                    var batchContent = $"@echo off\r\nstart \"\" \"shell:AppsFolder\\{appId}\"";
                    await File.WriteAllTextAsync(shortcutPath, batchContent);

                    // Attempt Icon Extraction from Package Assets
                    if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                    {
                        await TryExtractStoreIcon(logErrors, installLocation, sanitizedGameName, windowsImagesPath);
                    }
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, "Error processing Microsoft Store game entry.");
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Microsoft Store games.");
        }
    }

    private static async Task TryExtractStoreIcon(ILogErrors logErrors, string installPath, string sanitizedGameName, string windowsImagesPath)
    {
        try
        {
            var destPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
            if (File.Exists(destPath)) return;

            // Heuristic: Look for Logo.png, StoreLogo.png, or images in Assets folder

            var possibleFiles = new[] { "StoreLogo.png", "Logo.png", "AppIcon.png", "Square150x150Logo.png", "Square44x44Logo.png" };

            // Check root
            foreach (var fileName in possibleFiles)
            {
                var p = Path.Combine(installPath, fileName);
                if (File.Exists(p))
                {
                    File.Copy(p, destPath, true);
                    return;
                }
            }

            // Check Assets or Images folder
            var subDirs = new[] { "Assets", "Images" };
            foreach (var sub in subDirs)
            {
                var dir = Path.Combine(installPath, sub);
                if (Directory.Exists(dir))
                {
                    var pngs = Directory.GetFiles(dir, "*.png");
                    // Pick the largest one usually
                    var bestIcon = pngs.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                    if (bestIcon != null)
                    {
                        File.Copy(bestIcon, destPath, true);
                        return;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, $"Failed to extract Microsoft Store icon for {sanitizedGameName}");
        }
    }
}