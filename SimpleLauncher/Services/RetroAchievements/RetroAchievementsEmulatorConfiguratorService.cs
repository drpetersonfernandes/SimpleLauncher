using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.RetroAchievements;

internal static class RetroAchievementsEmulatorConfiguratorService
{
    // RetroArch
    internal static bool ConfigureRetroArch(string exePath, string username, string password)
    {
        var exeDir = Path.GetDirectoryName(exePath);
        if (exeDir != null)
        {
            var configPath = Path.Combine(exeDir, "retroarch.cfg");
            if (!File.Exists(configPath)) return false;

            var settingsToUpdate = new Dictionary<string, string>
            {
                { "cheevos_enable", "true" },
                { "cheevos_username", username },
                { "cheevos_password", password },
                { "cheevos_hardcore_mode_enable", "true" }
            };

            return UpdateSimpleIniFile(configPath, settingsToUpdate, " = ");
        }

        return false;
    }

    // PCSX2
    public static bool ConfigurePcsx2(string exePath, string username, string token)
    {
        var exeDir = Path.GetDirectoryName(exePath);
        var configDir = exeDir != null && Directory.Exists(Path.Combine(exeDir, "inis")) ? Path.Combine(exeDir, "inis") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PCSX2", "inis");
        var configPath = Path.Combine(configDir, "PCSX2.ini");

        if (!File.Exists(configPath)) return false;

        var settingsToUpdate = new Dictionary<string, string>
        {
            { "Enabled", "true" },
            { "Username", username },
            { "Token", token },
            { "Hardcore", "true" }
        };

        return UpdateIniFile(configPath, "Achievements", settingsToUpdate);
    }

    // DuckStation
    public static bool ConfigureDuckStation(string exePath, string username, string token)
    {
        var exeDir = Path.GetDirectoryName(exePath);
        var isPortable = exeDir != null && File.Exists(Path.Combine(exeDir, "portable.txt"));
        var configDir = isPortable ? exeDir : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DuckStation");
        if (configDir != null)
        {
            var configPath = Path.Combine(configDir, "settings.ini");

            if (!File.Exists(configPath))
            {
                return false;
            }

            // Encrypt the token using DuckStation's logic
            var encryptedToken = EncryptDuckStationToken(token, username, isPortable);

            var settingsToUpdate = new Dictionary<string, string>
            {
                { "Enabled", "true" },
                { "ChallengeMode", "true" },
                { "EncoreMode", "false" },
                { "SpectatorMode", "false" },
                { "UnofficialTestMode", "false" },
                { "UseRAIntegration", "false" },
                { "Notifications", "true" },
                { "LeaderboardNotifications", "true" },
                { "LeaderboardTrackers", "true" },
                { "SoundEffects", "true" },
                { "ProgressIndicators", "true" },
                { "NotificationLocation", "TopLeft" },
                { "IndicatorLocation", "BottomRight" },
                { "ChallengeIndicatorMode", "Notification" },
                { "NotificationsDuration", "5" },
                { "LeaderboardsDuration", "10" },
                { "Username", username },
                { "Token", encryptedToken }
            };
            return UpdateIniFile(configPath, "Cheevos", settingsToUpdate);
        }

        return false;
    }

    // PPSSPP
    public static bool ConfigurePpspp(string exePath, string username, string token)
    {
        var exeDir = Path.GetDirectoryName(exePath);
        if (exeDir != null)
        {
            var configDir = Path.Combine(exeDir, "memstick", "PSP", "SYSTEM");
            var configPath = Path.Combine(configDir, "ppsspp.ini");

            if (!File.Exists(configPath)) return false;

            // 1. Update the main INI file
            var settingsToUpdate = new Dictionary<string, string>
            {
                { "AchievementsEnable", "True" },
                { "AchievementsChallengeMode", "True" },
                { "AchievementsUserName", username }
            };

            var iniUpdated = UpdateIniFile(configPath, "Achievements", settingsToUpdate);

            // 2. Save the session key to the separate .dat file
            try
            {
                var datFilePath = Path.Combine(configDir, "ppsspp_retroachievements.dat");
                File.WriteAllText(datFilePath, token);
                return iniUpdated;
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to create PPSSPP session file at {configDir}");
                return false;
            }
        }

        return false;
    }

    // Dolphin
    public static bool ConfigureDolphin(string exePath, string username, string token)
    {
        var exeDir = Path.GetDirectoryName(exePath);
        string configDir;

        if (exeDir != null && File.Exists(Path.Combine(exeDir, "portable.txt")))
        {
            configDir = Path.Combine(exeDir, "User", "Config");
        }
        else
        {
            // %APPDATA%
            configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Dolphin Emulator", "Config");
        }

        var configPath = Path.Combine(configDir, "RetroAchievements.ini");

        try
        {
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
            if (!File.Exists(configPath)) File.WriteAllText(configPath, ""); // Create empty file if missing
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to prepare Dolphin config path at {configDir}");
            return false;
        }

        var settingsToUpdate = new Dictionary<string, string>
        {
            { "ChallengeIndicatorsEnabled", "True" },
            { "DiscordPresenceEnabled", "False" },
            { "Enabled", "True" },
            { "EncoreEnabled", "False" },
            { "HardcoreEnabled", "True" },
            { "LeaderboardTrackerEnabled", "True" },
            { "ProgressEnabled", "True" },
            { "SpectatorEnabled", "False" },
            { "UnofficialEnabled", "False" },
            { "Username", username },
            { "ApiToken", token }
        };

        return UpdateIniFile(configPath, "Achievements", settingsToUpdate);
    }

    // Flycast
    public static bool ConfigureFlycast(string exePath, string username, string token)
    {
        var exeDir = Path.GetDirectoryName(exePath);
        if (exeDir != null)
        {
            var configPath = Path.Combine(exeDir, "emu.cfg");

            // Flycast can also store config in %appdata%\flycast
            if (!File.Exists(configPath))
            {
                configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "flycast", "emu.cfg");
            }

            if (!File.Exists(configPath)) return false;

            var settingsToUpdate = new Dictionary<string, string>
            {
                { "Enabled", "yes" },
                { "HardcoreMode", "yes" },
                { "Token", token },
                { "UserName", username }
            };

            return UpdateIniFile(configPath, "achievements", settingsToUpdate);
        }

        return false;
    }

    // BizHawk
    public static bool ConfigureBizHawk(string exePath, string username, string token)
    {
        var exeDir = Path.GetDirectoryName(exePath);
        if (exeDir != null)
        {
            var configPath = Path.Combine(exeDir, "config.ini");
            if (!File.Exists(configPath)) return false;

            try
            {
                var jsonContent = File.ReadAllText(configPath);

                if (JsonNode.Parse(jsonContent) is not JsonObject jsonNode) return false;

                // BizHawk uses flat root-level keys for RA settings
                jsonNode["SkipRATelemetryWarning"] = true;
                jsonNode["RAUsername"] = username;
                jsonNode["RAToken"] = token;
                jsonNode["RACheevosActive"] = true;
                jsonNode["RALBoardsActive"] = true;
                jsonNode["RARichPresenceActive"] = true;
                jsonNode["RAHardcoreMode"] = true;
                jsonNode["RASoundEffects"] = true;
                jsonNode["RAAllowUnofficialCheevos"] = false;
                jsonNode["RAAutostart"] = true;

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(configPath, jsonNode.ToJsonString(options));
                return true;
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to configure BizHawk at {configPath}");
                return false;
            }
        }

        return false;
    }

    // Helper for simple INI files
    private static bool UpdateSimpleIniFile(string filePath, Dictionary<string, string> settingsToUpdate, string separator)
    {
        try
        {
            var lines = File.ReadAllLines(filePath).ToList();
            var updatedSettings = new HashSet<string>(settingsToUpdate.Keys);

            for (var i = 0; i < lines.Count; i++)
            {
                var trimmedLine = lines[i].Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#')) continue;

                var parts = trimmedLine.Split([separator], 2, StringSplitOptions.None);
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                if (settingsToUpdate.TryGetValue(key, out var newValue))
                {
                    lines[i] = $"{key}{separator}\"{newValue}\"";
                    updatedSettings.Remove(key);
                }
            }

            // Add any settings that were not found
            foreach (var key in updatedSettings)
            {
                lines.Add($"{key}{separator}\"{settingsToUpdate[key]}\"");
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to update simple INI file: {filePath}");
            return false;
        }
    }

    // Helper for standard INI files with [Sections]
    private static bool UpdateIniFile(string filePath, string section, Dictionary<string, string> settingsToUpdate)
    {
        try
        {
            var lines = File.ReadAllLines(filePath).ToList();
            var inSection = false;
            var sectionFound = false;
            var updatedSettings = new HashSet<string>(settingsToUpdate.Keys, StringComparer.OrdinalIgnoreCase);
            var sectionLineIndex = -1;

            for (var i = 0; i < lines.Count; i++)
            {
                var trimmedLine = lines[i].Trim();
                if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']'))
                {
                    if (inSection)
                    {
                        // We've left the target section
                        break;
                    }

                    if (trimmedLine.Equals($"[{section}]", StringComparison.OrdinalIgnoreCase))
                    {
                        inSection = true;
                        sectionFound = true;
                        sectionLineIndex = i;
                    }
                }
                else if (inSection)
                {
                    var parts = trimmedLine.Split(['='], 2);
                    if (parts.Length != 2) continue;

                    var key = parts[0].Trim();
                    if (settingsToUpdate.TryGetValue(key, out var newValue))
                    {
                        lines[i] = $"{key} = {newValue}";
                        updatedSettings.Remove(key);
                    }
                }
            }

            // Add settings that were not found
            if (sectionFound)
            {
                // Find the end of the section to insert new keys
                var insertIndex = sectionLineIndex + 1;
                while (insertIndex < lines.Count && !string.IsNullOrWhiteSpace(lines[insertIndex]) && !lines[insertIndex].Trim().StartsWith('['))
                {
                    insertIndex++;
                }

                foreach (var key in updatedSettings.Reverse()) // Insert in reverse to maintain order
                {
                    lines.Insert(insertIndex, $"{key} = {settingsToUpdate[key]}");
                }
            }
            else // Section not found, add it at the end
            {
                lines.Add("");
                lines.Add($"[{section}]");
                foreach (var kvp in settingsToUpdate)
                {
                    lines.Add($"{kvp.Key} = {kvp.Value}");
                }
            }

            File.WriteAllLines(filePath, lines, Encoding.UTF8);
            return true;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to update INI file: {filePath}");
            return false;
        }
    }

    // --- DuckStation Encryption Helpers ---

    private static string EncryptDuckStationToken(string token, string username, bool isPortable)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username)) return string.Empty;

        try
        {
            var key = GetDuckStationEncryptionKey(username, isPortable);

            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.Zeros; // DuckStation uses zero padding (aligns to block size with 0s)

            // Key is first 16 bytes, IV is last 16 bytes of the 32-byte derived key
            var aesKey = new byte[16];
            var aesIv = new byte[16];
            Array.Copy(key, 0, aesKey, 0, 16);
            Array.Copy(key, 16, aesIv, 0, 16);

            aes.Key = aesKey;
            aes.IV = aesIv;

            var tokenBytes = Encoding.UTF8.GetBytes(token);

            using var encryptor = aes.CreateEncryptor();
            var encryptedBytes = encryptor.TransformFinalBlock(tokenBytes, 0, tokenBytes.Length);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to encrypt DuckStation token.");
            return string.Empty;
        }
    }

    private static byte[] GetDuckStationEncryptionKey(string username, bool isPortable)
    {
        var inputBytes = new List<byte>();

        // Only use machine key if not portable and on Windows
        if (!isPortable && System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            var machineGuid = GetWindowsMachineGuid();
            if (!string.IsNullOrEmpty(machineGuid))
            {
                inputBytes.AddRange(Encoding.UTF8.GetBytes(machineGuid));
            }
        }

        inputBytes.AddRange(Encoding.UTF8.GetBytes(username));

        var key = SHA256.HashData(inputBytes.ToArray());

        // Extra rounds (100)
        for (var i = 0; i < 100; i++)
        {
            key = SHA256.HashData(key);
        }

        return key;
    }

    private static string GetWindowsMachineGuid()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            return key?.GetValue("MachineGuid") as string;
        }
        catch
        {
            return null;
        }
    }
}