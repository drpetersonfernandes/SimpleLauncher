using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace SimpleLauncher.Services;

public static class LaunchTools
{
    // Define the LogPath
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
            var ex = new ArgumentNullException(nameof(toolPath), contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user (using a generic message for tool not found)
            MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            return;
        }

        // Check if the tool executable exists
        if (!File.Exists(toolPath))
        {
            // Notify developer
            var contextMessage = $"External tool not found: {toolPath}";
            var ex = new FileNotFoundException(contextMessage, toolPath);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            return;
        }

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = arguments ?? string.Empty, // Use empty string if no arguments
                UseShellExecute = true // Use shell execute for launching external tools
            };

            // Set working directory if provided, otherwise it defaults to the executable's directory
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                // Resolve the working directory relative to the app directory for robustness
                var resolvedWorkingDirectory = PathHelper.ResolveRelativeToAppDirectory(workingDirectory);

                if (Directory.Exists(resolvedWorkingDirectory))
                {
                    psi.WorkingDirectory = resolvedWorkingDirectory;
                }
                else
                {
                    // Log a warning if the specified working directory doesn't exist,
                    // but still attempt to launch using the default working directory.
                    var warningMessage = $"Specified working directory not found: {workingDirectory}. Launching with default working directory.";
                    _ = LogErrors.LogErrorAsync(new DirectoryNotFoundException(warningMessage), warningMessage);
                    // Do not set psi.WorkingDirectory, let it default.
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
        string workingDirectory = null; // Let it default to tool's directory initially

        // Determine arguments based on available folders, resolving them to absolute paths
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

        if (absoluteImageFolder != null && absoluteRomFolder != null)
        {
            // Quote paths to handle spaces
            arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
            // Set the working directory to the tool's directory explicitly if arguments are passed
            workingDirectory = Path.GetDirectoryName(toolPath);
        }

        try
        {
            // Use the generic launcher
            LaunchExternalTool(toolPath, arguments, workingDirectory);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // This specific exception handling for user cancellation remains here
            // because it's a specific behavior of the FindRomCover tool launch.
            // Notify the developer
            const string contextMessage = "The operation was canceled by the user while trying to launch 'FindRomCover.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FindRomCoverLaunchWasCanceledByUserMessageBox();
        }
        // Generic Exception handling is now inside LaunchExternalTool
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
}