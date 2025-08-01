using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace SimpleLauncher.Services;

public static class LaunchTools
{
    private static readonly string LogPath = GetLogPath.Path();

    /// <summary>
    /// Launches an external executable with optional arguments and working directory.
    /// Handles basic file existence checks and generic launch exceptions.
    /// </summary>
    /// <param name="toolPath">The full path to the executable file.</param>
    /// <param name="arguments">Optional command-line arguments for the executable.</param>
    /// <param name="workingDirectory">Optional working directory for the process. If null or empty, the executable's directory is used.</param>
    private static void LaunchExternalTool(string toolPath, string arguments = null, string workingDirectory = null)
    {
        if (string.IsNullOrEmpty(toolPath))
        {
            // Notify developer
            const string contextMessage = "Tool path cannot be null or empty.";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user (using a generic message for tool not found)
            MessageBoxLibrary.SelectedToolNotFoundMessageBox();

            return;
        }

        // Check if the tool executable exists
        if (!File.Exists(toolPath))
        {
            // Notify developer
            var contextMessage = $"External tool not found: {toolPath}";
            _ = LogErrors.LogErrorAsync(null, contextMessage);

            // Notify user
            MessageBoxLibrary.SelectedToolNotFoundMessageBox();

            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = arguments ?? string.Empty,
                UseShellExecute = true // Use shell execute for launching external tools
            };

            // Set working directory if provided, otherwise it defaults to the executable's directory
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                var resolvedWorkingDirectory = PathHelper.ResolveRelativeToAppDirectory(workingDirectory);

                if (Directory.Exists(resolvedWorkingDirectory))
                {
                    psi.WorkingDirectory = resolvedWorkingDirectory;
                }
                else
                {
                    // Notify developer
                    var warningMessage = $"Specified working directory not found: {workingDirectory}. Launching with default working directory.";
                    _ = LogErrors.LogErrorAsync(new DirectoryNotFoundException(warningMessage), warningMessage);
                }
            }

            Process.Start(psi);
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"An error occurred while launching external tool: {toolPath}.\n" +
                                 $"Arguments: {arguments ?? "None"}\n" +
                                 $"Working Directory: {workingDirectory ?? "Default"}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(LogPath);
        }
    }

    internal static void CreateBatchFilesForXbox360XBLAGames_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void CreateBatchFilesForWindowsGames_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void FindRomCoverLaunch_Click(string selectedImageFolder, string selectedRomFolder)
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "FindRomCover", "FindRomCover.exe");

        var arguments = string.Empty;
        string workingDirectory = null;

        // Resolve the selected image and rom folders
        string absoluteImageFolder = null;
        if (!string.IsNullOrEmpty(selectedImageFolder))
        {
            absoluteImageFolder = PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder);
        }

        string absoluteRomFolder = null;
        if (!string.IsNullOrEmpty(selectedRomFolder))
        {
            absoluteRomFolder = PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder);
        }

        // Check if both resolved paths are valid
        if (!string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(absoluteRomFolder))
        {
            arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
            workingDirectory = Path.GetDirectoryName(toolPath); // Keep working directory as tool's directory
        }
        else
        {
            if (string.IsNullOrEmpty(absoluteImageFolder) && !string.IsNullOrEmpty(selectedImageFolder))
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(null, $"FindRomCover: Could not resolve image folder path: '{selectedImageFolder}'");
            }

            if (string.IsNullOrEmpty(absoluteRomFolder) && !string.IsNullOrEmpty(selectedRomFolder))
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(null, $"FindRomCover: Could not resolve ROM folder path: '{selectedRomFolder}'");
            }
        }

        try
        {
            LaunchExternalTool(toolPath, arguments, workingDirectory);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // Notify developer
            const string contextMessage = "The operation was canceled by the user while trying to launch 'FindRomCover.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FindRomCoverLaunchWasCanceledByUserMessageBox();
        }
    }

    internal static void CreateBatchFilesForPS3Games_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void BatchConvertIsoToXiso_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertIsoToXiso", "BatchConvertIsoToXiso.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void BatchConvertToCHD_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", "BatchConvertToCHD.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void BatchConvertToCompressedFile_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCompressedFile", "BatchConvertToCompressedFile.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void BatchConvertToRVZ_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToRVZ", "BatchConvertToRVZ.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void BatchVerifyCHDFiles_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchVerifyCHDFiles", "BatchVerifyCHDFiles.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void BatchVerifyCompressedFiles_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchVerifyCompressedFiles", "BatchVerifyCompressedFiles.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void CreateBatchFilesForScummVMGames_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames.exe");
        LaunchExternalTool(toolPath);
    }

    internal static void CreateBatchFilesForSegaModel3Games_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games.exe");
        LaunchExternalTool(toolPath);
    }

    public static void RomValidator_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "RomValidator", "RomValidator.exe");
        LaunchExternalTool(toolPath);
    }
}