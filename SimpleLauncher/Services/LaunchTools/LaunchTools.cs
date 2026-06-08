using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.LaunchTools;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.LaunchTools;

public class LaunchTools : ILaunchTools
{
    private readonly ILogErrors _logErrors;
    private readonly IConfiguration _configuration;
    private readonly IMessageBoxLibraryService _messageBoxLibrary;
    private readonly IResourceProvider _resourceProvider;

    public LaunchTools(ILogErrors logErrors, IConfiguration configuration, IMessageBoxLibraryService messageBoxLibrary, IResourceProvider resourceProvider)
    {
        _logErrors = logErrors;
        _configuration = configuration;
        _messageBoxLibrary = messageBoxLibrary;
        _resourceProvider = resourceProvider;
    }

    /// <summary>
    /// Launches an external executable with optional arguments and working directory.
    /// Handles basic file existence checks and generic launch exceptions.
    /// </summary>
    private async Task LaunchExternalTool(string toolPath, string arguments = null, string workingDirectory = null)
    {
        if (string.IsNullOrEmpty(toolPath))
        {
            _logErrors.LogAndForget(null, "Tool path cannot be null or empty.");
            await _messageBoxLibrary.SelectedToolNotFoundMessageBox();
            return;
        }

        if (!File.Exists(toolPath))
        {
            _logErrors.LogAndForget(null, $"External tool not found: {toolPath}");
            await _messageBoxLibrary.SelectedToolNotFoundMessageBox();
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
            await _messageBoxLibrary.ToolLaunchWasCanceledByUserMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer of genuine errors
            var contextMessage = $"An error occurred while launching external tool: {toolPath}.\n" +
                                 $"Arguments: {arguments ?? "None"}\n" +
                                 $"Working Directory: {workingDirectory ?? "Default"}";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await _messageBoxLibrary.ErrorLaunchingToolMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
        }
    }

    private async Task<string> GetToolExecutablePath(string toolFolder, string baseName, bool useArchSubfolders = false)
    {
        var architecture = RuntimeInformation.ProcessArchitecture;
        var archPath = architecture switch
        {
            Architecture.X64 => useArchSubfolders ? Path.Combine("x64", $"{baseName}.exe") : $"{baseName}.exe",
            Architecture.Arm64 => useArchSubfolders ? Path.Combine("arm64", $"{baseName}.exe") : $"{baseName}_arm64.exe",
            _ => null
        };

        if (archPath == null)
        {
            var msg = _resourceProvider.GetString("AppNotAvailableForArch", "This application is not available for {0}");
            await _messageBoxLibrary.LaunchToolInformationMessageBox(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return null;
        }

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", toolFolder, archPath);
    }

    public async Task CreateBatchFilesForXbox360XblaGames()
    {
        var toolPath = await GetToolExecutablePath("CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames");
        if (toolPath == null) return;

        await LaunchExternalTool(toolPath);
    }

    public async Task CreateBatchFilesForWindowsGames()
    {
        var toolPath = await GetToolExecutablePath("CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames");
        if (toolPath == null) return;

        await LaunchExternalTool(toolPath);
    }

    public async Task FindRomCoverLaunch(string selectedImageFolder, string selectedRomFolder)
    {
        var toolPath = await GetToolExecutablePath("FindRomCover", "FindRomCover");
        if (toolPath == null) return;

        var arguments = string.Empty;
        var workingDirectory = Path.GetDirectoryName(toolPath);

        var absoluteImageFolder = !string.IsNullOrEmpty(selectedImageFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder) : null;
        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
        }

        await LaunchExternalTool(toolPath, arguments, workingDirectory);
    }

    public async Task CreateBatchFilesForPs3Games()
    {
        var toolPath = await GetToolExecutablePath("CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games");
        if (toolPath == null) return;

        await LaunchExternalTool(toolPath);
    }

    public async Task BatchConvertIsoToXiso()
    {
        var toolPath = await GetToolExecutablePath("BatchConvertIsoToXiso", "BatchConvertIsoToXiso");
        if (toolPath == null) return;

        await LaunchExternalTool(toolPath);
    }

    public async Task BatchConvertToChd(string selectedRomFolder)
    {
        var toolPath = await GetToolExecutablePath("BatchConvertToCHD", "BatchConvertToCHD");
        if (toolPath == null) return;

        var arguments = string.Empty;
        var workingDirectory = Path.GetDirectoryName(toolPath);

        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteRomFolder}\"";
        }

        await LaunchExternalTool(toolPath, arguments, workingDirectory);
    }

    public async Task BatchConvertToCompressedFile()
    {
        var toolPath = await GetToolExecutablePath("BatchConvertToCompressedFile", "BatchConvertToCompressedFile");
        if (toolPath == null) return;

        await LaunchExternalTool(toolPath);
    }

    public async Task BatchConvertToRvz()
    {
        var toolPath = await GetToolExecutablePath("BatchConvertToRVZ", "BatchConvertToRVZ");
        if (toolPath == null) return;

        await LaunchExternalTool(toolPath);
    }

    public async Task CreateBatchFilesForScummVmGames()
    {
        var toolPath = await GetToolExecutablePath("CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames");
        if (toolPath == null) return;

        await LaunchExternalTool(toolPath);
    }

    public async Task RomValidator()
    {
        var toolPath = await GetToolExecutablePath("RomValidator", "RomValidator");
        if (toolPath == null) return;

        await LaunchExternalTool(toolPath);
    }

    public async Task GameCoverScraper(string selectedImageFolder, string selectedRomFolder)
    {
        var toolPath = await GetToolExecutablePath("GameCoverScraper", "GameCoverScraper", true);
        if (toolPath == null) return;

        var arguments = string.Empty;
        var workingDirectory = Path.GetDirectoryName(toolPath);

        var absoluteImageFolder = !string.IsNullOrEmpty(selectedImageFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder) : null;
        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
        }

        await LaunchExternalTool(toolPath, arguments, workingDirectory);
    }

    public async Task RetroGameCoverDownloader(string selectedImageFolder, string selectedRomFolder)
    {
        var toolPath = await GetToolExecutablePath("RetroGameCoverDownloader", "RetroGameCoverDownloader");
        if (toolPath == null) return;

        var arguments = string.Empty;
        var workingDirectory = Path.GetDirectoryName(toolPath);

        var absoluteImageFolder = !string.IsNullOrEmpty(selectedImageFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder) : null;
        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteRomFolder}\" \"{absoluteImageFolder}\"";
        }

        await LaunchExternalTool(toolPath, arguments, workingDirectory);
    }
}
