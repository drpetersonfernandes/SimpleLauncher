using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.MountFiles;

public static class FindEbootBin
{
    public static string FindEbootBinRecursive(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
        {
            return null;
        }

        const string targetFileName = "EBOOT.BIN";
        DebugLogger.Log($"[FindEbootBin.FindEbootBinRecursive] Searching for {targetFileName} in {directoryPath}");

        try
        {
            // Check top directory first
            var filesInTopDir = Directory.GetFiles(directoryPath, targetFileName, SearchOption.TopDirectoryOnly);
            if (filesInTopDir.Length > 0)
            {
                DebugLogger.Log(
                    $"[FindEbootBin.FindEbootBinRecursive] Found {targetFileName} in top directory: {filesInTopDir[0]}");
                return filesInTopDir[0];
            }

            // Check common PS3 structure: <mount>\PS3_GAME\USRDIR\EBOOT.BIN
            var ps3GameDirs = Directory.GetDirectories(directoryPath, "PS3_GAME", SearchOption.TopDirectoryOnly);
            foreach (var ps3GameDir in ps3GameDirs)
            {
                var usrDir = Path.Combine(ps3GameDir, "USRDIR");
                if (!Directory.Exists(usrDir)) continue;

                var filesInUsrDir = Directory.GetFiles(usrDir, targetFileName, SearchOption.TopDirectoryOnly);
                if (filesInUsrDir.Length <= 0) continue;

                DebugLogger.Log(
                    $"[FindEbootBin.FindEbootBinRecursive] Found {targetFileName} in PS3_GAME/USRDIR: {filesInUsrDir[0]}");
                return filesInUsrDir[0];
            }

            // Fallback to full recursive search if not found in common locations
            DebugLogger.Log(
                $"[FindEbootBin.FindEbootBinRecursive] {targetFileName} not found in typical locations. Starting full recursive search in {directoryPath}...");
            var filesRecursive = Directory.GetFiles(directoryPath, targetFileName, SearchOption.AllDirectories);
            if (filesRecursive.Length > 0)
            {
                DebugLogger.Log(
                    $"[FindEbootBin.FindEbootBinRecursive] Found {targetFileName} via full recursive search: {filesRecursive[0]}");
                return filesRecursive[0];
            }
        }
        catch (UnauthorizedAccessException uaEx)
        {
            DebugLogger.Log($"[FindEbootBin.FindEbootBinRecursive] UnauthorizedAccessException searching for {targetFileName} in {directoryPath}: {uaEx.Message}");

            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(uaEx, $"Unauthorized access while searching for EBOOT.BIN in directory at {directoryPath}.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[FindEbootBin.FindEbootBinRecursive] Error searching for {targetFileName} in {directoryPath}: {ex.Message}");

            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error while searching for EBOOT.BIN in directory at {directoryPath}.");
        }

        DebugLogger.Log($"[FindEbootBin.FindEbootBinRecursive] {targetFileName} not found in {directoryPath}.");
        return null;
    }
}