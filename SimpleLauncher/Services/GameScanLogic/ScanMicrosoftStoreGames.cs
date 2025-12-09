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
            // Playnite uses PackageManager API, but we stick to PowerShell for portability in SimpleLauncher.
            // We combine Get-StartApps (for AppID) with Get-AppxPackage (for InstallLocation).

            const string script = """

                                  $apps = Get-StartApps
                                  $packages = Get-AppxPackage
                                  $results = @()

                                  foreach ($app in $apps) {
                                      $pkg = $packages | Where-Object { $app.AppID -like "$($_.PackageFamilyName)*" }
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

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (string.IsNullOrWhiteSpace(output)) return;

            // Handle case where single object is returned (not array)
            var jsonStr = output.Trim();
            if (jsonStr.StartsWith('{'))
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
                        await TryExtractStoreIcon(installLocation, sanitizedGameName, windowsImagesPath);
                    }
                }
                catch
                {
                    /* Ignore */
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Microsoft Store games.");
        }
    }

    private static Task TryExtractStoreIcon(string installPath, string sanitizedGameName, string windowsImagesPath)
    {
        var destPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
        if (File.Exists(destPath)) return Task.CompletedTask;

        // Heuristic: Look for Logo.png, StoreLogo.png, or images in Assets folder
        // Playnite has complex logic for reading resources.pri, but we will do a file scan.

        var possibleFiles = new[] { "StoreLogo.png", "Logo.png", "AppIcon.png", "Square150x150Logo.png", "Square44x44Logo.png" };

        // Check root
        foreach (var fileName in possibleFiles)
        {
            var p = Path.Combine(installPath, fileName);
            if (File.Exists(p))
            {
                File.Copy(p, destPath, true);
                return Task.CompletedTask;
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
                    return Task.CompletedTask;
                }
            }
        }

        return Task.CompletedTask;
    }
}
