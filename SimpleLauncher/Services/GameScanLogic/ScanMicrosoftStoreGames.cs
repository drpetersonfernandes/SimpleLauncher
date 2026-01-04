using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public static class ScanMicrosoftStoreGames
{
    // Replaced Whitelist with a Blacklist (IgnoredAppNames) to allow all games to be found automatically.
    // This list includes common system apps and tools that are not games.
    private static readonly HashSet<string> IgnoredAppNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Microsoft.WindowsCalculator", "Microsoft.WindowsAlarms", "Microsoft.WindowsSoundRecorder",
        "Microsoft.WindowsMaps", "Microsoft.ZuneMusic", "Microsoft.ZuneVideo", "Microsoft.SkypeApp",
        "Microsoft.MicrosoftEdge", "Microsoft.Office.OneNote", "Microsoft.People", "Microsoft.Windows.Photos",
        "Microsoft.YourPhone", "Microsoft.WindowsStore", "windows.immersivecontrolpanel",
        "Microsoft.Windows.Cortana", "Microsoft.GetHelp", "Microsoft.WindowsCamera",
        "Microsoft.XboxApp", "Microsoft.XboxGamingOverlay", "Microsoft.XboxGameOverlay",
        "Microsoft.XboxSpeechToTextOverlay", "Microsoft.XboxIdentityProvider", "Microsoft.GamingApp",
        "Microsoft.MicrosoftStickyNotes", "Microsoft.ScreenSketch", "Microsoft.WindowsTerminal",
        "Microsoft.Paint", "Microsoft.Notepad", "Notepad", "Microsoft.WindowsFeedbackHub", "Microsoft.Microsoft365",
        "Microsoft.OneDrive", "Microsoft.ToDo", "Microsoft.BingNews", "Microsoft.BingWeather",
        "Microsoft.Windows.ContentDeliveryManager", "Microsoft.Windows.ShellExperienceHost",
        "Microsoft.AsyncTextService", "Microsoft.ECApp", "Microsoft.LockApp", "Microsoft.CredDialogHost", "NVIDIA Control Panel",
        "Microsoft.WindowsSecurity", "Windows Security", "AMD Software", "Microsoft.Copilot", "Copilot",
        "Microsoft 365 Copilot", "AMD Link", "Bloco de notas", "Microsoft Teams", "Segurança do Windows", "do Windows", "Realtek Audio Console"
    };

    public static async Task ScanMicrosoftStoreGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath)
    {
        try
        {
            // Enhanced PowerShell script:
            // 1. Gets Start Menu Apps (for the correct Display Name and AppID).
            // 2. Gets AppxPackages (for InstallLocation and Logo).
            // 3. Filters out Frameworks, Resources, and Non-Store/Developer signed apps (System components).
            // 4. Matches them based on PackageFamilyName.
            const string script = """
                                  $ErrorActionPreference = 'SilentlyContinue'
                                  $apps = Get-StartApps
                                  $packages = Get-AppxPackage
                                  $pkgHash = @{}
                                  
                                  # Index packages by FamilyName for speed
                                  foreach ($p in $packages) {
                                      if (-not $p.IsFramework -and -not $p.IsResourcePackage) {
                                          $pkgHash[$p.PackageFamilyName] = $p
                                      }
                                  }
                                  
                                  $results = @()
                                  
                                  foreach ($app in $apps) {
                                      # AppID is usually "FamilyName!AppId"
                                      if ([string]::IsNullOrEmpty($app.AppID)) { continue }
                                      
                                      $parts = $app.AppID.Split('!')
                                      $famName = $parts[0]
                                      
                                      if ($pkgHash.ContainsKey($famName)) {
                                          $pkg = $pkgHash[$famName]
                                          
                                          # Filter out System apps that might have slipped through (Signature check)
                                          if ($pkg.SignatureKind -eq 'System') { continue }
                                          
                                          $results += @{
                                              Name = $app.Name
                                              AppID = $app.AppID
                                              InstallLocation = $pkg.InstallLocation
                                              PackageFamilyName = $pkg.PackageFamilyName
                                              Logo = $pkg.Logo
                                          }
                                      }
                                  }
                                  $results | ConvertTo-Json -Depth 2
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

            var output = await process.StandardOutput.ReadToEndAsync();
            var errorOutput = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0 && !string.IsNullOrWhiteSpace(errorOutput))
            {
                // Log warning but don't crash, PS might emit non-fatal errors to stderr
                DebugLogger.Log($"[ScanMicrosoftStoreGames] PowerShell warning/error: {errorOutput}");
            }

            var foundGames = new List<string>();

            if (string.IsNullOrWhiteSpace(output)) return;

            var jsonStr = output.Trim();
            // Safeguard against non-JSON output
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
                    var packageFamilyName = element.TryGetProperty("PackageFamilyName", out var pfn) ? pfn.GetString() : "";
                    var logoRelativePath = element.TryGetProperty("Logo", out var lg) ? lg.GetString() : null;

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(appId)) continue;

                    // 1. Blacklist Check
                    if (IgnoredAppNames.Contains(name) || IgnoredAppNames.Contains(packageFamilyName)) continue;
                    // Also check if the family name starts with ignored prefixes
                    if (!string.IsNullOrEmpty(packageFamilyName) && IgnoredAppNames.Any(ignored => packageFamilyName.StartsWith(ignored, StringComparison.OrdinalIgnoreCase))) continue;

                    // 2. Heuristic: If it has an install location, check if it looks like a game
                    // Most games have an EXE. Some system apps don't (they are just hosts).
                    if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                    {
                        // Optional: Filter out if no .exe is found, though some apps might be pure DLLs hosted by a runner.
                        // For games, 99% have an exe.
                        if (!Directory.EnumerateFiles(installLocation, "*.exe", SearchOption.AllDirectories).Any())
                        {
                            continue;
                        }
                    }

                    foundGames.Add($"Name: {name}, AppID: {appId}, PackageFamilyName: {packageFamilyName}");

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(name);
                    var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.bat");

                    // Use 'start shell:AppsFolder\...' for reliable launching
                    var batchContent = $"@echo off\r\nstart \"\" \"shell:AppsFolder\\{appId}\"";
                    await File.WriteAllTextAsync(shortcutPath, batchContent);

                    // Attempt Icon Extraction
                    if (!string.IsNullOrEmpty(installLocation) && Directory.Exists(installLocation))
                    {
                        await TryExtractStoreIcon(logErrors, name, installLocation, logoRelativePath, sanitizedGameName, windowsImagesPath);
                    }
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, "Error processing Microsoft Store game entry.");
                }
            }

            if (foundGames.Count > 0)
            {
                var report = new System.Text.StringBuilder();
                report.AppendLine(CultureInfo.InvariantCulture, $"Found {foundGames.Count} potential games from Microsoft Store scan:");
                foreach (var game in foundGames)
                {
                    report.AppendLine(CultureInfo.InvariantCulture, $"- {game}");
                }

                await logErrors.LogErrorAsync(new Exception("Microsoft Store Games Scan Report"), report.ToString());
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Microsoft Store games.");
        }
    }

    private static async Task TryExtractStoreIcon(ILogErrors logErrors, string gameName, string installPath, string logoRelativePath, string sanitizedGameName, string windowsImagesPath)
    {
        var destPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
        if (File.Exists(destPath)) return;

        // 1. Try API first
        if (await GameScannerService.TryDownloadImageFromApiAsync(gameName, destPath, logErrors))
        {
            return;
        }

        try
        {
            // 2. Try the Logo property returned by PowerShell (often points to Assets\StoreLogo.png or similar)
            if (!string.IsNullOrEmpty(logoRelativePath))
            {
                var fullLogoPath = Path.Combine(installPath, logoRelativePath);
                if (File.Exists(fullLogoPath))
                {
                    // Use try-catch for file operations
                    try
                    {
                        File.Copy(fullLogoPath, destPath, true);
                        return;
                    }
                    catch (Exception ex)
                    {
                        await logErrors.LogErrorAsync(ex, $"Failed to copy Microsoft Store logo for {sanitizedGameName}");
                    }
                }
            }

            // 3. Heuristic Search: Look for common logo names
            // Windows Store apps often use "targetsize" naming for scaled icons.
            var possibleFiles = new List<string>
            {
                "StoreLogo.png", "Logo.png", "AppIcon.png",
                "Square150x150Logo.png", "Square310x310Logo.png", "Square44x44Logo.png",
                "Wide310x150Logo.png", "SplashScreen.png"
            };

            // Add search for targetsize (e.g., AppIcon.targetsize-256.png)
            var searchDirectories = new[] { installPath, Path.Combine(installPath, "Assets"), Path.Combine(installPath, "Images") };

            foreach (var dir in searchDirectories)
            {
                if (!Directory.Exists(dir)) continue;

                // Check exact matches
                foreach (var fileName in possibleFiles)
                {
                    var p = Path.Combine(dir, fileName);
                    if (File.Exists(p))
                    {
                        try
                        {
                            File.Copy(p, destPath, true);
                            return;
                        }
                        catch (Exception ex)
                        {
                            await logErrors.LogErrorAsync(ex, $"Failed to copy Microsoft Store logo for {sanitizedGameName}");
                            // Continue to next possibility
                        }
                    }
                }

                // Check for high-res targetsize images
                var pngs = Directory.GetFiles(dir, "*.png");
                var bestIcon = pngs
                    .Where(f => f.Contains("targetsize", StringComparison.OrdinalIgnoreCase) || f.Contains("scale", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(f => new FileInfo(f).Length) // Bigger is usually better quality
                    .FirstOrDefault();

                if (bestIcon != null)
                {
                    try
                    {
                        File.Copy(bestIcon, destPath, true);
                        return;
                    }
                    catch (Exception ex)
                    {
                        await logErrors.LogErrorAsync(ex, $"Failed to copy Microsoft Store logo for {sanitizedGameName}");
                    }
                }

                // Fallback: Just take the largest PNG in the Assets folder
                if (dir.EndsWith("Assets", StringComparison.Ordinal) || dir.EndsWith("Images", StringComparison.Ordinal))
                {
                    var largestPng = pngs.OrderByDescending(f => new FileInfo(f).Length).FirstOrDefault();
                    if (largestPng != null)
                    {
                        try
                        {
                            File.Copy(largestPng, destPath, true);
                            return;
                        }
                        catch (Exception ex)
                        {
                            await logErrors.LogErrorAsync(ex, $"Failed to copy Microsoft Store logo for {sanitizedGameName}");
                        }
                    }
                }
            }

            // 4. Final fallback to extracting icon from an EXE in the install folder
            await GameScannerService.ExtractIconFromGameFolder(logErrors, installPath, sanitizedGameName, windowsImagesPath);
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, $"Failed to extract Microsoft Store icon for {sanitizedGameName}");
        }
    }
}