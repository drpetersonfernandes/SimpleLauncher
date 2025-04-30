using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public static class LaunchTools
{
    internal static void CreateBatchFilesForXbox360XBLAGames_Click(string logPath)
    {
        try
        {
            var createBatchFilesForXbox360XblaGamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForXbox360XBLAGames", "CreateBatchFilesForXbox360XBLAGames.exe");

            if (File.Exists(createBatchFilesForXbox360XblaGamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForXbox360XblaGamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'CreateBatchFilesForXbox360XBLAGames.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'CreateBatchFilesForXbox360XBLAGames.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }

    internal static void CreateBatchFilesForWindowsGames_Click(string logPath)
    {
        try
        {
            var createBatchFilesForWindowsGamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForWindowsGames", "CreateBatchFilesForWindowsGames.exe");

            if (File.Exists(createBatchFilesForWindowsGamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForWindowsGamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'CreateBatchFilesForWindowsGames.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'CreateBatchFilesForWindowsGames.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }

    internal static void FindRomCoverLaunch_Click(string selectedImageFolder, string selectedRomFolder, string logPath)
    {
        try
        {
            var findRomCoverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "FindRomCover", "FindRomCover.exe");

            if (File.Exists(findRomCoverPath))
            {
                string absoluteImageFolder = null;
                string absoluteRomFolder = null;

                // Check if _selectedImageFolder and _selectedRomFolder are set
                if (!string.IsNullOrEmpty(selectedImageFolder))
                {
                    absoluteImageFolder = PathHelper.ResolveRelativeToAppDirectory(selectedImageFolder);
                }

                if (!string.IsNullOrEmpty(selectedRomFolder))
                {
                    absoluteRomFolder = PathHelper.ResolveRelativeToAppDirectory(selectedRomFolder);
                }

                // Determine arguments based on available folders
                var arguments = string.Empty;
                if (absoluteImageFolder != null && absoluteRomFolder != null)
                {
                    arguments = $"\"{absoluteImageFolder}\" \"{absoluteRomFolder}\"";
                }

                // Start the process with or without arguments
                Process.Start(new ProcessStartInfo
                {
                    FileName = findRomCoverPath,
                    Arguments = arguments,
                    UseShellExecute = true,
                    WorkingDirectory = findRomCoverPath
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "The file 'FindRomCover.exe' is missing.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.FindRomCoverMissingMessageBox();
            }
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // Notify developer
            const string contextMessage = "The operation was canceled by the user while trying to launch 'FindRomCover.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FindRomCoverLaunchWasCanceledByUserMessageBox();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'FindRomCover.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.FindRomCoverLaunchWasBlockedMessageBox(logPath);
        }
    }

    internal static void CreateBatchFilesForPS3Games_Click(string logPath)
    {
        try
        {
            var createBatchFilesForPs3GamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForPS3Games", "CreateBatchFilesForPS3Games.exe");

            if (File.Exists(createBatchFilesForPs3GamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForPs3GamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'CreateBatchFilesForPS3Games.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'CreateBatchFilesForPS3Games.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }

    internal static void BatchConvertIsoToXiso_Click(string logPath)
    {
        try
        {
            var createBatchConvertIsoToXisoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertIsoToXiso", "BatchConvertIsoToXiso.exe");

            if (File.Exists(createBatchConvertIsoToXisoPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchConvertIsoToXisoPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'BatchConvertIsoToXiso.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'BatchConvertIsoToXiso.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }

    internal static void BatchConvertToCHD_Click(string logPath)
    {
        try
        {
            var createBatchConvertToChdPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCHD", "BatchConvertToCHD.exe");

            if (File.Exists(createBatchConvertToChdPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchConvertToChdPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'BatchConvertToCHD.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'BatchConvertToCHD.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }

    internal static void BatchConvertToCompressedFile_Click(string logPath)
    {
        try
        {
            var batchConvertToCompressedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToCompressedFile", "BatchConvertToCompressedFile.exe");

            if (File.Exists(batchConvertToCompressedFilePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = batchConvertToCompressedFilePath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'BatchConvertToCompressedFile.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'BatchConvertToCompressedFile.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }

    internal static void BatchVerifyCHDFiles_Click(string logPath)
    {
        try
        {
            var batchVerifyChdFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchVerifyCHDFiles", "BatchVerifyCHDFiles.exe");

            if (File.Exists(batchVerifyChdFilesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = batchVerifyChdFilesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'BatchVerifyCHDFiles.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'BatchVerifyCHDFiles.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }

    internal static void BatchVerifyCompressedFiles_Click(string logPath)
    {
        try
        {
            var batchVerifyCompressedFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchVerifyCompressedFiles", "BatchVerifyCompressedFiles.exe");

            if (File.Exists(batchVerifyCompressedFilesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = batchVerifyCompressedFilesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'BatchVerifyCompressedFiles.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'BatchVerifyCompressedFiles.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }

    internal static void CreateBatchFilesForScummVMGames_Click(string logPath)
    {
        try
        {
            var createBatchFilesForScummVmGamesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForScummVMGames", "CreateBatchFilesForScummVMGames.exe");

            if (File.Exists(createBatchFilesForScummVmGamesPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForScummVmGamesPath,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'CreateBatchFilesForScummVMGames.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'CreateBatchFilesForScummVMGames.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }

    internal static void CreateBatchFilesForSegaModel3Games_Click(string logPath)
    {
        try
        {
            var createBatchFilesForSegaModel3Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "CreateBatchFilesForSegaModel3Games", "CreateBatchFilesForSegaModel3Games.exe");

            if (File.Exists(createBatchFilesForSegaModel3Path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = createBatchFilesForSegaModel3Path,
                    UseShellExecute = true
                });
            }
            else
            {
                // Notify developer
                const string contextMessage = "'CreateBatchFilesForSegaModel3Games.exe' was not found.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SelectedToolNotFoundMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while launching 'CreateBatchFilesForSegaModel3Games.exe'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorLaunchingToolMessageBox(logPath);
        }
    }
}