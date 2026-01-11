using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SimpleLauncher.Models.GameScanLogic;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public static class ScanMicrosoftStoreGames
{
    private static readonly HashSet<string> IgnoredAppNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "3D Builder",
        "AccessEnum",
        "ADExplorer",
        "ADInsight",
        "Adobe",
        "Affinity",
        "Alexa",
        "Alpine Linux",
        "Amazon Music",
        "AMD Link",
        "AMD Software",
        "Android",
        "Apple",
        "AppUp.",
        "Aquila Reader",
        "Arduino IDE",
        "Armoury Crate",
        "ASUS",
        "Audio Control",
        "Audio Trimmer & Joiner",
        "Autologon",
        "Autoruns",
        "Better Audio Editor",
        "BGInfo",
        "Bing",
        "Blender",
        "Bluetooth",
        "Bookviser Inc.",
        "Booking.com",
        "Brother Print",
        "CacheSet",
        "CANAL+",
        "Canon",
        "ChatGPT",
        "Chroma Control",
        "Cinebench",
        "Color Palette Universal",
        "Comix",
        "Cool File",
        "CoreinfoEx",
        "Corel PaintShop Pro",
        "Dell",
        "DirectorManagerV2",
        "Dolby",
        "Dropbox",
        "DTSInc.",
        "DVD Play",
        "EarTrumpet",
        "ECApp",
        "Editor",
        "Elisa",
        "Email",
        "Energy Star",
        "Epson Print and Scan",
        "EyeTune",
        "FBReader.ORGLimited.",
        "FeedDeck",
        "Filelight",
        "Fitbit Coach",
        "Fixyfier",
        "Focus To-Do",
        "Food Storage",
        "Freda ebook",
        "Galaxy Buds",
        "GameSir Nexus",
        "GamingApp",
        "Gospel Library",
        "Hash Checker",
        "HEIC",
        "HelloMAUI",
        "HEY Mail",
        "Home Remote",
        "HP",
        "HyperX NGENUITY",
        "ibisPaint",
        "IDLE",
        "iMazing",
        "Intel",
        "IP CAM Controller",
        "IPTV",
        "Journal",
        "KDEe.V.",
        "Killer Intelligence Center",
        "Kodi",
        "LDS Scripture",
        "Lenovo",
        "LG Monitor",
        "LinkedIn",
        "LocalSend",
        "LockApp",
        "MaxxAudioPro",
        "McAfee© Personal Security",
        "McAfeer",
        "Messenger",
        "MINDSTORMS",
        "Mozilla.",
        "Move Mouse",
        "MP3 Cutter",
        "ms-resource",
        "MSI",
        "MSIX",
        "My Thrustmaster Panel",
        "Nahimic",
        "NanaZip",
        "NeeView",
        "Network Speed",
        "Notification Manager",
        "NVIDIA Control Panel",
        "Office.OneNote",
        "Online Radio",
        "Overclocking",
        "Paint",
        "Paisa",
        "Palette de",
        "PDF",
        "PDF to DWG Converter",
        "Planes móviles",
        "plink",
        "Poczta",
        "Print 3D",
        "propos de Windows",
        "PSCP",
        "PSFTP",
        "PuTTY",
        "Python",
        "Raindrop.io",
        "Rayen.",
        "Real HEIC to JPG Converter",
        "Realtek Audio Console",
        "Recomendaciones",
        "Remotehilfe",
        "RICOH Driver",
        "RyTuneX",
        "Safe",
        "Scan",
        "ScreenToGif",
        "Seguridad",
        "Segurança",
        "SEIKOEPSONCORPORATION.",
        "Sécurité",
        "ShowKeyPlus",
        "Smarters IPTV",
        "Solidigm SynergyT Toolkit",
        "Sound Blaster Connect",
        "Speedtest",
        "Spotify",
        "Sticky Notes",
        "Sugerencias",
        "Sweet Home",
        "Szybka pomoc",
        "Terminal",
        "ThunderboltT",
        "Tobii Experience",
        "ToDo",
        "TranslucentTB",
        "TuneIn Radio",
        "TvMate",
        "Ubuntu",
        "Visionneuse",
        "WavesAudio.",
        "WhatsApp",
        "Wi-Fi",
        "WinDbg",
        "Wintoys",
        "Xbox",
        "Xerox Print and Scan Experience",
        "ZeroDev.",
        "1Password",
        "Albums partag",
        "Assistance rapide",
        "Assistenza rapida",
        "Astuces",
        "AukZip",
        "Aura Creator",
        "Aurora Overlay",
        "Befehlspalette",
        "BeWidgets",
        "Bloc",
        "Bloc de notas",
        "Bloc-notes",
        "Blocco note",
        "Bookviser Preview",
        "Calculator",
        "Calendar",
        "Calendario",
        "Calendrier",
        "Centre de configuration",
        "Centro de comando",
        "Centrum sterowania",
        "Cortana",
        "Correo",
        "Courrier",
        "de notas",
        "Dicas",
        "do Windows",
        "Dropshelf",
        "Escáner",
        "Family",
        "FBReader",
        "FluentInfo",
        "Forfaits mobiles",
        "Gamepad Battery Status",
        "Gigabyte Dynamic Light",
        "IDLE (Python 3.12)",
        "Kalender",
        "Kalendarz",
        "KDAN PDF",
        "Mail",
        "Mobile Plans",
        "My Favorite Files",
        "Notatnik",
        "Notepad",
        "OMEN Audio Control",
        "OP Auto Clicker",
        "Operator messages",
        "Outlook",
        "Paint 3D",
        "Paintÿ3D",
        "PC Manager",
        "Phone Link",
        "Portail de",
        "PPTX Viewer",
        "Prime Video",
        "Python 3.12",
        "QuickLook",
        "Remote Keyboard Desktop",
        "Screenbox",
        "Seguridad de Windows",
        "Sicurezza di Windows",
        "Surface",
        "Tips",
        "Visualizador 3D",
        "Windows App",
        "Windows Folder Organizer",
        "Windows HDR",
        "Windows HDR Calibration",
        "Windows Security",
        "Windows Subsystem",
        "Windows Terminal",
        "Windows-Sicherheit",
        "Wireless Display Adapter",
        "YourPhone",
        "ZuneMusic",
        "ZuneVideo",
        "0D9A1B2D.PDFReaderUWP_jhretta7p24aw", // KDAN PDF
        "14914FroltSoftware.27457B918A8BA_v5vt3srnrv4et", // My Favorite Files
        "18496Starpine.Screenbox_rm8wvch11q4my", // Screenbox
        "21090PaddyXu.QuickLook_egxr34yet59cg", // QuickLook
        "22450.PPTXViewer_0aqw1zw0x2snt", // PPTX Viewer
        "33C30B79.NGENUITY_922sw8z9z7n5w", // HyperX NGENUITY
        "371WilliamHa.Dropshelf_k4fd9x13b0jec", // Dropshelf
        "38458AutoClicker.OPAutoClicker-AutoTap_5e1qkq7gw5abm", // OP Auto Clicker
        "39211d.karakatsanis.XboxControllerBattery_3fta4zx0djza0", // Gamepad Battery Status
        "49624ubuntuegor.FluentInfo_c9p81xkhha7ay", // FluentInfo
        "55290BeXCool.BeWidgets_n3myysfhx5594", // BeWidgets
        "57868Codaapp.37800EEDB46F1_4bn2s5v6tep1y", // AukZip
        "5A894077.McAfeeSecurity_wafk5atnkzcwy", // McAfee© Personal Security
        "65d483df-b37e-4fcf-94de-8b795233db63_1mmjbktjj1mkp", // Gigabyte Dynamic Light
        "7EE7776C.LinkedInforWindows_w1wdnht996qgy", // LinkedIn
        "8741Veirbel.Fixyfier_a9nzdnn6kvaww", // Fixyfier
        "Agilebits.1Password_amwd9z03whsfe", // 1Password
        "TheDebianProject.DebianGNULinux_76v4gfsz19hv4" // Debian
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
                // Check for execution policy restrictions
                if (IsExecutionPolicyRestricted(errorOutput))
                {
                    MessageBoxLibrary.PowerShellExecutionPolicyRestrictions();
                    return;
                }

                // Log warning but don't crash, PS might emit non-fatal errors to stderr
                DebugLogger.Log($"[ScanMicrosoftStoreGames] PowerShell warning/error: {errorOutput}");
            }

            if (string.IsNullOrWhiteSpace(output)) return;

            var jsonStr = output.Trim();
            // Safeguard against non-JSON output
            if (!jsonStr.StartsWith('[') && !jsonStr.StartsWith('{')) return;

            var potentialGames = new List<SelectableGameItem>();

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

                    potentialGames.Add(new SelectableGameItem
                    {
                        Name = name,
                        AppId = appId,
                        InstallLocation = installLocation,
                        PackageFamilyName = packageFamilyName,
                        LogoRelativePath = logoRelativePath,
                        IsSelected = true // Default to selected for the verification window
                    });
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, "Error processing Microsoft Store game entry.");
                }
            }

            if (potentialGames.Count != 0)
            {
                // Send the list of newly found, unignored programs to the developer for analysis
                try
                {
                    var newPrograms = potentialGames.Select(p => p.Name).ToList();
                    if (newPrograms.Count != 0)
                    {
                        var reportContent = new StringBuilder();
                        reportContent.AppendLine("--- Microsoft Store Scan Results ---");
                        reportContent.AppendLine("The following programs were found and are not on the ignore list:");
                        foreach (var programName in newPrograms.OrderBy(n => n))
                        {
                            reportContent.AppendLine(CultureInfo.InvariantCulture, $"- {programName}");
                        }

                        // Use LogErrorAsync to send the report. Pass null for the exception.
                        await logErrors.LogErrorAsync(null, reportContent.ToString());
                        DebugLogger.Log("[ScanMicrosoftStoreGames] Sent list of potential Microsoft Store games to developer.");
                    }
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, "Failed to send Microsoft Store scan results to developer.");
                }

                List<SelectableGameItem> confirmedGames = null;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var verificationWindow = new GameVerificationWindow(potentialGames);
                    if (verificationWindow.ShowDialog() == true)
                    {
                        confirmedGames = verificationWindow.ConfirmedGames;
                    }
                });

                if (confirmedGames != null && confirmedGames.Count != 0)
                {
                    foreach (var game in confirmedGames)
                    {
                        var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(game.Name);
                        var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.bat");

                        // Use 'start shell:AppsFolder\...' for reliable launching
                        var batchContent = $"@echo off\r\nstart \"\" \"shell:AppsFolder\\{game.AppId}\"";
                        await File.WriteAllTextAsync(shortcutPath, batchContent);

                        // Attempt Icon Extraction
                        if (!string.IsNullOrEmpty(game.InstallLocation) && Directory.Exists(game.InstallLocation))
                        {
                            await TryExtractStoreIcon(logErrors, game.Name, game.InstallLocation, game.LogoRelativePath, sanitizedGameName, windowsImagesPath);
                        }
                    }
                }
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
