using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

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
    /// Handles basic file existence checks, PE validation, and generic launch exceptions.
    /// </summary>
    private async Task LaunchExternalToolAsync(string toolPath, string arguments = null, string workingDirectory = null)
    {
        if (string.IsNullOrEmpty(toolPath))
        {
            _logErrors.LogAndForget(null, "Tool path cannot be null or empty.");
            await _messageBoxLibrary.SelectedToolNotFoundMessageBoxAsync();
            return;
        }

        if (!File.Exists(toolPath))
        {
            _logErrors.LogAndForget(null, $"External tool not found: {toolPath}");
            await _messageBoxLibrary.SelectedToolNotFoundMessageBoxAsync();
            return;
        }

        if (!IsValidPeFile(toolPath))
        {
            _logErrors.LogAndForget(null, $"External tool is not a valid PE executable: {toolPath}");
            await _messageBoxLibrary.SelectedToolNotFoundMessageBoxAsync();
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = arguments ?? "",
                UseShellExecute = true
            };

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
            await _messageBoxLibrary.ToolLaunchWasCanceledByUserMessageBoxAsync();
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 216)
        {
            // 216 = ERROR_EXE_MACHINE_TYPE_MISMATCH
            // "The specified executable is not a valid application for this OS platform."
            _logErrors.LogAndForget(ex, $"Tool executable architecture mismatch: {toolPath}.\n" +
                                        $"NativeErrorCode: {ex.NativeErrorCode}, HResult: 0x{ex.HResult:X8}");
            await _messageBoxLibrary.SelectedToolNotFoundMessageBoxAsync();
        }
        catch (Exception ex)
        {
            var contextMessage = $"An error occurred while launching external tool: {toolPath}.\n" +
                                 $"Arguments: {arguments ?? "None"}\n" +
                                 $"Working Directory: {workingDirectory ?? "Default"}\n" +
                                 $"NativeErrorCode: {(ex is Win32Exception w32 ? w32.NativeErrorCode : -1)}, HResult: 0x{ex.HResult:X8}";
            _logErrors.LogAndForget(ex, contextMessage);

            await _messageBoxLibrary.ErrorLaunchingToolMessageBoxAsync(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
        }
    }

    /// <summary>
    /// Validates that a file is a valid PE executable by checking the MZ and PE signatures.
    /// </summary>
    private static bool IsValidPeFile(string filePath)
    {
        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (fs.Length < 64) return false;

            Span<byte> buffer = stackalloc byte[4];
            fs.ReadExactly(buffer[..2]);
            if (buffer[0] != 0x4D || buffer[1] != 0x5A) return false; // MZ header

            fs.Seek(0x3C, SeekOrigin.Begin);
            fs.ReadExactly(buffer);
            var peOffset = BitConverter.ToInt32(buffer);

            if (peOffset < 0 || peOffset + 4 > fs.Length) return false;

            fs.Seek(peOffset, SeekOrigin.Begin);
            fs.ReadExactly(buffer);
            return buffer[0] == 0x50 && buffer[1] == 0x45 && buffer[2] == 0x00 && buffer[3] == 0x00; // PE\0\0
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GetToolExecutablePathAsync(string toolFolder, string baseName, bool useArchSubfolders = false)
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
            await _messageBoxLibrary.LaunchToolInformationMessageBoxAsync(string.Format(CultureInfo.InvariantCulture, msg, architecture));
            return null;
        }

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", toolFolder, archPath);
    }

    public async Task CreateBatchFilesForXbox360XblaGamesAsync()
    {
        var toolPath = await GetToolExecutablePathAsync("CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames");
        if (toolPath == null) return;

        await LaunchExternalToolAsync(toolPath);
    }

    public async Task CreateBatchFilesForWindowsGamesAsync()
    {
        var toolPath = await GetToolExecutablePathAsync("CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames");
        if (toolPath == null) return;

        await LaunchExternalToolAsync(toolPath);
    }

    public async Task FindRomCoverLaunchAsync(string selectedImageFolder, string selectedRomFolder)
    {
        var toolPath = await GetToolExecutablePathAsync("FindRomCover", "FindRomCover");
        if (toolPath == null) return;

        var arguments = "";
        var workingDirectory = Path.GetDirectoryName(toolPath);

        var absoluteImageFolder = !string.IsNullOrEmpty(selectedImageFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder) : null;
        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
        }

        await LaunchExternalToolAsync(toolPath, arguments, workingDirectory);
    }

    public async Task CreateBatchFilesForPs3GamesAsync()
    {
        var toolPath = await GetToolExecutablePathAsync("CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games");
        if (toolPath == null) return;

        await LaunchExternalToolAsync(toolPath);
    }

    public async Task BatchConvertIsoToXisoAsync()
    {
        var toolPath = await GetToolExecutablePathAsync("BatchConvertIsoToXiso", "BatchConvertIsoToXiso");
        if (toolPath == null) return;

        await LaunchExternalToolAsync(toolPath);
    }

    public async Task BatchConvertToChdAsync(string selectedRomFolder)
    {
        var toolPath = await GetToolExecutablePathAsync("BatchConvertToCHD", "BatchConvertToCHD");
        if (toolPath == null) return;

        var arguments = "";
        var workingDirectory = Path.GetDirectoryName(toolPath);

        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteRomFolder}\"";
        }

        await LaunchExternalToolAsync(toolPath, arguments, workingDirectory);
    }

    public async Task BatchConvertToCompressedFileAsync()
    {
        var toolPath = await GetToolExecutablePathAsync("BatchConvertToCompressedFile", "BatchConvertToCompressedFile");
        if (toolPath == null) return;

        await LaunchExternalToolAsync(toolPath);
    }

    public async Task BatchConvertToRvzAsync()
    {
        var toolPath = await GetToolExecutablePathAsync("BatchConvertToRVZ", "BatchConvertToRVZ");
        if (toolPath == null) return;

        await LaunchExternalToolAsync(toolPath);
    }

    public async Task CreateBatchFilesForScummVmGamesAsync()
    {
        var toolPath = await GetToolExecutablePathAsync("CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames");
        if (toolPath == null) return;

        await LaunchExternalToolAsync(toolPath);
    }

    public async Task RomValidatorAsync()
    {
        var toolPath = await GetToolExecutablePathAsync("RomValidator", "RomValidator");
        if (toolPath == null) return;

        await LaunchExternalToolAsync(toolPath);
    }

    public async Task GameCoverScraperAsync(string selectedImageFolder, string selectedRomFolder)
    {
        var toolPath = await GetToolExecutablePathAsync("GameCoverScraper", "GameCoverScraper", true);
        if (toolPath == null) return;

        var arguments = "";
        var workingDirectory = Path.GetDirectoryName(toolPath);

        var absoluteImageFolder = !string.IsNullOrEmpty(selectedImageFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder) : null;
        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
        }

        await LaunchExternalToolAsync(toolPath, arguments, workingDirectory);
    }

    public async Task RetroGameCoverDownloaderAsync(string selectedImageFolder, string selectedRomFolder)
    {
        var toolPath = await GetToolExecutablePathAsync("RetroGameCoverDownloader", "RetroGameCoverDownloader");
        if (toolPath == null) return;

        var arguments = "";
        var workingDirectory = Path.GetDirectoryName(toolPath);

        var absoluteImageFolder = !string.IsNullOrEmpty(selectedImageFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder) : null;
        var absoluteRomFolder = !string.IsNullOrEmpty(selectedRomFolder) ? PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder) : null;

        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteRomFolder}\" \"{absoluteImageFolder}\"";
        }

        await LaunchExternalToolAsync(toolPath, arguments, workingDirectory);
    }
}
