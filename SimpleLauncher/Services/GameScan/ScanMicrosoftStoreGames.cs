using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models.GameScan;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.GameScan;

internal static class ScanMicrosoftStoreGames
{
    private static readonly HashSet<string> IgnoredAppNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "123 FacilePhoto",
        "18496Starpine.Screenbox_rm8wvch11q4my",
        "1Password",
        "22450.PPTXViewer_0aqw1zw0x2snt",
        "33C30B79.NGENUITY_922sw8z9z7n5w",
        "39211d.karakatsanis.XboxControllerBattery_3fta4zx0djza0",
        "3BMeteo",
        "3D Builder",
        "3D Viewer",
        "3D-viewer",
        "55290BeXCool.BeWidgets_n3myysfhx5594",
        "57868Codaapp.37800EEDB46F1_4bn2s5v6tep1y",
        "5A894077.McAfeeSecurity_wafk5atnkzcwy",
        "7EE7776C.LinkedInforWindows_w1wdnht996qgy",
        "8741Veirbel.Fixyfier_a9nzdnn6kvaww",
        "8BitDo Ultimate Software X",
        "9 Zip",
        "Accesorios de Xbox",
        "Acer LiveArt",
        "Acer Product Registration",
        "Acer Purified Voice Console (R)",
        "AccessEnum",
        "Accessoires Xbox",
        "ADExplorer",
        "ADInsight",
        "Adobe XD",
        "Adobe",
        "Adobe Express Photos",
        "Adobe Fresco",
        "Adquira o Office",
        "Affinity",
        "Affinity Designer 2",
        "Affinity Photo 2",
        "Affinity Publisher 2",
        "Agenda",
        "Air Screen Mirroring Receiver",
        "Alarm Clock HD",
        "Albums partag",
        "Alexa",
        "Alpine Linux",
        "Amazon Music",
        "AMD Link",
        "AMD Radeon Software",
        "AMD Software",
        "Android",
        "Aow Tools",
        "Apple",
        "AppleÿMusic",
        "AppleÿTV",
        "Applus",
        "AppUp.IntelArcSoftware_8j3eq9eme6ctt",
        "AppUp.IntelOptaneMemoryandStorageManagement_8j3eq9eme6ctt",
        "Aquila Reader",
        "Arduino IDE",
        "Ark View",
        "Armoury Crate",
        "Asistencia",
        "Asistencia rápida",
        "Assistance rapide",
        "Assistência Rápida",
        "Assistenza rapida",
        "Astuces",
        "ASUS",
        "ASUS Welcome",
        "AuDimix",
        "Audio Control",
        "Audio Trimmer & Joiner",
        "AukZip",
        "Aura Creator",
        "Aurora Overlay",
        "Autologon",
        "Autoruns",
        "Avatares originales de Xbox",
        "AZ Torrent Downloader",
        "Base64 Convert",
        "Befehlspalette",
        "Better Audio Editor",
        "BeWidgets",
        "BGInfo",
        "Binary Hex and Decimal Converter",
        "Bing",
        "Bing News",
        "Bing Weather",
        "Bing Wallpaper",
        "BIMx",
        "Bitwarden",
        "BIVROST 360Player",
        "Blank Canvas",
        "Blender",
        "Blip App",
        "Bloc",
        "Bloc App",
        "Bloc de notas",
        "Bloco de notas",
        "Bloc-notes",
        "Blocco note",
        "Bluetooth",
        "Bookviser Inc.BookviserPreview_kfyhp4yj0hvpw",
        "Bookviser Preview",
        "Booking.com",
        "Brother Print",
        "Bulk File Rename",
        "ByteStream Torrent",
        "CacheSet",
        "Calculator",
        "Calendar",
        "Calendário",
        "Calendrier",
        "CANAL+",
        "CameraL",
        "Camo Studio",
        "Canon",
        "Canon Inkjet",
        "Canon Inkjet Print Utility",
        "Canon Office Printer Utility",
        "Canon PRINT",
        "Canon Print Assistant",
        "Care Center",
        "Cattura e annota",
        "Centre de configuration",
        "Centre de configuration des graphiques Intel©",
        "Centre de configuration des graphiques Intelr",
        "Centro comandi della grafica Intel©",
        "Centro de comando",
        "Centro de comando de gráficos Intel©",
        "Centro de control ThunderboltT",
        "Centro de controle ThunderboltT",
        "Centro di controllo ThunderboltT",
        "Centrum sterowania",
        "ChatGPT",
        "CheckListr",
        "Chroma Control",
        "Cinebench",
        "Clair Obscur: Expedition 33",
        "Clima",
        "Clocks - The evolving clock App",
        "Code Writer",
        "Collegamento al telefono",
        "Color Palette Universal",
        "Command Palette",
        "Comix",
        "Compagnon de la console Xbox",
        "Company Portal",
        "Conectar",
        "Contatar o Suporte",
        "Convert-Image",
        "Cool File",
        "Copilot",
        "CoreinfoEx",
        "Corel PaintShop Pro",
        "Cortana",
        "Correo",
        "Courrier",
        "Cover",
        "CPU Overclocking",
        "CPU Stress",
        "Crosshair Zoom",
        "CrystalDiskMark",
        "Dados da Rede Celular e Wi-Fi Pagos",
        "DDT Global",
        "de notas",
        "DebugView",
        "Debian",
        "Dell",
        "Dell Free Fall Data Protection",
        "Desktop remoto",
        "Desktops",
        "Dev Home",
        "Dev Home (Preview)",
        "DevToys",
        "Dicas",
        "Dirac Audio Manager",
        "DirectorManagerV2",
        "Disk Space Analyzer Max",
        "Disk2vhd",
        "Diskmon",
        "DiskView",
        "DisplayLink Manager",
        "Ditto",
        "Dolby",
        "Dolby Access",
        "DolbyLaboratories.DolbyAccess_rz1tebttyb220",
        "do Windows",
        "Dragon Center",
        "Dropshelf",
        "Dropbox",
        "Dropbox - promoção",
        "Dropbox promotion",
        "DTS Audio Processing",
        "DTS CUSTOM",
        "DTS Sound",
        "DTS Sound Unbound",
        "DTS:X Ultra",
        "DTSInc.DTSAudioProcessing_t5j2fzbtdg37r",
        "DTSInc.DTSXUltra_t5j2fzbtdg37r",
        "DuckDuckGo",
        "DVD Creator - Video Burner Pro",
        "DVD Play",
        "EarTrumpet",
        "EarTrumpet (dev)",
        "Easy Disk Catalog Maker",
        "EasyMail",
        "EasyScan - PDF Scan",
        "ECApp",
        "Editable Word",
        "Editor",
        "Editor de avatares de Xbox",
        "eFootballT Settings",
        "El Tiempo",
        "Elisa",
        "Email",
        "Energy Star",
        "Enlace M¢vil",
        "Epson Print and Scan",
        "Escaner",
        "Escáner",
        "Escritorio remoto",
        "ETDProperties",
        "EV3 Classroom",
        "Evernote",
        "Excel",
        "Express Torrent Downloader",
        "EyeTune",
        "Family",
        "FBReader",
        "FBReader.ORGLimited.FBReader_n9cpejf4jr0x8",
        "FeedDeck",
        "FeedFlow",
        "FeedLab",
        "Ferramenta de Captura",
        "fClock",
        "File Renamer",
        "Filelight",
        "Files",
        "Film e TV",
        "Filmes e TV",
        "Films Guide",
        "Films & TV",
        "Films en tv",
        "Films et TV",
        "Filmler ve TV",
        "Firefox",
        "Fitbit Coach",
        "Fixyfier",
        "Flock",
        "FluentInfo",
        "Fluent Emoji Gallery",
        "Fluent Video Player",
        "FluentWeather",
        "Focus To-Do",
        "Food Storage",
        "Forfaits mobiles",
        "Freda ebook",
        "Free PDF Converter - Totally Free",
        "Free Virtual Keyboard",
        "fTorrent - Fast Torrent Downloader",
        "Galaxy Buds",
        "Game Bar",
        "Gamepad Battery Status",
        "Game Controller Tester",
        "GameSir Nexus",
        "GamingApp",
        "Get Help",
        "Gigabyte Dynamic Light",
        "Glance by MirametrixR",
        "GlideX",
        "Google",
        "Gospel Library",
        "GT Auto Clicker",
        "Haberler",
        "Hash Checker",
        "Hash Viewer",
        "Hava Durumu",
        "HEIC",
        "HEIC Convert Pro - Image Converter",
        "HEIC Converter X",
        "HelloMAUI",
        "Herramienta Recortes",
        "HEY Mail",
        "HEVC Player for Windows",
        "Home Remote",
        "HORI Device Manager for Xbox Series X Series S",
        "Hotspot Shield VPN - Wifi Proxy",
        "HP",
        "HP Audio Center",
        "HP Audio Control",
        "HP Display Center",
        "HP Enhanced Lighting",
        "HP PC Hardware Diagnostics Windows",
        "HP Privacy Settings",
        "HP Prime Free",
        "HP Programmable Key",
        "HP QuickDrop",
        "HP Smart",
        "HP Support Assistant",
        "HP System Event Utility",
        "HP System Information",
        "Hue Essentials",
        "Hulp vragen",
        "HyperX NGENUITY",
        "ibisPaint",
        "iCloud",
        "iCloud Parolalar",
        "IDLE",
        "Image Raw.Viewer",
        "iMazing",
        "Inicio de desarrollo",
        "Intel",
        "Intel(R) Management and Security Status",
        "Intel© Application Optimization",
        "Intel© Connectivity Performance Suite",
        "Intel© Grafikleri Kontrol Merkezi",
        "Intelr Graphics Control Panel",
        "Intel© Graphics Command Center",
        "Intel© Rapid Storage Technology Application",
        "Intel© Unisont",
        "IntelR",
        "Intelr Graphics",
        "Intelr Graphics Command Center",
        "Intelr Graphics Software",
        "Intelr OptaneT",
        "IPCam Monitor",
        "IP CAM Controller",
        "IPTV",
        "IPTV Smarters Expert",
        "IPTV Stream Player Official",
        "ISO Image Creator",
        "iTunes",
        "Journal",
        "Kalender",
        "Kalendarz",
        "Kamera",
        "Karnaval Radyo",
        "KDAN PDF",
        "KDEe.V.Filelight_7vt06qxq7ptv8",
        "KDE Connect",
        "Killer Intelligence Center",
        "Knippen en aantekenen",
        "Kodi",
        "Komut Paleti",
        "LDS Scripture",
        "Lecteur multimídia",
        "Lector",
        "Lenovo",
        "Lenovo Hotkeys",
        "Lenovo Smart Noise Cancellation",
        "Lenovo Vantage",
        "Lettore multimediale",
        "LG Monitor",
        "LG Monitor App Installer",
        "LinkedIn",
        "Lively Wallpaper",
        "LoadOrder",
        "LocalSend",
        "LockApp",
        "Luminosity app",
        "Mail",
        "manus",
        "MaxxAudioPro",
        "McAfee© Personal Security",
        "McAfeer",
        "MD5 Win Verifier",
        "Media Player",
        "Mediaspeler",
        "Meld Studio",
        "Memo",
        "Messenger",
        "Meteo",
        "Micro Torrent Downloader",
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
        "Microsoft Teams (work or school)",
        "Microsoft To Do",
        "Microsoft Whiteboard",
        "Microsoft Wi-Fi",
        "Microsoft.Messaging_8wekyb3d8bbwe",
        "Microsoft News: Noticias destacadas en español",
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
        "mitmdump (headless)",
        "mitmproxy (console ui)",
        "mitmweb (web ui)",
        "Mixed Reality",
        "Mixed Reality Portal",
        "Mixed Reality ポータル",
        "Mixed Reality-portal",
        "Mlol Ebook Reader",
        "Mobile connect",
        "Mobile Plans",
        "ModernFlyouts (Preview)",
        "Monitorian",
        "Mozilla.Firefox_n80bbvh6b1yt2",
        "Mozilla Thunderbird",
        "Move Mouse",
        "Movies & TV",
        "MP3 Cutter",
        "ms-resource",
        "MSI",
        "MSI Center",
        "MSIX",
        "MusicBee",
        "MultiMail - Multi-Account Email Client",
        "MyAppName",
        "My Favorite Files",
        "My Files-X Free",
        "My RSS Feeds",
        "My Thrustmaster Panel",
        "MyASUS",
        "MyReader",
        "Nahimic",
        "NanaZip",
        "NeeView",
        "Network Speed",
        "Network Speed Test",
        "News",
        "Newsflow",
        "NitroSense",
        "NostalgicPlayer",
        "Not Defteri",
        "Notatnik",
        "Notas Autoadesivas",
        "Notas rápidas",
        "Notícias",
        "Notification Manager",
        "Notification Manager for Adobe Acrobat",
        "Notification Manager for Acrobat Reader",
        "Notizie",
        "Notepad",
        "NotMyFault",
        "NVC - Free Any Video Converter",
        "NVIDIA Control Panel",
        "Obter Ajuda",
        "Obtener ayuda",
        "Obtenir de l'aide",
        "Office.OneNote",
        "Old Windows versions",
        "OMEN Audio Control",
        "OMEN Gaming Hub",
        "One Calendar",
        "One Game Launcher",
        "One Photo Viewer",
        "OneDrive",
        "OneNote",
        "OneNote for Windows 10",
        "OP Auto Clicker",
        "Online Radio",
        "Open RAR",
        "Operator messages",
        "OTT Navigator - IPTV Player",
        "Outil Capture",
        "Outlook",
        "Outlook (new)",
        "Outlook (クラシック)",
        "Overclocking",
        "Página Inicial de Desenvolvimento",
        "Paint",
        "Paint 3D",
        "Paintÿ3D",
        "Paisa",
        "Palette de commandes",
        "Paperclip by FireCube",
        "PC Manager",
        "PDF",
        "PDF Reader by Xodo",
        "PDF to DWG Converter",
        "PDF X",
        "Películas y TV",
        "Pense-bˆtes",
        "People",
        "Pessoas",
        "PetitCalendar",
        "Phone Link",
        "PhotoDirector for acer",
        "PhotoKing",
        "Piani dati mobili",
        "Plaknotities",
        "Planes móviles",
        "Plex",
        "plink",
        "Poczta",
        "Portail de",
        "Portal de realidad mixta",
        "Portale realt… mista",
        "Posta",
        "Povezivanje sa telefonom",
        "Power Automate",
        "Power Automate Troubleshooter",
        "Power BI Desktop",
        "PowerDirector for acer",
        "PowerPoint",
        "PowerShell",
        "PPTX Viewer",
        "PressReader",
        "Prime Video",
        "Print 3D",
        "Process Explorer",
        "Process Monitor",
        "ProjectDuGet",
        "Pronalazenje pomoci",
        "propos de Windows",
        "PSCP",
        "PSFTP",
        "PuTTY",
        "PuTTY Authentication Agent",
        "PuTTY Key Generator",
        "Python",
        "Python 3.12",
        "Python 3.13",
        "Python install manager",
        "PythonSoftwareFoundation.PythonManager_3847v3x7pw1km",
        "Quick Access",
        "Quick Assist",
        "Quick Picture Viewer",
        "QuickLook",
        "Raindrop.io",
        "RamMap",
        "Rayen.RyTuneX_h37fyha1qbnfe",
        "RDCMan",
        "React Native Gallery (Legacy)",
        "Real HEIC to JPG Converter",
        "Realtek Audio Console",
        "Recipe Keeper",
        "Recomendaciones",
        "Recorte y anotaci¢n",
        "Rememory",
        "Remote Desktop",
        "Remote Keyboard Desktop",
        "Remotehilfe",
        "Reproductor multimedia",
        "Reprodutor Multim¡dia",
        "Richiesta supporto",
        "RICOH Driver",
        "Riquadro comandi",
        "Rodel Agent",
        "RTD - Rapid Torrent Downloader",
        "RyTuneX",
        "Safe",
        "Samsung Flow",
        "Samsung Printer Experience",
        "Scan",
        "Scanner",
        "Scanneur",
        "Screen Sketch",
        "Screenbox",
        "ScreenToGif",
        "Screenshot Snipping Tool",
        "Seelen UI",
        "Seguran‡a do Windows",
        "Seguridad",
        "Seguridad de Windows",
        "Segurança",
        "SEIKOEPSONCORPORATION.EpsonPrintandScan_ezaqdwkaef94e",
        "Sécurité",
        "ShareEnum",
        "ShellRunas",
        "ShowKeyPlus",
        "Sicurezza di Windows",
        "Simple HTTP Server",
        "Simple Video Trim & Merge",
        "Simple Word Search",
        "SimplePanoramaViewer",
        "SketchBook",
        "Skype",
        "SkypeApp",
        "Skype Preview",
        "SLU Service",
        "Smart File Renamer ù",
        "SmartAudio 2",
        "Smarters IPTV",
        "Snip & Sketch",
        "Snipping Tool",
        "Solidigm SynergyT Toolkit",
        "Solitaire & Casual Games",
        "Sottosistema Windows per AndroidT",
        "Sound Blaster Connect",
        "Speedtest",
        "Speedy Duplicate Finder",
        "Spooky View",
        "Spotify",
        "Sticky Notes",
        "Strumento di cattura",
        "Sugerencias",
        "Suggerimenti",
        "Surface",
        "Sway",
        "Sweet Home",
        "Sweet Home 3D",
        "Sysinternals Suite",
        "SysTools ISO Converter",
        "Szybka pomoc",
        "Takvim",
        "TCPView",
        "Telegram",
        "Telegram Desktop",
        "Telefoonkoppeling",
        "Terminal",
        "Terminale",
        "Terminal Preview",
        "Text Reader",
        "ThunderboltT",
        "ThunderboltT Control Center",
        "Tips",
        "Tobii Experience",
        "ToDo",
        "TopNotify",
        "Torrent Downloader HD",
        "Torrent RT FREE",
        "Torrex",
        "Traducteur",
        "Translator",
        "TranslucentTB",
        "Transitions DJ",
        "TuneIn Radio",
        "TvMate",
        "Ubuntu",
        "Unigram",
        "User Experience Improvement Program",
        "Victrix Control Hub",
        "Video Player+",
        "Vidogram",
        "Vincular ao Celular",
        "Virtual Sticky Notes",
        "Visor 3D",
        "Visor de datos de diagnóstico",
        "Visualizador 3D",
        "Visualizzatore 3D",
        "Visionneuse",
        "Visionneuse 3D",
        "Visum Visionneuse de photo",
        "VLC",
        "VMMap",
        "Vreme",
        "WavesAudio.WavesMaxxAudioProforDell_fh4rh281wavaa",
        "Weather",
        "Weer",
        "WhatsApp",
        "Wi-Fi",
        "WinDbg",
        "WindowTop",
        "Windows App",
        "Windows File Recovery",
        "Windows Folder Organizer",
        "Windows HDR",
        "Windows HDR Calibration",
        "Windows Security",
        "Windows Subsystem",
        "Windows Subsystem for AndroidT",
        "Windows Terminal",
        "Windows-Sicherheit",
        "windows.immersivecontrolpanel",
        "Wino Mail",
        "Wireless Display Adapter",
        "WinObj",
        "Wintoys",
        "Wonderwall",
        "Word",
        "WunderMail for Gmail",
        "Xerox Print and Scan Experience",
        "Xbox Console Companion",
        "YourPhone",
        "ZeroDev.RemoteKeyboardDesktop_7sg2dz33ww9gp",
        "ZoomIt",
        "ZuneMusic",
        "ZuneVideo",
        "µruh z"
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
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
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
                    var newPrograms = potentialGames.Select(static p => p.Name).ToList();
                    if (newPrograms.Count != 0)
                    {
                        var reportContent = new StringBuilder();
                        reportContent.AppendLine("--- Microsoft Store Scan Results ---");
                        reportContent.AppendLine("The following programs were found and are not on the ignore list:");
                        foreach (var programName in newPrograms.OrderBy(static n => n))
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
                        await Task.Run(() => File.Copy(fullLogoPath, destPath));
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
                            await Task.Run(() => File.Copy(p, destPath));
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
                    .Where(static f => f.Contains("targetsize", StringComparison.OrdinalIgnoreCase) || f.Contains("scale", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(static f => new FileInfo(f).Length) // Bigger is usually better quality
                    .FirstOrDefault();

                if (bestIcon != null)
                {
                    try
                    {
                        await Task.Run(() => File.Copy(bestIcon, destPath));
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
                    var largestPng = pngs.OrderByDescending(static f => new FileInfo(f).Length).FirstOrDefault();
                    if (largestPng != null)
                    {
                        try
                        {
                            await Task.Run(() => File.Copy(largestPng, destPath));
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