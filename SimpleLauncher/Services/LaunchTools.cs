using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

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
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "CreateBatchFilesForXbox360XBLAGames.exe";
                    break;
                case Architecture.X86:
                    executableName = "CreateBatchFilesForXbox360XBLAGames_x86.exe";
                    break;
                case Architecture.Arm64:
                    executableName = "CreateBatchFilesForXbox360XBLAGames_arm64.exe";
                    break;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", executableName);
            LaunchExternalTool(toolPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching CreateBatchFilesForXbox360XBLAGames");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("CreateBatchFilesForXbox360XBLAGames", LogPath);
        }
    }

    internal static void CreateBatchFilesForWindowsGames_Click()
    {
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "CreateBatchFilesForWindowsGames.exe";
                    break;
                case Architecture.X86:
                    executableName = "CreateBatchFilesForWindowsGames_x86.exe";
                    break;
                case Architecture.Arm64:
                    executableName = "CreateBatchFilesForWindowsGames_arm64.exe";
                    break;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForWindowsGames", executableName);
            LaunchExternalTool(toolPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching CreateBatchFilesForWindowsGames");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("CreateBatchFilesForWindowsGames", LogPath);
        }
    }

    internal static void FindRomCoverLaunch_Click(string selectedImageFolder, string selectedRomFolder)
    {
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "FindRomCover.exe";
                    break;
                case Architecture.X86:
                    executableName = "FindRomCover_x86.exe";
                    break;
                case Architecture.Arm64:
                    executableName = "FindRomCover_arm64.exe";
                    break;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "FindRomCover", executableName);
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
                const string contextMessage = "The operation was canceled by the user while trying to launch FindRomCover.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.FindRomCoverLaunchWasCanceledByUserMessageBox();
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error launching FindRomCover");

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("FindRomCover", LogPath);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching FindRomCover");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("FindRomCover", LogPath);
        }
    }

    internal static void CreateBatchFilesForPS3Games_Click()
    {
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "CreateBatchFilesForPS3Games.exe";
                    break;
                case Architecture.X86:
                    executableName = "CreateBatchFilesForPS3Games_x86.exe";
                    break;
                case Architecture.Arm64:
                    executableName = "CreateBatchFilesForPS3Games_arm64.exe";
                    break;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForPS3Games", executableName);
            LaunchExternalTool(toolPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching CreateBatchFilesForPS3Games");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("CreateBatchFilesForPS3Games", LogPath);
        }
    }

    internal static void BatchConvertIsoToXiso_Click()
    {
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "BatchConvertIsoToXiso.exe";
                    break;
                case Architecture.X86:
                    executableName = "BatchConvertIsoToXiso_x86.exe";
                    break;
                case Architecture.Arm64:
                    MessageBoxLibrary.LaunchToolInformation("This application is not available for win-arm64");
                    return;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertIsoToXiso", executableName);
            LaunchExternalTool(toolPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching BatchConvertIsoToXiso");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("BatchConvertIsoToXiso", LogPath);
        }
    }

    internal static void BatchConvertToCHD_Click()
    {
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "BatchConvertToCHD.exe";
                    break;
                case Architecture.X86:
                    executableName = "BatchConvertToCHD_x86.exe";
                    break;
                case Architecture.Arm64:
                    MessageBoxLibrary.LaunchToolInformation("This application is not available for win-arm64");
                    return;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", executableName);
            LaunchExternalTool(toolPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching BatchConvertToCHD");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("BatchConvertToCHD", LogPath);
        }
    }

    internal static void BatchConvertToCompressedFile_Click()
    {
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "BatchConvertToCompressedFile.exe";
                    break;
                case Architecture.X86:
                    executableName = "BatchConvertToCompressedFile_x86.exe";
                    break;
                case Architecture.Arm64:
                    executableName = "BatchConvertToCompressedFile_arm64.exe";
                    break;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCompressedFile", executableName);
            LaunchExternalTool(toolPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching BatchConvertToCHD");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("BatchConvertToCHD", LogPath);
        }
    }

    internal static void BatchConvertToRVZ_Click()
    {
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "BatchConvertToRVZ.exe";
                    break;
                case Architecture.Arm64:
                    executableName = "BatchConvertToRVZ_arm64.exe";
                    break;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToRVZ", executableName);
            LaunchExternalTool(toolPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching BatchConvertToRVZ");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("BatchConvertToRVZ", LogPath);
        }
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
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "CreateBatchFilesForScummVMGames.exe";
                    break;
                case Architecture.X86:
                    executableName = "CreateBatchFilesForScummVMGames_x86.exe";
                    break;
                case Architecture.Arm64:
                    executableName = "CreateBatchFilesForScummVMGames_arm64.exe";
                    break;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForScummVMGames", executableName);
            LaunchExternalTool(toolPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching CreateBatchFilesForScummVMGames");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("CreateBatchFilesForScummVMGames", LogPath);
        }
    }

    internal static void CreateBatchFilesForSegaModel3Games_Click()
    {
        try
        {
            var architecture = RuntimeInformation.ProcessArchitecture;
            string executableName;

            switch (architecture)
            {
                case Architecture.X64:
                    executableName = "CreateBatchFilesForSegaModel3Games.exe";
                    break;
                case Architecture.X86:
                    executableName = "CreateBatchFilesForSegaModel3Games_x86.exe";
                    break;
                case Architecture.Arm64:
                    executableName = "CreateBatchFilesForSegaModel3Games_arm64.exe";
                    break;
                default:
                    MessageBoxLibrary.LaunchToolInformation($"This application is not available for {architecture}");
                    return;
            }

            var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForSegaModel3Games", executableName);
            LaunchExternalTool(toolPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error launching CreateBatchFilesForSegaModel3Games");

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorLaunchingTheToolMessageBox("CreateBatchFilesForSegaModel3Games", LogPath);
        }
    }

    public static void RomValidator_Click()
    {
        var toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "RomValidator", "RomValidator.exe");
        LaunchExternalTool(toolPath);
    }
}