using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameScan.Models;
using SimpleLauncher.Services.SanitizeInputString;
using SimpleLauncher.SharedModels;

namespace SimpleLauncher.Services.GameScan;

internal static class ScanMicrosoftStoreGames
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

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
                                  $OutputEncoding = [Console]::OutputEncoding = [System.Text.Encoding]::UTF8;
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
                                  $results | ConvertTo-Json -Depth 2 -Compress
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
                Arguments = $"-NoProfile -Command \"{script}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            string output;
            string errorOutput;
            int exitCode;

            try
            {
                // Add a 30-second timeout to prevent indefinite hangs on locked-down systems
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    DebugLogger.Log("[ScanMicrosoftStoreGames] PowerShell process returned null (likely blocked by policy). Skipping Microsoft Store scan.");
                    return;
                }

                output = await process.StandardOutput.ReadToEndAsync(cts.Token);
                errorOutput = await process.StandardError.ReadToEndAsync(cts.Token);
                await process.WaitForExitAsync(cts.Token);
                exitCode = process.ExitCode;
            }
            catch (OperationCanceledException)
            {
                DebugLogger.Log("[ScanMicrosoftStoreGames] PowerShell scan timed out after 30 seconds. Skipping Microsoft Store scan.");
                return;
            }
            catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode is 5 or 2 or 126)
            {
                // 5 = Access Denied (AppLocker/WDAC), 2 = File Not Found, 126 = Module Not Found
                DebugLogger.Log($"[ScanMicrosoftStoreGames] PowerShell blocked or unavailable (Win32 error {ex.NativeErrorCode}). Skipping Microsoft Store scan.");
                return;
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[ScanMicrosoftStoreGames] Failed to execute PowerShell: {ex.Message}. Skipping Microsoft Store scan.");
                return;
            }

            if (exitCode != 0 && !string.IsNullOrWhiteSpace(errorOutput))
            {
                // Check for execution policy restrictions - skip silently if restricted
                if (IsExecutionPolicyRestricted(errorOutput))
                {
                    DebugLogger.Log("[ScanMicrosoftStoreGames] PowerShell execution policy restrictions detected. Skipping Microsoft Store games scan.");
                    return;
                }

                // Log warning but don't crash, PS might emit non-fatal errors to stderr
                DebugLogger.Log($"[ScanMicrosoftStoreGames] PowerShell warning/error: {errorOutput}");
            }

            if (string.IsNullOrWhiteSpace(output)) return;

            var jsonStr = output.Trim();
            // Safeguard against non-JSON output
            if (!jsonStr.StartsWith('[') && !jsonStr.StartsWith('{')) return;

            var allInstalledApps = new List<SelectableGameItem>();

            if (jsonStr.StartsWith('{')) // Single object returned
            {
                jsonStr = $"[{jsonStr}]";
            }

            using var doc = JsonDocument.Parse(jsonStr);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return;

            // Track seen AppIds to avoid duplicates from multiple Start Menu entries
            var seenAppIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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

                    // Skip duplicates
                    if (!seenAppIds.Add(appId)) continue;

                    allInstalledApps.Add(new SelectableGameItem
                    {
                        Name = name,
                        AppId = appId,
                        InstallLocation = installLocation,
                        PackageFamilyName = packageFamilyName,
                        LogoRelativePath = logoRelativePath,
                        IsSelected = true
                    });
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, "Error processing Microsoft Store game entry.");
                }
            }

            if (allInstalledApps.Count == 0)
            {
                DebugLogger.Log("[ScanMicrosoftStoreGames] No Microsoft Store apps found.");
                return;
            }

            DebugLogger.Log($"[ScanMicrosoftStoreGames] Found {allInstalledApps.Count} Microsoft Store apps. Sending to classification API...");
            foreach (var app in allInstalledApps)
            {
                DebugLogger.Log($"[ScanMicrosoftStoreGames]   -> Sending: Name=\"{app.Name}\" (Normalized=\"{app.Name.Trim().ToUpperInvariant()}\") AppId=\"{app.AppId}\"");
            }

            var confirmedGames = await ClassifyGamesViaApiAsync(allInstalledApps, logErrors);

            if (confirmedGames is { Count: > 0 })
            {
                DebugLogger.Log($"[ScanMicrosoftStoreGames] API returned {confirmedGames.Count} confirmed games.");

                foreach (var game in confirmedGames)
                {
                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(game.Name);
                    var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.bat");

                    var batchContent = $"@echo off\r\nstart \"\" \"shell:AppsFolder\\{game.AppId}\"";
                    await File.WriteAllTextAsync(shortcutPath, batchContent);

                    if (!string.IsNullOrEmpty(game.InstallLocation) && Directory.Exists(game.InstallLocation))
                    {
                        await TryExtractStoreIcon(logErrors, game.Name, game.InstallLocation, game.LogoRelativePath, sanitizedGameName, windowsImagesPath);
                    }
                }
            }
            else
            {
                DebugLogger.Log("[ScanMicrosoftStoreGames] API returned no confirmed games. The admin may need to curate the game list via the dashboard.");
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Microsoft Store games.");
        }
    }

    private static async Task<List<SelectableGameItem>> ClassifyGamesViaApiAsync(List<SelectableGameItem> installedApps, ILogErrors logErrors)
    {
        try
        {
            var httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            using var client = httpClientFactory.CreateClient("GameClassificationClient");

            var requestBody = new
            {
                SoftwareNames = installedApps.Select(static app => new
                {
                    app.Name,
                    app.AppId,
                    app.InstallLocation,
                    app.PackageFamilyName,
                    app.LogoRelativePath
                }).ToList()
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await client.PostAsync("api/GameIdentification/IsAGame", content, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                DebugLogger.Log($"[ScanMicrosoftStoreGames] Game classification API returned status: {response.StatusCode}");
                await logErrors.LogErrorAsync(null, $"Game classification API failed with status {response.StatusCode}. Returning empty game list.");
                return [];
            }

            var responseJson = await response.Content.ReadAsStringAsync(cts.Token);
            var apiResponse = JsonSerializer.Deserialize<GameClassificationResponse>(responseJson, JsonOptions);

            if (apiResponse?.Games == null)
            {
                DebugLogger.Log("[ScanMicrosoftStoreGames] Game classification API returned null games list.");
                return [];
            }

            DebugLogger.Log($"[ScanMicrosoftStoreGames] API deserialized games count: {apiResponse.Games.Count}");
            foreach (var g in apiResponse.Games)
            {
                DebugLogger.Log($"[ScanMicrosoftStoreGames]   <- Received game: Name=\"{g.Name}\" AppId=\"{g.AppId}\"");
            }

            var confirmedGames = apiResponse.Games.Select(static g => new SelectableGameItem
            {
                Name = g.Name,
                AppId = g.AppId,
                InstallLocation = g.InstallLocation,
                PackageFamilyName = g.PackageFamilyName,
                LogoRelativePath = g.LogoRelativePath,
                IsSelected = true
            }).ToList();

            return confirmedGames;
        }
        catch (OperationCanceledException)
        {
            DebugLogger.Log("[ScanMicrosoftStoreGames] Game classification API request timed out. Returning empty game list.");
            return [];
        }
        catch (HttpRequestException ex)
        {
            DebugLogger.Log($"[ScanMicrosoftStoreGames] Game classification API network error: {ex.Message}. Returning empty game list.");
            return [];
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[ScanMicrosoftStoreGames] Game classification API error: {ex.Message}. Returning empty game list.");
            await logErrors.LogErrorAsync(ex, "Failed to classify games via API.");
            return [];
        }
    }

    private static async Task TryExtractStoreIcon(ILogErrors logErrors, string gameName, string installPath, string logoRelativePath, string sanitizedGameName, string windowsImagesPath)
    {
        var destPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
        // Check if a valid icon already exists (non-zero size to handle corrupt/empty files from previous failed copies)
        if (File.Exists(destPath))
        {
            var fileInfo = new FileInfo(destPath);
            if (fileInfo.Length > 0) return;
            // If file exists but is empty/corrupt, we continue to overwrite it
        }

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
                        await Task.Run(() => File.Copy(fullLogoPath, destPath, true));
                        return;
                    }
                    catch (IOException ex) when (ex.Message.Contains("could not be encrypted", StringComparison.OrdinalIgnoreCase))
                    {
                        // EFS encryption error - fallback to byte-level copy which doesn't preserve encryption attributes
                        try
                        {
                            await Task.Run(() =>
                            {
                                var bytes = File.ReadAllBytes(fullLogoPath);
                                File.WriteAllBytes(destPath, bytes);
                            });
                            return;
                        }
                        catch (Exception fallbackEx)
                        {
                            await logErrors.LogErrorAsync(fallbackEx, $"Failed to copy Microsoft Store logo for {sanitizedGameName} (fallback method)");
                        }
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
                            await Task.Run(() => File.Copy(p, destPath, true));
                            return;
                        }
                        catch (IOException ex) when (ex.Message.Contains("could not be encrypted", StringComparison.OrdinalIgnoreCase))
                        {
                            // EFS encryption error - fallback to byte-level copy which doesn't preserve encryption attributes
                            try
                            {
                                await Task.Run(() =>
                                {
                                    var bytes = File.ReadAllBytes(p);
                                    File.WriteAllBytes(destPath, bytes);
                                });
                                return;
                            }
                            catch (Exception fallbackEx)
                            {
                                await logErrors.LogErrorAsync(fallbackEx, $"Failed to copy Microsoft Store logo for {sanitizedGameName} (fallback method)");
                            }
                        }
                        catch (Exception ex)
                        {
                            await logErrors.LogErrorAsync(ex, $"Failed to copy Microsoft Store logo for {sanitizedGameName}");
                            // Continue to next possibility
                        }
                    }
                }

                // Check for high-res targetsize images
                string[] pngs;
                try
                {
                    pngs = Directory.GetFiles(dir, "*.png");
                }
                catch (DirectoryNotFoundException)
                {
                    // Directory may have been removed or is inaccessible
                    continue;
                }
                catch (UnauthorizedAccessException)
                {
                    // No permission to access this directory
                    continue;
                }

                var bestIcon = pngs
                    .Where(static f => f.Contains("targetsize", StringComparison.OrdinalIgnoreCase) || f.Contains("scale", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(static f =>
                    {
                        try
                        {
                            return new FileInfo(f).Length;
                        }
                        catch (FileNotFoundException)
                        {
                            return 0L;
                        }
                    }) // Bigger is usually better quality
                    .FirstOrDefault();

                if (bestIcon != null)
                {
                    try
                    {
                        await Task.Run(() => File.Copy(bestIcon, destPath, true));
                        return;
                    }
                    catch (IOException ex) when (ex.Message.Contains("could not be encrypted", StringComparison.OrdinalIgnoreCase))
                    {
                        // EFS encryption error - fallback to byte-level copy which doesn't preserve encryption attributes
                        try
                        {
                            await Task.Run(() =>
                            {
                                var bytes = File.ReadAllBytes(bestIcon);
                                File.WriteAllBytes(destPath, bytes);
                            });
                            return;
                        }
                        catch (Exception fallbackEx)
                        {
                            await logErrors.LogErrorAsync(fallbackEx, $"Failed to copy Microsoft Store logo for {sanitizedGameName} (fallback method)");
                        }
                    }
                    catch (Exception ex)
                    {
                        await logErrors.LogErrorAsync(ex, $"Failed to copy Microsoft Store logo for {sanitizedGameName}");
                    }
                }

                // Fallback: Just take the largest PNG in the Assets folder
                if (dir.EndsWith("Assets", StringComparison.Ordinal) || dir.EndsWith("Images", StringComparison.Ordinal))
                {
                    var largestPng = pngs.OrderByDescending(static f =>
                    {
                        try
                        {
                            return new FileInfo(f).Length;
                        }
                        catch (FileNotFoundException)
                        {
                            return 0L;
                        }
                    }).FirstOrDefault();
                    if (largestPng != null)
                    {
                        try
                        {
                            await Task.Run(() => File.Copy(largestPng, destPath, true));
                            return;
                        }
                        catch (IOException ex) when (ex.Message.Contains("could not be encrypted", StringComparison.OrdinalIgnoreCase))
                        {
                            // EFS encryption error - fallback to byte-level copy which doesn't preserve encryption attributes
                            try
                            {
                                await Task.Run(() =>
                                {
                                    var bytes = File.ReadAllBytes(largestPng);
                                    File.WriteAllBytes(destPath, bytes);
                                });
                                return;
                            }
                            catch (Exception fallbackEx)
                            {
                                await logErrors.LogErrorAsync(fallbackEx, $"Failed to copy Microsoft Store logo for {sanitizedGameName} (fallback method)");
                            }
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

    /// <summary>
    /// Detects if PowerShell error output indicates execution policy restrictions
    /// </summary>
    private static bool IsExecutionPolicyRestricted(string errorOutput)
    {
        if (string.IsNullOrWhiteSpace(errorOutput)) return false;

        var lowerError = errorOutput.ToLowerInvariant();
        return lowerError.Contains("execution of scripts is disabled") ||
               (lowerError.Contains("execution policy") &&
                (lowerError.Contains("prevents execution") ||
                 lowerError.Contains("restricted") ||
                 lowerError.Contains("cannot be loaded"))) ||
               (lowerError.Contains("is not digitally signed") && lowerError.Contains("execution policy"));
    }
}