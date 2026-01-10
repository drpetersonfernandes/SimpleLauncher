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
        "3D Viewer",
        "55290BeXCool.BeWidgets_n3myysfhx5594", // PackageFamilyName for BeWidgets
        "57868Codaapp.37800EEDB46F1_4bn2s5v6tep1y", // PackageFamilyName for AukZip
        "5A894077.McAfeeSecurity_wafk5atnkzcwy", // PackageFamilyName for McAfee© Personal Security
        "7EE7776C.LinkedInforWindows_w1wdnht996qgy", // PackageFamilyName for LinkedIn
        "8741Veirbel.Fixyfier_a9nzdnn6kvaww", // PackageFamilyName for Fixyfier
        "18496Starpine.Screenbox_rm8wvch11q4my", // PackageFamilyName for Screenbox
        "22450.PPTXViewer_0aqw1zw0x2snt", // PackageFamilyName for PPTX Viewer
        "33C30B79.NGENUITY_922sw8z9z7n5w", // PackageFamilyName for HyperX NGENUITY
        "39211d.karakatsanis.XboxControllerBattery_3fta4zx0djza0", // PackageFamilyName for Gamepad Battery Status
        "AccessEnum",
        "Accessoires Xbox",
        "Adobe",
        "ADExplorer",
        "ADInsight",
        "Affinity",
        "Albums partag",
        "Alexa",
        "Alpine Linux",
        "Amazon Music",
        "AMD Link",
        "AMD Software",
        "Android",
        "Apple",
        "AppleÿMusic",
        "AppleÿTV",
        "AppUp.IntelArcSoftware_8j3eq9eme6ctt", // PackageFamilyName for Intelr Graphics Software
        "AppUp.IntelOptaneMemoryandStorageManagement_8j3eq9eme6ctt", // PackageFamilyName for Intel© Rapid Storage Technology Application
        "Aquila Reader",
        "Arduino IDE",
        "Armoury Crate",
        "Asistencia",
        "Assistance rapide",
        "Assistenza rapida",
        "Astuces",
        "ASUS",
        "Audio Control",
        "Audio Trimmer & Joiner",
        "AukZip",
        "Aura Creator",
        "Aurora Overlay",
        "Autologon",
        "Autoruns",
        "Befehlspalette",
        "Better Audio Editor",
        "BeWidgets",
        "BGInfo",
        "Bing",
        "Bing News",
        "Bing Weather",
        "BIVROST 360Player",
        "Blender",
        "Bloc",
        "Bloc de notas",
        "Bloc-notes",
        "Blocco note",
        "Bluetooth",
        "Bookviser Inc.BookviserPreview_kfyhp4yj0hvpw", // PackageFamilyName for Bookviser Preview
        "Bookviser Preview",
        "Booking.com",
        "Brother Print",
        "CacheSet",
        "Calculator",
        "Calendar",
        "Calendario",
        "Calendrier",
        "CANAL+",
        "Canon",
        "Canon Inkjet",
        "Canon Inkjet Print Utility",
        "Centre de configuration",
        "Centro de comando",
        "Centrum sterowania",
        "ChatGPT",
        "Chroma Control",
        "Cinebench",
        "Color Palette Universal",
        "Comix",
        "Command Palette",
        "Cool File",
        "Copilot",
        "CoreinfoEx",
        "Corel PaintShop Pro",
        "Cortana",
        "Correo",
        "Courrier",
        "de notas",
        "Dell",
        "Dev Home",
        "Dicas",
        "DirectorManagerV2",
        "Dolby",
        "Dolby Access",
        "DolbyLaboratories.DolbyAccess_rz1tebttyb220", // PackageFamilyName for Dolby Access
        "do Windows",
        "Dropbox",
        "DTS Audio Processing",
        "DTS Sound",
        "DTS Sound Unbound",
        "DTS:X Ultra",
        "DTSInc.DTSAudioProcessing_t5j2fzbtdg37r", // PackageFamilyName for DTS Audio Processing
        "DTSInc.DTSXUltra_t5j2fzbtdg37r", // PackageFamilyName for DTS:X Ultra
        "DVD Play",
        "EarTrumpet",
        "ECApp",
        "Editor",
        "Elisa",
        "Email",
        "Energy Star",
        "Epson Print and Scan",
        "Escáner",
        "EyeTune",
        "Family",
        "FBReader",
        "FBReader.ORGLimited.FBReader_n9cpejf4jr0x8", // PackageFamilyName for FBReader
        "FeedDeck",
        "Filelight",
        "Files",
        "Fitbit Coach",
        "Fixyfier",
        "Focus To-Do",
        "Food Storage",
        "Forfaits mobiles",
        "Freda ebook",
        "Galaxy Buds",
        "Game Bar",
        "Gamepad Battery Status",
        "GameSir Nexus",
        "GamingApp",
        "Get Help",
        "Glance by MirametrixR",
        "GlideX",
        "Gospel Library",
        "Hash Checker",
        "HEIC",
        "HelloMAUI",
        "HEY Mail",
        "Home Remote",
        "HP",
        "HP Audio Control",
        "HP Smart",
        "HP System Information",
        "HyperX NGENUITY",
        "ibisPaint",
        "IDLE",
        "iMazing",
        "Intel",
        "Intel© Rapid Storage Technology Application",
        "Intelr Graphics",
        "Intelr Graphics Software",
        "Intelr OptaneT",
        "IntelR",
        "IP CAM Controller",
        "IPTV",
        "iTunes",
        "Journal",
        "Kalender",
        "Kalendarz",
        "KDEe.V.Filelight_7vt06qxq7ptv8", // PackageFamilyName for Filelight
        "Killer Intelligence Center",
        "Kodi",
        "LDS Scripture",
        "Lenovo",
        "LG Monitor",
        "LinkedIn",
        "LocalSend",
        "LockApp",
        "Mail",
        "MaxxAudioPro",
        "McAfee© Personal Security",
        "McAfeer",
        "Messenger",
        "Microsoft 365",
        "Microsoft 365 Copilot",
        "Microsoft AsyncTextService",
        "Microsoft Clipchamp",
        "Microsoft CredDialogHost",
        "Microsoft Defender",
        "Microsoft Edge",
        "Microsoft Emulator",
        "Microsoft People",
        "Microsoft Teams",
        "Microsoft Teams (personal)",
        "Microsoft Whiteboard",
        "Microsoft.Messaging_8wekyb3d8bbwe", // PackageFamilyName for Operator messages
        "Microsoft.Windows.ContentDeliveryManager",
        "Microsoft.Windows.Cortana",
        "Microsoft.Windows.Photos",
        "Microsoft.Windows.ShellExperienceHost",
        "Microsoft.WindowsAlarms",
        "Microsoft.WindowsCalculator",
        "Microsoft.WindowsCamera",
        "Microsoft.WindowsFeedbackHub",
        "Microsoft.WindowsMaps",
        "Microsoft.WindowsSecurity",
        "Microsoft.WindowsSoundRecorder",
        "Microsoft.WindowsStore",
        "MINDSTORMS",
        "Minecraft Education",
        "Mixed Reality",
        "Mixed Reality Portal",
        "Mobile Plans",
        "Mozilla.Firefox_n80bbvh6b1yt2", // PackageFamilyName for Firefox
        "Move Mouse",
        "MP3 Cutter",
        "ms-resource",
        "MSI",
        "MSIX",
        "My Thrustmaster Panel",
        "MyASUS",
        "Nahimic",
        "NanaZip",
        "NeeView",
        "Network Speed",
        "Notatnik",
        "Notification Manager",
        "Notepad",
        "NVIDIA Control Panel",
        "Office.OneNote",
        "OMEN Audio Control",
        "OMEN Gaming Hub",
        "OneDrive",
        "Online Radio",
        "Operator messages",
        "Outlook",
        "Overclocking",
        "Paint",
        "Paint 3D",
        "Paintÿ3D",
        "Paisa",
        "Palette de",
        "PC Manager",
        "PDF",
        "PDF to DWG Converter",
        "Phone Link",
        "Planes móviles",
        "plink",
        "Poczta",
        "Portal de realidad mixta",
        "Portail de",
        "Power Automate",
        "Power BI Desktop",
        "PPTX Viewer",
        "Prime Video",
        "Print 3D",
        "propos de Windows",
        "PSCP",
        "PSFTP",
        "PuTTY",
        "PuTTY Authentication Agent",
        "PuTTY Key Generator",
        "Python",
        "Python install manager",
        "PythonSoftwareFoundation.PythonManager_3847v3x7pw1km", // PackageFamilyName for Python install manager
        "Quick Assist",
        "Raindrop.io",
        "Rayen.RyTuneX_h37fyha1qbnfe", // PackageFamilyName for RyTuneX
        "Real HEIC to JPG Converter",
        "Realtek Audio Console",
        "Recomendaciones",
        "Remote Keyboard Desktop",
        "Remotehilfe",
        "RICOH Driver",
        "RyTuneX",
        "Safe",
        "Scan",
        "Screen Sketch",
        "Screenbox",
        "ScreenToGif",
        "Seguridad",
        "Seguridad de Windows",
        "Segurança",
        "SEIKOEPSONCORPORATION.EpsonPrintandScan_ezaqdwkaef94e", // PackageFamilyName for Epson Print and Scan
        "Sécurité",
        "ShowKeyPlus",
        "Sicurezza di Windows",
        "SkypeApp",
        "Smarters IPTV",
        "Solidigm SynergyT Toolkit",
        "Sound Blaster Connect",
        "Speedtest",
        "Spotify",
        "Sticky Notes",
        "Sugerencias",
        "Surface",
        "Sweet Home",
        "Szybka pomoc",
        "Terminal",
        "ThunderboltT",
        "Tips",
        "Tobii Experience",
        "ToDo",
        "TranslucentTB",
        "TuneIn Radio",
        "TvMate",
        "Ubuntu",
        "Visualizador 3D",
        "Visionneuse",
        "WavesAudio.WavesMaxxAudioProforDell_fh4rh281wavaa", // PackageFamilyName for MaxxAudioPro
        "WhatsApp",
        "Wi-Fi",
        "WinDbg",
        "Windows App",
        "Windows File Recovery",
        "Windows HDR",
        "Windows HDR Calibration",
        "Windows Security",
        "Windows Subsystem",
        "Windows Terminal",
        "Windows-Sicherheit",
        "windows.immersivecontrolpanel",
        "Wintoys",
        "Xbox",
        "Xbox Accessories",
        "Xbox Insider Hub",
        "XboxApp",
        "XboxGameOverlay",
        "XboxGamingOverlay",
        "XboxIdentityProvider",
        "XboxSpeechToTextOverlay",
        "Xerox Print and Scan Experience",
        "YourPhone",
        "ZeroDev.RemoteKeyboardDesktop_7sg2dz33ww9gp", // PackageFamilyName for Remote Keyboard Desktop
        "ZuneMusic",
        "ZuneVideo"
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
