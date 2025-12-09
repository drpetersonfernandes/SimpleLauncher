using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.GameScanLogic;

public class ScanMicrosoftStoreGames
{
    // Whitelist for Microsoft Store games to avoid adding Calculator/Photos etc.
    internal static readonly string[] MicrosoftStoreKeywords =
    {
        "Minecraft", "Solitaire", "Forza", "Halo", "Gears of War", "Sea of Thieves", "Flight Simulator", "Age of Empires", "Among Us", "Roblox"
    };

    public static async Task ScanMicrosoftStoreGamesAsync(ILogErrors logErrors, string windowsRomsPath)
    {
        try
        {
            // Sticking to PowerShell for portability in this context, but refining the filter.
            const string script = "Get-StartApps | ConvertTo-Json";

            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null) return;

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (string.IsNullOrWhiteSpace(output)) return;

            using var doc = JsonDocument.Parse(output);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return;

            foreach (var element in doc.RootElement.EnumerateArray())
            {
                try
                {
                    var name = element.GetProperty("Name").GetString();
                    var appId = element.GetProperty("AppID").GetString();

                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(appId)) continue;

                    // Filter: Must match whitelist OR not be a system app (heuristic)
                    // Since we can't easily detect "Game" type via PS Get-StartApps without more complex scripts,
                    // we rely on the whitelist for high-quality detection.
                    var isMatch = GameScannerService.MicrosoftStoreKeywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));

                    if (!isMatch) continue;

                    var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(name);
                    var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.bat");

                    // Use 'start shell:AppsFolder\...' for reliable launching
                    var batchContent = $"@echo off\r\nstart \"\" \"shell:AppsFolder\\{appId}\"";
                    await File.WriteAllTextAsync(shortcutPath, batchContent);

                    // Note: Extracting icons for UWP apps via simple file IO is challenging because
                    // assets are inside WindowsApps folder (restricted) or resources.pri.
                    // We skip icon extraction for UWP here.
                }
                catch
                {
                    /* Ignore */
                }
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, "An error occurred while scanning for Microsoft Store games.");
        }
    }
}