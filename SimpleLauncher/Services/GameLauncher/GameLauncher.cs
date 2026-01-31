using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.ExtractFiles;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.LoadAppSettings;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.MountFiles;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.TrayIcon;
using SimpleLauncher.Services.UsageStats;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher;

public class GameLauncher
{
    private readonly string _logPath = GetLogPath.Path();
    private SystemManager.SystemManager.Emulator _selectedEmulatorManager;
    private string _selectedEmulatorParameters;
    private const int MemoryAccessViolation = -1073741819;
    private const int DepViolation = -1073740791;
    private readonly IExtractionService _extractionService;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Stats _stats;

    public GameLauncher(IExtractionService extractionService, PlaySoundEffects playSoundEffects, Stats stats)
    {
        _extractionService = extractionService ?? throw new ArgumentNullException(nameof(extractionService));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _stats = stats ?? throw new ArgumentNullException(nameof(stats));
    }

    internal async Task HandleButtonClickAsync(string filePath, string selectedEmulatorName, string selectedSystemName, SystemManager.SystemManager selectedSystemManager, SettingsManager.SettingsManager settings, MainWindow mainWindow, GamePadController gamePadController)
    {
        try
        {
            // Resolve the path first to get a full, canonical path.
            var resolvedFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath);

            // Check existence using both standard and long-path prefixed versions to ensure maximum compatibility
            // across different drive types (local, removable, network).
            var pathForCheck = resolvedFilePath.StartsWith(@"\\?\", StringComparison.Ordinal) ? resolvedFilePath : @"\\?\" + resolvedFilePath;
            var exists = File.Exists(resolvedFilePath) || File.Exists(pathForCheck) || Directory.Exists(resolvedFilePath) || Directory.Exists(pathForCheck);

            if (string.IsNullOrWhiteSpace(resolvedFilePath) || !exists)
            {
                // Notify developer - pass a real exception instead of null to avoid "Exception is null" logs
                var contextMessage = $"Invalid resolvedFilePath or file/directory does not exist.\n\n" +
                                     $"Original filePath: {filePath}\n" +
                                     $"Resolved filePath: {resolvedFilePath}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new FileNotFoundException(contextMessage), contextMessage);

                // Notify user
                MessageBoxLibrary.FilePathIsInvalid(_logPath);
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedEmulatorName))
            {
                // Notify developer
                const string contextMessage = "[HandleButtonClickAsync] selectedEmulatorName is null or empty.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);

                return;
            }

            if (selectedSystemName == null)
            {
                // Notify developer
                const string contextMessage = "selectedSystemName is null.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);

                return;
            }

            if (selectedSystemManager == null)
            {
                // Notify developer
                const string contextMessage = "selectedSystemManager is null";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);

                return;
            }

            _selectedEmulatorManager = selectedSystemManager.Emulators.FirstOrDefault(e => e.EmulatorName.Equals(selectedEmulatorName, StringComparison.OrdinalIgnoreCase));
            if (_selectedEmulatorManager == null)
            {
                // Notify developer
                const string contextMessage = "_selectedEmulatorManager is null.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);

                return;
            }

            if (string.IsNullOrWhiteSpace(_selectedEmulatorManager.EmulatorName))
            {
                // Notify developer
                const string contextMessage = "_selectedEmulatorManager.EmulatorName is null.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);

                return;
            }

            _selectedEmulatorParameters = _selectedEmulatorManager.EmulatorParameters;

            // --- XENIA CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Xenia", StringComparison.OrdinalIgnoreCase) ||
                selectedEmulatorName.Contains("Xenia Canary", StringComparison.OrdinalIgnoreCase) ||
                selectedEmulatorName.Contains("XeniaCanary", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains("xenia.exe", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains("xenia_canary.exe", StringComparison.OrdinalIgnoreCase))
            {
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                var shouldRun = false;
                if (settings.XeniaShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var xeniaWindow = new InjectXeniaConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        xeniaWindow.ShowDialog();
                        shouldRun = xeniaWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject the settings into the Xenia config file since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        XeniaConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled the launch
            }

            // --- MAME CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase) ||
                selectedSystemName.Contains("MAME", StringComparison.OrdinalIgnoreCase))
            {
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystemManager.PrimarySystemFolder);
                var shouldRun = false;
                if (settings.MameShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var mameWindow = new InjectMameConfigWindow(settings, resolvedEmulatorExePath, resolvedSystemFolderPath) { Owner = mainWindow };
                        mameWindow.ShowDialog();
                        shouldRun = mameWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into mame.ini since window was skipped
                    MameConfigurationService.InjectSettings(resolvedEmulatorExePath, settings, resolvedSystemFolderPath);
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- RETROARCH CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("RetroArch", StringComparison.OrdinalIgnoreCase) ||
                filePath.Contains("retroarch.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.RetroArchShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var raWindow = new InjectRetroArchConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        raWindow.ShowDialog();
                        shouldRun = raWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into retroarch.cfg since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        RetroArchConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- SUPERMODEL CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Supermodel", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("Supermodel.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.SupermodelShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var supermodelWindow = new InjectSupermodelConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        supermodelWindow.ShowDialog();
                        shouldRun = supermodelWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into Supermodel.ini since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        SupermodelConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- MEDNAFEN CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Mednafen", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("mednafen.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.MednafenShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var mednafenWindow = new InjectMednafenConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        mednafenWindow.ShowDialog();
                        shouldRun = mednafenWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into mednafen.cfg since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        MednafenConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- SEGA MODEL 2 CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Model 2", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("emulator.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.SegaModel2ShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var segaModel2Window = new InjectSegaModel2ConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        segaModel2Window.ShowDialog();
                        shouldRun = segaModel2Window.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into EMULATOR.INI since window was skipped
                    SegaModel2ConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- ARES CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Ares", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("ares.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.AresShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var aresWindow = new InjectAresConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        aresWindow.ShowDialog();
                        shouldRun = aresWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into settings.bml since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        AresConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- DAPHNE CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Daphne", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("daphne.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                if (settings.DaphneShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var daphneWindow = new InjectDaphneConfigWindow(settings, null) { Owner = mainWindow };
                        daphneWindow.ShowDialog();
                        shouldRun = daphneWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // No file injection needed; arguments are built dynamically below.
                }

                if (!shouldRun) return; // User cancelled

                var daphneArgs = DaphneConfigurationService.BuildArguments(settings);
                _selectedEmulatorParameters = $"{_selectedEmulatorParameters} {daphneArgs}".Trim();
            }

            // --- BLASTEM CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Blastem", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("blastem.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.BlastemShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var blastemWindow = new InjectBlastemConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        blastemWindow.ShowDialog();
                        shouldRun = blastemWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into default.cfg since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        BlastemConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- MESEN CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Mesen", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("Mesen.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.MesenShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var mesenWindow = new InjectMesenConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        mesenWindow.ShowDialog();
                        shouldRun = mesenWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into settings.json since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        MesenConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- DUCKSTATION CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("DuckStation", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("duckstation", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.DuckStationShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var duckstationWindow = new InjectDuckStationConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        duckstationWindow.ShowDialog();
                        shouldRun = duckstationWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into settings.ini since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        DuckStationConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- RPCS3 CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("rpcs3.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.Rpcs3ShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var rpcs3Window = new InjectRpcs3ConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        rpcs3Window.ShowDialog();
                        shouldRun = rpcs3Window.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into config.yml since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        Rpcs3ConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- FLYCAST CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Flycast", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("flycast.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.FlycastShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var flycastWindow = new InjectFlycastConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        flycastWindow.ShowDialog();
                        shouldRun = flycastWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into emu.cfg since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        FlycastConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            // --- STELLA CONFIGURATION INTERCEPTION ---
            if (selectedEmulatorName.Contains("Stella", StringComparison.OrdinalIgnoreCase) ||
                _selectedEmulatorManager.EmulatorLocation.Contains("stella.exe", StringComparison.OrdinalIgnoreCase))
            {
                var shouldRun = false;
                var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(_selectedEmulatorManager.EmulatorLocation);
                if (settings.StellaShowSettingsBeforeLaunch)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var stellaWindow = new InjectStellaConfigWindow(settings, resolvedEmulatorExePath) { Owner = mainWindow };
                        stellaWindow.ShowDialog();
                        shouldRun = stellaWindow.ShouldRun;
                    });
                }
                else
                {
                    shouldRun = true;
                    // Inject settings into stella.sqlite3 since window was skipped
                    if (!string.IsNullOrEmpty(resolvedEmulatorExePath) && File.Exists(resolvedEmulatorExePath))
                    {
                        StellaConfigurationService.InjectSettings(resolvedEmulatorExePath, settings);
                    }
                }

                if (!shouldRun) return; // User cancelled
            }

            var wasGamePadControllerRunning = gamePadController.IsRunning;
            if (wasGamePadControllerRunning)
            {
                gamePadController.Stop();
            }

            var startTime = DateTime.Now;
            mainWindow.IsLoadingGames = true;

            try
            {
                // Check for GroupByFolder compatibility before proceeding with any launch logic
                if (selectedSystemManager.GroupByFolder)
                {
                    var emulatorName = _selectedEmulatorManager.EmulatorName ?? string.Empty;
                    var emulatorLocation = _selectedEmulatorManager.EmulatorLocation ?? string.Empty;

                    var isMame = emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
                                 emulatorLocation.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ||
                                 emulatorLocation.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase);

                    if (!isMame)
                    {
                        MessageBoxLibrary.GroupByFolderOnlyForMameMessageBox();
                        return; // Abort launch. The 'finally' block will handle cleanup.
                    }
                }

                // Specific handling for Cxbx-Reloaded
                if (selectedEmulatorName.Contains("Cxbx", StringComparison.OrdinalIgnoreCase) &&
                    Path.GetExtension(resolvedFilePath).Equals(".iso", StringComparison.OrdinalIgnoreCase))
                {
                    DebugLogger.Log($"Cxbx-Reloaded call detected. Attempting to mount and launch: {resolvedFilePath}");
                    await using var mountedDrive = await MountXisoFiles.MountAsync(resolvedFilePath, _logPath);
                    if (mountedDrive.IsMounted)
                    {
                        DebugLogger.Log($"ISO mounted successfully. Proceeding to launch {mountedDrive.MountedPath} with {selectedEmulatorName}.");
                        // Launch default.xbe
                        await LaunchRegularEmulatorAsync(mountedDrive.MountedPath, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, this);
                        DebugLogger.Log($"Emulator for {mountedDrive.MountedPath} has exited. Unmounting will occur automatically.");
                    }
                    else
                    {
                        DebugLogger.Log("ISO mounting failed. The user has been notified. Aborting launch.");
                        // User is already notified by MountAsync on failure.
                    }
                }

                // Specific handling for ScummVM games with ZIP files
                else if ((selectedSystemName.Contains("ScummVM", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("Scumm-VM", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("Scumm", StringComparison.OrdinalIgnoreCase))
                         && Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    DebugLogger.Log($"ScummVM game with ZIP call detected. Attempting to mount ZIP and launch: {resolvedFilePath}");
                    await MountZipFiles.MountZipFileAndLoadWithScummVmAsync(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, _logPath);
                }

                // Specific handling for RPCS3 with ZIP files
                else if (selectedEmulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) && Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    DebugLogger.Log($"RPCS3 with ZIP call detected. Attempting to mount ZIP and launch: {resolvedFilePath}");
                    await MountZipFiles.MountZipFileAndLoadEbootBinAsync(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, _logPath, this);
                }

                // Specific handling for XBLA games with ZIP files
                else if ((selectedSystemName.Contains("xbla", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("xbox live", StringComparison.OrdinalIgnoreCase) || selectedSystemName.Contains("live arcade", StringComparison.OrdinalIgnoreCase) || resolvedFilePath.Contains("xbla", StringComparison.OrdinalIgnoreCase))
                         && Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    DebugLogger.Log($"XBLA game with ZIP call detected. Attempting to mount ZIP and launch: {resolvedFilePath}");
                    await MountZipFiles.MountZipFileAndSearchForFileToLoadAsync(resolvedFilePath, selectedSystemName, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, _logPath, this);
                }
                else
                {
                    var fileExtension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();
                    switch (fileExtension)
                    {
                        case ".BAT":
                            await RunBatchFileAsync(resolvedFilePath, _selectedEmulatorManager, mainWindow);
                            break;
                        case ".LNK":
                        case ".URL":
                            await LaunchShortcutFileAsync(resolvedFilePath, _selectedEmulatorManager, mainWindow);
                            break;
                        case ".EXE":
                            await LaunchExecutableAsync(resolvedFilePath, _selectedEmulatorManager, mainWindow);
                            break;
                        default:
                            await LaunchRegularEmulatorAsync(resolvedFilePath, selectedEmulatorName, selectedSystemManager, _selectedEmulatorManager, _selectedEmulatorParameters, mainWindow, this);
                            break;
                    }
                }
            }
            catch (Win32Exception ex) // Catch Win32Exception specifically
            {
                if (CheckApplicationControlPolicy.CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
                {
                    // Specific handling for application control policy block
                    MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked launching game/tool.");
                }
                else
                {
                    // Existing error handling for other Win32Exceptions
                    // Notify developer
                    var contextMessage = $"Unhandled error in GameLauncher's main launch block.\n" +
                                         $"FilePath: {resolvedFilePath}\n" +
                                         $"SelectedSystem: {selectedSystemName}\n" +
                                         $"SelectedEmulator: {selectedEmulatorName}";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                    // Notify user
                    _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                var contextMessage = $"Unhandled error in GameLauncher's main launch block.\n" +
                                     $"FilePath: {resolvedFilePath}\n" +
                                     $"SelectedSystem: {selectedSystemName}\n" +
                                     $"SelectedEmulator: {selectedEmulatorName}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                // Notify user
                _ = MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);
            }
            finally
            {
                mainWindow.IsLoadingGames = false;
                if (wasGamePadControllerRunning)
                {
                    gamePadController.Start();
                }

                var endTime = DateTime.Now;
                var playTime = endTime - startTime;

                if (playTime.TotalSeconds > 5)
                {
                    UpdateStatsAndPlayCountAsync(playTime, resolvedFilePath);
                }
            }
        }
        catch (Exception e)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(e, "Unhandled error in GameLauncher's main launch block.");
        }

        return;

        void UpdateStatsAndPlayCountAsync(TimeSpan playTime, string resolvedFilePath)
        {
            settings.UpdateSystemPlayTime(selectedSystemName, playTime);
            settings.Save();
            var playTimeFormatted = playTime.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture);
            DebugLogger.Log($"PlayTime saved: {playTimeFormatted}");

            var playTime2 = (string)Application.Current.TryFindResource("Playtime") ?? "Playtime";
            TrayIconManager.ShowTrayMessage($"{playTime2}: {playTimeFormatted}");
            UpdateStatusBar.UpdateStatusBar.UpdateContent("", mainWindow);

            try
            {
                var playHistoryManager = mainWindow.PlayHistoryManager;
                playHistoryManager.AddOrUpdatePlayHistoryItem(resolvedFilePath, selectedSystemName, playTime);
                mainWindow.RefreshGameListAfterPlay(resolvedFilePath, selectedSystemName);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error updating play history";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            }

            var systemPlayTime = settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystemName);
            if (systemPlayTime != null)
            {
                mainWindow.PlayTime = systemPlayTime.PlayTime;
                DebugLogger.Log($"System PlayTime updated: {systemPlayTime.PlayTime}");
            }

            // Update stats
            _ = _stats.CallApiAsync(selectedEmulatorName);
        }
    }

    private async Task RunBatchFileAsync(string resolvedFilePath, SystemManager.SystemManager.Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        // On Windows, .bat files are not direct executables.
        // To redirect output (UseShellExecute = false), we must run cmd.exe /c "path_to_script.bat"
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"{resolvedFilePath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true, // Hide the console window
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        // Set the working directory to the directory of the batch file
        try
        {
            var workingDirectory = Path.GetDirectoryName(resolvedFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Could not get workingDirectory for batch file: '{resolvedFilePath}'. Using default.");

            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        var launched = (string)Application.Current.TryFindResource("Launched") ?? "launched";
        DebugLogger.Log("RunBatchFileAsync:\n\n");
        DebugLogger.Log($"Command: {psi.FileName} {psi.Arguments}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}\n");

        TrayIconManager.ShowTrayMessage($"{Path.GetFileName(resolvedFilePath)} {launched}");
        UpdateStatusBar.UpdateStatusBar.UpdateContent($"{Path.GetFileName(resolvedFilePath)} {launched}", mainWindow);

        using var process = new Process();
        process.StartInfo = psi;

        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                error.AppendLine(args.Data);
            }
        };

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the cmd.exe process for the batch file.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                // Notify developer
                var errorDetail = $"There was an issue running the batch process.\n" +
                                  $"Batch file: {resolvedFilePath}\n" +
                                  $"Exit code {process.ExitCode}\n" +
                                  $"Output: {output}\n" +
                                  $"Error: {error}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
                }
            }
        }
        catch (Win32Exception ex)
        {
            if (CheckApplicationControlPolicy.CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked launching batch file.");
            }
            else
            {
                string exitCodeInfo;
                try
                {
                    exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}";
                }
                catch (InvalidOperationException)
                {
                    exitCodeInfo = "Exit code: N/A (Process not associated)";
                }

                var errorDetail = $"Exception running the batch process.\n" +
                                  $"Batch file: {resolvedFilePath}\n" +
                                  $"{exitCodeInfo}\n" +
                                  $"Exception: {ex.Message}\n" +
                                  $"Output: {output}\n" +
                                  $"Error: {error}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string exitCodeInfo;
            try
            {
                exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}";
            }
            catch (InvalidOperationException)
            {
                exitCodeInfo = "Exit code: N/A (Process not associated)";
            }

            var errorDetail = $"Exception running the batch process.\n" +
                              $"Batch file: {resolvedFilePath}\n" +
                              $"{exitCodeInfo}\n" +
                              $"Exception: {ex.Message}\n" +
                              $"Output: {output}\n" +
                              $"Error: {error}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n{userNotified}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
            }
        }
    }

    private async Task LaunchShortcutFileAsync(string resolvedFilePath, SystemManager.SystemManager.Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        // Common UI updates.
        var launched = (string)Application.Current.TryFindResource("Launched") ?? "launched";
        var fileName = Path.GetFileName(resolvedFilePath);
        TrayIconManager.ShowTrayMessage($"{fileName} {launched}");
        UpdateStatusBar.UpdateStatusBar.UpdateContent($"{fileName} {launched}", mainWindow);

        try
        {
            var extension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();

            // Validate file exists first
            if (!File.Exists(@"\\?\" + resolvedFilePath))
            {
                throw new FileNotFoundException($"Shortcut file not found: {resolvedFilePath}");
            }

            if (extension == ".URL")
            {
                // Read and validate the .url file content
                var urlContent = await File.ReadAllTextAsync(resolvedFilePath);
#pragma warning disable SYSLIB1045
                var urlMatch = System.Text.RegularExpressions.Regex.Match(urlContent, @"URL=(.+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
#pragma warning restore SYSLIB1045

                if (!urlMatch.Success || string.IsNullOrWhiteSpace(urlMatch.Groups[1].Value))
                {
                    throw new InvalidOperationException($"Invalid .url file format or missing URL in: {resolvedFilePath}");
                }

                var targetUrl = urlMatch.Groups[1].Value.Trim();
                DebugLogger.Log($"LaunchShortcutFileAsync (.URL):\n\nShortcut File: {resolvedFilePath}\nTarget URL: {targetUrl}\n");

                // Verify protocol handler is registered ONLY if it's a real URI (contains ://)
                // This prevents treating drive letters (C:\) as protocols.
                var protocolIndex = targetUrl.IndexOf("://", StringComparison.Ordinal);
                if (protocolIndex > 0)
                {
                    var protocol = targetUrl[..protocolIndex];
                    if (!IsProtocolRegistered(protocol))
                    {
                        throw new InvalidOperationException($"Protocol handler for '{protocol}://' is not registered. Please ensure the associated application is installed.");
                    }
                }

                // Use shell execution with explicit working directory set to null
                var psi = new ProcessStartInfo
                {
                    FileName = targetUrl, // Launch the URL directly, not the .url file
                    UseShellExecute = true,
                    WorkingDirectory = null // Explicitly no working directory
                };

                using var process = new Process();
                process.StartInfo = psi;

                // For shell execution, Start() might return false if a process was reused.
                // Win32Exception will be thrown if it actually fails.
                process.Start();
            }
            else // .LNK files
            {
                var psi = new ProcessStartInfo
                {
                    FileName = resolvedFilePath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(resolvedFilePath) ?? AppDomain.CurrentDomain.BaseDirectory
                };

                DebugLogger.Log($"LaunchShortcutFileAsync (.LNK):\n\nShortcut File: {psi.FileName}\nWorking Directory: {psi.WorkingDirectory}\n");

                using var process = new Process();
                process.StartInfo = psi;

                // For shell execution, Start() might return false if a process was reused.
                process.Start();
            }
        }
        catch (Win32Exception ex) // Catch Win32Exception specifically
        {
            if (CheckApplicationControlPolicy.CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked launching shortcut file.");
            }
            else
            {
                // Existing error handling for other Win32Exceptions
                var fileContent = File.Exists(@"\\?\" + resolvedFilePath)
                    ? $"\nFile Content:\n{await File.ReadAllTextAsync(resolvedFilePath)}"
                    : "\nFile does not exist.";
                var errorDetail = $"Exception launching the shortcut file.\n" +
                                  $"Shortcut file: {resolvedFilePath}\n" +
                                  $"Exception: {ex.Message}" +
                                  fileContent;
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    var launchErrorTitle = (string)Application.Current.TryFindResource("LaunchErrorTitle") ?? "Launch Error";
                    MessageBoxLibrary.ShowCustomMessageBox("There was a Win32Exception.", launchErrorTitle, _logPath);
                }
            }
        }
        catch (Exception ex)
        {
            // Only attempt to read file content if it's a text-based .URL file
            var isUrlFile = Path.GetExtension(resolvedFilePath).Equals(".url", StringComparison.OrdinalIgnoreCase);
            var fileContent = isUrlFile && File.Exists(resolvedFilePath)
                ? $"\nFile Content:\n{await File.ReadAllTextAsync(resolvedFilePath)}"
                : "\nFile content not displayed (binary .LNK or missing file).";

            var errorDetail = $"Exception launching the shortcut file.\n" +
                              $"Shortcut file: {resolvedFilePath}\n" +
                              $"Exception: {ex.Message}" +
                              fileContent;
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n{userNotified}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                var launchErrorTitle = (string)Application.Current.TryFindResource("LaunchErrorTitle") ?? "Launch Error";
                var couldNotLaunchShortcut = (string)Application.Current.TryFindResource("CouldNotLaunchShortcut") ?? "Could not launch the game shortcut. The protocol handler may not be installed. Please ensure the game launcher (Steam, GOG Galaxy, etc.) is installed.";
                var userMessage = ex switch
                {
                    FileNotFoundException => $"Shortcut file not found: {Path.GetFileName(resolvedFilePath)}.",
                    InvalidOperationException when ex.Message.Contains("Protocol handler for", StringComparison.OrdinalIgnoreCase) => ex.Message,
                    _ => couldNotLaunchShortcut
                };

                MessageBoxLibrary.ShowCustomMessageBox(userMessage, launchErrorTitle, _logPath);
            }
        }
    }

    private async Task LaunchExecutableAsync(string resolvedFilePath, SystemManager.SystemManager.Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        var psi = new ProcessStartInfo
        {
            FileName = resolvedFilePath,
            Arguments = "", // No arguments for direct executable launch unless specified
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        try
        {
            var workingDirectory = Path.GetDirectoryName(resolvedFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Could not get workingDirectory for executable file: '{resolvedFilePath}'. Using default.");

            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        var launched = (string)Application.Current.TryFindResource("Launched") ?? "launched";
        DebugLogger.Log("LaunchExecutableAsync:\n\n");
        DebugLogger.Log($"Executable File: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}\n");

        TrayIconManager.ShowTrayMessage($"{Path.GetFileName(psi.FileName)} {launched}");
        UpdateStatusBar.UpdateStatusBar.UpdateContent($"{Path.GetFileName(psi.FileName)} {launched}", mainWindow);

        using var process = new Process();
        process.StartInfo = psi;

        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data)) output.AppendLine(args.Data);
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data)) error.AppendLine(args.Data);
        };

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the executable process.");
            }

            if (!process.HasExited)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
            }

            if (process.ExitCode != 0)
            {
                // Notify developer
                var errorDetail = $"Executable process exited with non-zero code.\n" +
                                  $"Executable file: {psi.FileName}\n" +
                                  $"Exit code {process.ExitCode}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
                }
            }
        }
        catch (Win32Exception ex) // Catch Win32Exception specifically
        {
            if (CheckApplicationControlPolicy.CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked launching executable.");
            }
            else
            {
                // Existing error handling for other Win32Exceptions
                string exitCodeInfo;
                try
                {
                    exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}";
                }
                catch (InvalidOperationException)
                {
                    exitCodeInfo = "Exit code: N/A (Process not associated)";
                }

                var errorDetail = $"Exception launching the executable file.\n" +
                                  $"Executable file: {psi.FileName}\n" +
                                  $"{exitCodeInfo}\n" +
                                  $"Exception: {ex.Message}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string exitCodeInfo;
            try
            {
                exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}";
            }
            catch (InvalidOperationException)
            {
                exitCodeInfo = "Exit code: N/A (Process not associated)";
            }

            var errorDetail = $"Exception launching the executable file.\n" +
                              $"Executable file: {psi.FileName}\n" +
                              $"{exitCodeInfo}\n" +
                              $"Exception: {ex.Message}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n{userNotified}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(_logPath);
            }
        }
    }

    internal async Task LaunchRegularEmulatorAsync(
        string resolvedFilePath,
        string selectedEmulatorName,
        SystemManager.SystemManager selectedSystemManager,
        SystemManager.SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        GameLauncher gameLauncher)
    {
        var isDirectory = Directory.Exists(resolvedFilePath);

        if (string.IsNullOrEmpty(selectedEmulatorName))
        {
            // Notify developer
            const string contextMessage = "[LaunchRegularEmulatorAsync] selectedEmulatorName is null or empty.";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            return;
        }

        // A simple and effective way to identify a mounted XBE path from our tool
        // is by its characteristic filename. This avoids hardcoding drive letters.
        var isMountedXbe = Path.GetFileName(resolvedFilePath).Equals("default.xbe", StringComparison.OrdinalIgnoreCase);

        // Check if the file to launch is a mounted ZIP file, which will not be extracted
        var isMountedZip = resolvedFilePath.StartsWith(MountZipFiles.ConfiguredMountDriveRoot, StringComparison.OrdinalIgnoreCase);

        // Declare tempExtractionPath here to be accessible in the finally block
        string tempExtractionPath = null;

        if (selectedSystemManager.ExtractFileBeforeLaunch && !isDirectory && !isMountedXbe && !isMountedZip)
        {
            if (selectedSystemManager.FileFormatsToLaunch == null || selectedSystemManager.FileFormatsToLaunch.Count == 0)
            {
                // Notify developer
                const string contextMessage = "FileFormatsToLaunch is null or empty, but ExtractFileBeforeLaunch is true for game launching. Cannot determine which file to launch after extraction.";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
                DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                // Notify user specifically for launching context
                MessageBoxLibrary.NullFileExtensionMessageBox(); // This is the correct place for this warning.
                return; // Abort launch due to incomplete configuration for launching
            }

            // Use the extraction service from the DI container
            var (extractedGameFilePath, extractedTempDirPath) = await _extractionService.ExtractToTempAndGetLaunchFileAsync(resolvedFilePath, selectedSystemManager.FileFormatsToLaunch);

            if (!string.IsNullOrEmpty(extractedGameFilePath))
            {
                resolvedFilePath = extractedGameFilePath;
            }

            // Always store the temp directory path for cleanup, even if no game file was found within it
            tempExtractionPath = extractedTempDirPath;
        }

        if (string.IsNullOrEmpty(resolvedFilePath))
        {
            // Notify developer
            const string contextMessage = "resolvedFilePath is null or empty after extraction attempt (or for mounted files).";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            // The finally block will handle cleanup of tempExtractionPath if it was set.
            return;
        }

        // For mounted files, ensure it still exists before proceeding
        if ((isMountedXbe || isMountedZip) && !File.Exists(@"\\?\" + resolvedFilePath))
        {
            // Notify developer
            var contextMessage = $"Mounted file {resolvedFilePath} not found when trying to launch with emulator.";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            return;
        }

        // Resolve the Emulator Path (executable)
        var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(selectedEmulatorManager.EmulatorLocation);
        if (string.IsNullOrEmpty(resolvedEmulatorExePath) || !File.Exists(@"\\?\" + resolvedEmulatorExePath))
        {
            // Notify developer
            var contextMessage = $"Emulator executable path is null, empty, or does not exist after resolving: '{selectedEmulatorManager.EmulatorLocation}' -> '{resolvedEmulatorExePath}'";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new FileNotFoundException(contextMessage), "Emulator configuration error.");
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            return;
        }

        // Determine the emulator's directory, which is the base for %EMULATORFOLDER%
        var resolvedEmulatorFolderPath = Path.GetDirectoryName(resolvedEmulatorExePath);
        if (string.IsNullOrEmpty(resolvedEmulatorFolderPath) || !Directory.Exists(@"\\?\" + resolvedEmulatorFolderPath)) // Should exist if exe exists
        {
            // Notify developer
            var contextMessage = $"Could not determine emulator folder path from executable path: '{resolvedEmulatorExePath}'";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(_logPath);

            return;
        }

        // Resolve System Folder Path, which is the base for %SYSTEMFOLDER%
        var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystemManager.PrimarySystemFolder);
        // Note: SystemFolder might not be strictly required to exist for all emulators/parameters,
        // but if %SYSTEMFOLDER% is used in parameters, this path needs to be valid.

        // Resolve Emulator Parameters using the PathHelper.ResolveParameterString
        var resolvedParameters = PathHelper.ResolveParameterString(
            rawEmulatorParameters, // The raw parameter string from config
            resolvedSystemFolderPath, // The fully resolved system folder path
            resolvedEmulatorFolderPath // The fully resolved emulator directory path
        );

        string arguments;

        // Handling MAME Related Games
        // Will load the filename without the extension
        if (selectedEmulatorName.Equals("MAME", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorManager.EmulatorLocation.Contains("mame.exe", StringComparison.OrdinalIgnoreCase))
        {
            string mameRomName;
            if (isDirectory)
            {
                mameRomName = Path.GetFileName(resolvedFilePath);
            }
            else
            {
                mameRomName = Path.GetFileNameWithoutExtension(resolvedFilePath);
            }

            DebugLogger.Log($"MAME call detected. Attempting to launch: {mameRomName}");

            arguments = $"{resolvedParameters} \"{mameRomName}\"";
        }
        else // General call - Provide full filepath
        {
            arguments = $"{resolvedParameters} \"{resolvedFilePath}\"";
        }

        string workingDirectory;
        try
        {
            // Set the working directory to the directory of the emulator executable
            workingDirectory = resolvedEmulatorFolderPath;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Could not get workingDirectory for emulator: '{resolvedEmulatorFolderPath}'. Using default.");
            DebugLogger.Log($"Could not get workingDirectory for emulator: '{resolvedEmulatorFolderPath}'. Using default.");

            workingDirectory = AppDomain.CurrentDomain.BaseDirectory; // fallback
        }

        var psi = new ProcessStartInfo
        {
            FileName = resolvedEmulatorExePath,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        DebugLogger.Log($"LaunchRegularEmulatorAsync:\n\n" +
                        $"Program Location: {resolvedEmulatorExePath}\n" +
                        $"Arguments: {arguments}\n" +
                        $"Working Directory: {psi.WorkingDirectory}\n" +
                        $"File to launch: {resolvedFilePath}");

        var fileName = Path.GetFileNameWithoutExtension(resolvedFilePath);
        var launchedwith = (string)Application.Current.TryFindResource("launchedwith") ?? "launched with";
        TrayIconManager.ShowTrayMessage($"{fileName} {launchedwith} {selectedEmulatorName}");
        UpdateStatusBar.UpdateStatusBar.UpdateContent($"{fileName} {launchedwith} {selectedEmulatorName}", mainWindow);

        using var process = new Process();
        process.StartInfo = psi;
        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                output.AppendLine(args.Data);
            }
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
            {
                error.AppendLine(args.Data);
            }
        };

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the process.");
            }

            if (!process.HasExited)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();
            }

            if (process.HasExited)
            {
                if (DoNotCheckErrorsOnSpecificEmulators(selectedEmulatorName, resolvedEmulatorExePath, process, psi, output, error)) return;

                await CheckForMemoryAccessViolationAsync(process, psi, output, error);
                await CheckForDepViolationAsync(process, psi, output, error);
                await CheckForExitCodeWithErrorAnyAsync(process, psi, output, error, selectedEmulatorManager);
            }
        }
        catch (InvalidOperationException ex)
        {
            // Notify developer
            const string contextMessage = "InvalidOperationException while launching emulator.";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                await MessageBoxLibrary.InvalidOperationExceptionMessageBox(_logPath);
                SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(ex, contextMessage, gameLauncher, _playSoundEffects);
            }
        }
        catch (Win32Exception ex) // Catch Win32Exception specifically
        {
            if (CheckApplicationControlPolicy.CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked launching emulator.");
            }
            else
            {
                // Existing error handling for other Win32Exceptions
                // Notify developer
                // Safely check if the process ever started before trying to access its properties.
                // A simple way is to check if an ID was ever assigned.
                string exitCodeInfo;
                try
                {
                    // This check is safe even if the process didn't start.
                    _ = process.Id;
                    exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A (Still Running or Failed to get code)")}";
                }
                catch (InvalidOperationException)
                {
                    exitCodeInfo = "Exit code: N/A (Process failed to start)";
                }

                var errorDetail = $"{exitCodeInfo}\n" +
                                  $"Emulator: {psi.FileName}\n" +
                                  $"Calling parameters: {psi.Arguments}\n" +
                                  $"Emulator output: {output}\n" +
                                  $"Emulator error: {error}\n";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"The emulator could not open the game with the provided parameters. {userNotified}\n\n{errorDetail}";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    await MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);
                    SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(ex, contextMessage, gameLauncher, _playSoundEffects);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            // Safely check if the process ever started before trying to access its properties.
            // A simple way is to check if an ID was ever assigned.
            string exitCodeInfo;
            try
            {
                // This check is safe even if the process didn't start.
                _ = process.Id;
                exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A (Still Running or Failed to get code)")}";
            }
            catch (InvalidOperationException)
            {
                exitCodeInfo = "Exit code: N/A (Process failed to start)";
            }

            var errorDetail = $"{exitCodeInfo}\n" +
                              $"Emulator: {psi.FileName}\n" +
                              $"Calling parameters: {psi.Arguments}\n" +
                              $"Emulator output: {output}\n" +
                              $"Emulator error: {error}\n";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"The emulator could not open the game with the provided parameters. {userNotified}\n\n{errorDetail}";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                // Notify user
                await MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);
                SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(ex, contextMessage, gameLauncher, _playSoundEffects);
            }
        }
        finally
        {
            // Only attempt to delete if a temporary extraction path was actually set
            if (!string.IsNullOrEmpty(tempExtractionPath) && Directory.Exists(tempExtractionPath))
            {
                try
                {
                    DebugLogger.Log($"[LaunchRegularEmulatorAsync] Attempting to delete temporary extraction directory: {tempExtractionPath}");
                    Directory.Delete(tempExtractionPath, true); // Use Directory.Delete with recursive=true
                    DebugLogger.Log($"[LaunchRegularEmulatorAsync] Successfully deleted temporary extraction directory: {tempExtractionPath}");
                }
                catch (Exception ex)
                {
                    // Log the error but don't prevent other finally block actions
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to delete temporary extraction directory: {tempExtractionPath}");
                    DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error deleting temporary extraction directory {tempExtractionPath}: {ex.Message}");
                }
            }
        }
    }

    private async Task CheckForExitCodeWithErrorAnyAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, SystemManager.SystemManager.Emulator emulatorManager)
    {
        string contextMessage;

        // Ignore MemoryAccessViolation and DepViolation
        if (!process.HasExited || process.ExitCode == 0 || process.ExitCode == MemoryAccessViolation || process.ExitCode == DepViolation)
        {
            return;
        }

        // Check if the output contains "File open/read error" and ignore it,
        // Common RetroArch error that should be ignored
        if (output.ToString().Contains("File open/read error", StringComparison.OrdinalIgnoreCase))
        {
            DebugLogger.Log($"[CheckForExitCodeWithErrorAnyAsync] Ignored exit code {process.ExitCode} due to 'File open/read error' in output.");
            return;
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError)
        {
            // Notify developer
            contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                             $"User was notified.\n\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
        }
        else
        {
            // Notify developer
            contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                             $"User was not notified.\n\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError)
        {
            // Notify user
            await MessageBoxLibrary.CouldNotLaunchGameMessageBox(_logPath);
            SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(null, contextMessage, this, _playSoundEffects);
        }
    }

    private static Task CheckForMemoryAccessViolationAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        if (process.HasExited && process.ExitCode != MemoryAccessViolation)
        {
            return Task.CompletedTask;
        }

        // Notify developer
        var contextMessage = $"There was a memory access violation error running the emulator.\n" +
                             $"User was not notified.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

        return Task.CompletedTask;
    }

    private static Task CheckForDepViolationAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        if (process.HasExited && process.ExitCode != DepViolation) return Task.CompletedTask;

        // Notify developer
        var contextMessage = $"Data Execution Prevention (DEP) violation error occurred while running the emulator.\n" +
                             $"User was not notified.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

        return Task.CompletedTask;
    }

    private static bool DoNotCheckErrorsOnSpecificEmulators(string selectedEmulatorName, string resolvedEmulatorExePath, Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        if (selectedEmulatorName.Contains("Kega Fusion", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("KegaFusion", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Kega", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Fusion", StringComparison.OrdinalIgnoreCase) ||
            resolvedEmulatorExePath.Contains("Fusion.exe", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Project64", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Project 64", StringComparison.OrdinalIgnoreCase) ||
            resolvedEmulatorExePath.Contains("Project64.exe", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Emulicious", StringComparison.OrdinalIgnoreCase) ||
            resolvedEmulatorExePath.Contains("Emulicious.exe", StringComparison.OrdinalIgnoreCase) ||
            selectedEmulatorName.Contains("Speccy", StringComparison.OrdinalIgnoreCase) ||
            resolvedEmulatorExePath.Contains("Speccy.exe", StringComparison.OrdinalIgnoreCase))
        {
            // Notify developer
            var contextMessage = $"User just ran {selectedEmulatorName}.\n" +
                                 $"'Simple Launcher' do not track error codes for this emulator.\n\n" +
                                 $"Exit code: {process.ExitCode}\n" +
                                 $"Emulator: {psi.FileName}\n" +
                                 $"Calling parameters: {psi.Arguments}\n" +
                                 $"Emulator output: {output}\n" +
                                 $"Emulator error: {error}\n";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

            return true;
        }

        return false;
    }

    private static bool IsProtocolRegistered(string protocol)
    {
        if (string.IsNullOrEmpty(protocol)) return false;

        try
        {
            // Protocol names are case-insensitive in registry, but typically stored lowercase.
            // Ensure we check for the existence of the protocol key itself.
            using var protocolKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(protocol.ToLowerInvariant());
            if (protocolKey == null)
            {
                DebugLogger.Log($"[IsProtocolRegistered] Protocol key '{protocol.ToLowerInvariant()}' not found in HKEY_CLASSES_ROOT.");
                return false;
            }

            // A protocol is considered "registered" if it has a command handler defined.
            // This is typically found under shell\open\command.
            using var shellOpenCommandKey = protocolKey.OpenSubKey(@"shell\open\command");
            if (shellOpenCommandKey == null)
            {
                DebugLogger.Log($"[IsProtocolRegistered] 'shell\\open\\command' subkey not found for protocol '{protocol.ToLowerInvariant()}'.");
                return false;
            }

            var command = shellOpenCommandKey.GetValue(null) as string; // Default value
            if (string.IsNullOrWhiteSpace(command))
            {
                DebugLogger.Log($"[IsProtocolRegistered] Command handler is empty for protocol '{protocol.ToLowerInvariant()}'.");
                return false;
            }

            DebugLogger.Log($"[IsProtocolRegistered] Protocol '{protocol.ToLowerInvariant()}' is registered with command: '{command}'.");
            return true;
        }
        catch (Exception ex)
        {
            // Log any exceptions during registry access.
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error checking if protocol '{protocol}' is registered.");
            DebugLogger.Log($"[IsProtocolRegistered] Error checking protocol '{protocol}': {ex.Message}");
            return false;
        }
    }
}