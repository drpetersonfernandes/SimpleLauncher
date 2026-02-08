using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using Application = System.Windows.Application;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.LaunchTools;

public class LaunchTools : ILaunchTools
{
    private readonly ILogErrors _logErrors;
    private readonly IConfiguration _configuration;

    public LaunchTools(ILogErrors logErrors, IConfiguration configuration)
    {
        _logErrors = logErrors;
        _configuration = configuration;
    }

    /// <summary>
    /// Launches an external executable with optional arguments and working directory.
    /// Handles basic file existence checks and generic launch exceptions.
    /// </summary>
    private void LaunchExternalTool(string toolPath, string arguments = null, string workingDirectory = null)
    {
        if (string.IsNullOrEmpty(toolPath))
        {
            _ = _logErrors.LogErrorAsync(null, "Tool path cannot be null or empty.");
            MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            return;
        }

        if (!File.Exists(toolPath))
        {
            _ = _logErrors.LogErrorAsync(null, $"External tool not found: {toolPath}");
            MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = true
            };

            // Set working directory if provided, otherwise it defaults to the executable's directory
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                var resolvedWorkingDirectory = PathHelper.ResolveRelativeToAppDirectory(workingDirectory);
                if (Directory.Exists(resolvedWorkingDirectory))
                {
                    psi.WorkingDirectory = resolvedWorkingDirectory;
                }
            }

            Process.Start(psi);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223 || ex.NativeErrorCode == 5 || (uint)ex.HResult == 0x800704C7)
        {
            // 1223 = User cancelled UAC.
            // 5 = Access Denied (sometimes returned if UAC is disabled but user lacks rights).
            // 0x800704C7 = HRESULT for Operation Cancelled.
            // We do NOT log these to the developer API as they are expected user actions.
            MessageBoxLibrary.ToolLaunchWasCanceledByUserMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer of genuine errors
            var contextMessage = $"An error occurred while launching external tool: {toolPath}.\n" +
                                 $"Arguments: {arguments ?? "None"}\n" +
                                 $"Working Directory: {workingDirectory ?? "Default"}";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
        }
    }

    public void CreateBatchFilesForXbox360XblaGames()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "CreateBatchFilesForXbox360XBLAGames.exe",
            Architecture.Arm64 => "CreateBatchFilesForXbox360XBLAGames_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", executableName);
        LaunchExternalTool(toolPath);
    }

    public void CreateBatchFilesForWindowsGames()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "CreateBatchFilesForWindowsGames.exe",
            Architecture.Arm64 => "CreateBatchFilesForWindowsGames_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForWindowsGames", executableName);
        LaunchExternalTool(toolPath);
    }

    public void FindRomCoverLaunch(string selectedImageFolder, string selectedRomFolder)
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "FindRomCover.exe",
            Architecture.Arm64 => "FindRomCover_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "FindRomCover", executableName);
        var arguments = string.Empty;
        var workingDirectory = Path.GetDirectoryName(toolPath);

        var absoluteImageFolder = !string.IsNullOrEmpty(selectedImageFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder) : null;
        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
        }

        LaunchExternalTool(toolPath, arguments, workingDirectory);
    }

    public void CreateBatchFilesForPs3Games()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "CreateBatchFilesForPS3Games.exe",
            Architecture.Arm64 => "CreateBatchFilesForPS3Games_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForPS3Games", executableName);
        LaunchExternalTool(toolPath);
    }

    public void BatchConvertIsoToXiso()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "BatchConvertIsoToXiso.exe",
            Architecture.Arm64 => "BatchConvertIsoToXiso_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertIsoToXiso", executableName);
        LaunchExternalTool(toolPath);
    }

    public void BatchConvertToChd()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "BatchConvertToCHD.exe",
            Architecture.Arm64 => "BatchConvertToCHD_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", executableName);
        LaunchExternalTool(toolPath);
    }

    public void BatchConvertToCompressedFile()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "BatchConvertToCompressedFile.exe",
            Architecture.Arm64 => "BatchConvertToCompressedFile_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCompressedFile", executableName);
        LaunchExternalTool(toolPath);
    }

    public void BatchConvertToRvz()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "BatchConvertToRVZ.exe",
            Architecture.Arm64 => "BatchConvertToRVZ_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToRVZ", executableName);
        LaunchExternalTool(toolPath);
    }

    public void CreateBatchFilesForScummVmGames()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "CreateBatchFilesForScummVMGames.exe",
            Architecture.Arm64 => "CreateBatchFilesForScummVMGames_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForScummVMGames", executableName);
        LaunchExternalTool(toolPath);
    }

    public void CreateBatchFilesForSegaModel3Games()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "CreateBatchFilesForSegaModel3Games.exe",
            Architecture.Arm64 => "CreateBatchFilesForSegaModel3Games_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForSegaModel3Games", executableName);
        LaunchExternalTool(toolPath);
    }

    public void RomValidator()
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executableName = architecture switch
        {
            Architecture.X64 => "RomValidator.exe",
            Architecture.Arm64 => "RomValidator_arm64.exe",
            _ => null
        };

        if (executableName == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "RomValidator", executableName);
        LaunchExternalTool(toolPath);
    }

    public void GameCoverScraper(string selectedImageFolder, string selectedRomFolder)
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executablePath = architecture switch
        {
            Architecture.X64 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "GameCoverScraper", "x64", "GameCoverScraper.exe"),
            Architecture.Arm64 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "GameCoverScraper", "arm64", "GameCoverScraper.exe"),
            _ => null
        };

        if (executablePath == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var arguments = string.Empty;
        var workingDirectory = Path.GetDirectoryName(executablePath);

        var absoluteImageFolder = !string.IsNullOrEmpty(selectedImageFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder) : null;
        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
        }

        LaunchExternalTool(executablePath, arguments, workingDirectory);
    }

    public void RetroGameCoverDownloader(string selectedImageFolder, string selectedRomFolder)
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var executablePath = architecture switch
        {
            Architecture.X64 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "RetroGameCoverDownloader", "RetroGameCoverDownloader.exe"),
            Architecture.Arm64 => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "RetroGameCoverDownloader", "RetroGameCoverDownloader_arm64.exe"),
            _ => null
        };

        if (executablePath == null)
        {
            var msg = (string)Application.Current.TryFindResource("AppNotAvailableForArch") ?? "This application is not available for {0}";
            MessageBoxLibrary.LaunchToolInformation(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return;
        }

        var arguments = string.Empty;
        var workingDirectory = Path.GetDirectoryName(executablePath);

        var absoluteImageFolder = !string.IsNullOrEmpty(selectedImageFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder) : null;
        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
        }

        LaunchExternalTool(executablePath, arguments, workingDirectory);
    }
}