using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.New.InjectConfigWindows;
using SimpleLauncher.New.ViewModels;

namespace SimpleLauncher.New.Services.GameLauncher.Handlers;

/// <summary>
/// All 21 emulator config handlers — simplified direct injection (no settings dialogs).
/// Each handler delegates to the corresponding Core ConfigurationService.InjectSettings().
/// </summary>
/// <summary>
/// RetroArch config handler — direct injection or dialog.
/// </summary>
public class RetroArchConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("RetroArch", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("retroarch.exe", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager is null || context.Settings is null) return false;

        var exePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exePath)) return false;

        // Show dialog if user opted into settings-before-launch
        if (context.Settings.RetroArch.ShowSettingsBeforeLaunch && context.WindowContext is not null)
        {
            var shouldRun = false;
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectRetroArchConfigViewModel(
                        context.Settings, null!, Log.Logger);
                    var win = new InjectRetroArchConfigWindow(vm)
                    {
                        Owner = context.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exePath);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        // Default: direct injection
        RetroArchConfigurationService.InjectSettings(exePath, context.Settings, Log.Logger);
        return true;
    }
}

/// <summary>
/// PCSX2 config handler — dialog or direct injection.
/// </summary>
public class Pcsx2ConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("PCSX2", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("pcsx2", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager is null || context.Settings is null) return false;

        var exePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exePath)) return false;

        if (context.Settings.Pcsx2.ShowSettingsBeforeLaunch && context.WindowContext is not null)
        {
            var shouldRun = false;
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectPcsx2ConfigViewModel(context.Settings, null!, Log.Logger);
                    var win = new InjectPcsx2ConfigWindow(vm)
                    {
                        Owner = context.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exePath);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        Pcsx2ConfigurationService.InjectSettings(exePath, context.Settings, Log.Logger);
        return true;
    }
}

/// <summary>
/// DuckStation config handler — direct injection.
/// </summary>
public class DuckStationConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("DuckStation", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("duckstation", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager is null || context.Settings is null) return false;

        var exePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exePath)) return false;

        if (context.Settings.DuckStation.ShowSettingsBeforeLaunch && context.WindowContext is not null)
        {
            var shouldRun = false;
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectDuckStationConfigViewModel(context.Settings, null!, Log.Logger);
                    var win = new InjectDuckStationConfigWindow(vm)
                    {
                        Owner = context.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exePath);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        DuckStationConfigurationService.InjectSettings(exePath, context.Settings, Log.Logger);
        return true;
    }
}

/// <summary>
/// Dolphin config handler — direct injection.
/// </summary>
public class DolphinConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Dolphin", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("Dolphin", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager is null || context.Settings is null) return false;

        var exePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exePath)) return false;

        if (context.Settings.Dolphin.ShowSettingsBeforeLaunch && context.WindowContext is not null)
        {
            var shouldRun = false;
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectDolphinConfigViewModel(context.Settings, null!, Log.Logger);
                    var win = new InjectDolphinConfigWindow(vm)
                    {
                        Owner = context.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exePath);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        DolphinConfigurationService.InjectSettings(exePath, context.Settings, Log.Logger);
        return true;
    }
}

/// <summary>
/// MAME config handler — direct injection.
/// </summary>
public class MameConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("mame", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager is null || context.Settings is null) return false;

        var exePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exePath)) return false;

        if (context.Settings.Mame.ShowSettingsBeforeLaunch && context.WindowContext is not null)
        {
            var shouldRun = false;
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectMameConfigViewModel(context.Settings, null!, Log.Logger);
                    var win = new InjectMameConfigWindow(vm)
                    {
                        Owner = context.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exePath);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        MameConfigurationService.InjectSettings(exePath, context.Settings, Log.Logger);
        return true;
    }
}

/// <summary>
/// Flycast config handler — direct injection.
/// </summary>
public class FlycastConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Flycast", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("flycast", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager is null || context.Settings is null) return false;

        var exePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exePath)) return false;

        if (context.Settings.Flycast.ShowSettingsBeforeLaunch && context.WindowContext is not null)
        {
            var shouldRun = false;
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectFlycastConfigViewModel(context.Settings, null!, Log.Logger);
                    var win = new InjectFlycastConfigWindow(vm)
                    {
                        Owner = context.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exePath);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        FlycastConfigurationService.InjectSettings(exePath, context.Settings, Log.Logger);
        return true;
    }
}

/// <summary>
/// RPCS3 config handler — direct injection.
/// </summary>
public class Rpcs3ConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("rpcs3", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager is null || context.Settings is null) return false;

        var exePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exePath)) return false;

        if (context.Settings.Rpcs3.ShowSettingsBeforeLaunch && context.WindowContext is not null)
        {
            var shouldRun = false;
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectRpcs3ConfigViewModel(context.Settings, null!, Log.Logger);
                    var win = new InjectRpcs3ConfigWindow(vm)
                    {
                        Owner = context.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exePath);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        Rpcs3ConfigurationService.InjectSettings(exePath, context.Settings, Log.Logger);
        return true;
    }
}

/// <summary>
/// Xenia config handler — direct injection.
/// </summary>
public class XeniaConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Xenia", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("xenia", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager is null || context.Settings is null) return false;

        var exePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exePath)) return false;

        if (context.Settings.Xenia.ShowSettingsBeforeLaunch && context.WindowContext is not null)
        {
            var shouldRun = false;
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectXeniaConfigViewModel(context.Settings, null!, Log.Logger);
                    var win = new InjectXeniaConfigWindow(vm)
                    {
                        Owner = context.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exePath);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        XeniaConfigurationService.InjectSettings(exePath, context.Settings, Log.Logger);
        return true;
    }
}

/// <summary>
/// Cemu config handler — direct injection.
/// </summary>
public class CemuConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string emulatorName, string emulatorPath)
    {
        return emulatorName.Contains("Cemu", StringComparison.OrdinalIgnoreCase) ||
               (emulatorPath?.Contains("Cemu", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext context)
    {
        if (context.EmulatorManager is null || context.Settings is null) return false;

        var exePath = PathHelper.ResolveRelativeToAppDirectory(context.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exePath)) return false;

        if (context.Settings.Cemu.ShowSettingsBeforeLaunch && context.WindowContext is not null)
        {
            var shouldRun = false;
            await context.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectCemuConfigViewModel(context.Settings, null!, Log.Logger);
                    var win = new InjectCemuConfigWindow(vm)
                    {
                        Owner = context.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exePath);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        CemuConfigurationService.InjectSettings(exePath, context.Settings, Log.Logger);
        return true;
    }
}

// ── Remaining 12 handlers follow the same pattern ──
// Ares, Azahar, Blastem, Daphne, Mednafen, Mesen, Raine, Redream,
// SegaModel2, Stella, Supermodel, Yumir

public class AresConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Ares", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return false;

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return false;

        if (c.Settings.Ares.ShowSettingsBeforeLaunch && c.WindowContext is not null)
        {
            var shouldRun = false;
            await c.WindowContext.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var vm = new InjectAresConfigViewModel(c.Settings, null!, Log.Logger);
                    var win = new InjectAresConfigWindow(vm)
                    {
                        Owner = c.WindowContext.PlatformWindow as System.Windows.Window
                    };
                    win.Initialize(exe);
                    win.ShowDialog();
                    shouldRun = win.ShouldRun;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Emulator config dialog failed");
                    shouldRun = true;
                }
            });
            return shouldRun;
        }

        AresConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return true;
    }
}

public class AzaharConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Azahar", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        AzaharConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}

public class BlastemConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("BlastEm", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        BlastemConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}

public class DaphneConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Daphne", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        DaphneConfigurationService.BuildArguments(c.Settings);
        return Task.FromResult(true);
    }
}

public class MednafenConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Mednafen", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        MednafenConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}

public class MesenConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Mesen", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        MesenConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}

public class RaineConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Raine", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        RaineConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}

public class RedreamConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Redream", StringComparison.OrdinalIgnoreCase) || n.Contains("ReDream", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        RedreamConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}

public class SegaModel2ConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Model 2", StringComparison.OrdinalIgnoreCase) || n.Contains("Model2", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        SegaModel2ConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}

public class StellaConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Stella", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        StellaConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}

public class SupermodelConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Supermodel", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        SupermodelConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}

public class YumirConfigHandler : IEmulatorConfigHandler
{
    public bool IsMatch(string n, string p)
    {
        return n.Contains("Yumir", StringComparison.OrdinalIgnoreCase);
    }

    public Task<bool> HandleConfigurationAsync(LaunchContext c)
    {
        if (c.EmulatorManager is null || c.Settings is null) return Task.FromResult(false);

        var exe = PathHelper.ResolveRelativeToAppDirectory(c.EmulatorManager.EmulatorLocation);
        if (!File.Exists(exe)) return Task.FromResult(false);

        YumirConfigurationService.InjectSettings(exe, c.Settings, Log.Logger);
        return Task.FromResult(true);
    }
}
