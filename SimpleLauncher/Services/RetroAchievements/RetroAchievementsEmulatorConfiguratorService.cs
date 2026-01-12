using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.RetroAchievements;

public static class RetroAchievementsEmulatorConfiguratorService
{
    // RetroArch
    public static bool ConfigureRetroArch(string exePath, string username, string apiKey, string password)
    {
        var configPath = Path.Combine(Path.GetDirectoryName(exePath)!, "retroarch.cfg");
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

    // // PCSX2
    // public static bool ConfigurePcsx2(string exePath, string username, string apiKey, string password)
    // {
    //     var exeDir = Path.GetDirectoryName(exePath)!;
    //     var configDir = Directory.Exists(Path.Combine(exeDir, "inis")) ? Path.Combine(exeDir, "inis") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PCSX2", "inis");
    //     var configPath = Path.Combine(configDir, "PCSX2.ini");
    //
    //     if (!File.Exists(configPath)) return false;
    //
    //     var settingsToUpdate = new Dictionary<string, string>
    //     {
    //         { "Enabled", "true" },
    //         { "Username", username },
    //         { "Token", password },
    //         { "Hardcore", "true" }
    //     };
    //
    //     return UpdateIniFile(configPath, "Achievements", settingsToUpdate);
    // }

    // // DuckStation
    // public static bool ConfigureDuckStation(string exePath, string username, string apiKey, string password)
    // {
    //     var exeDir = Path.GetDirectoryName(exePath)!;
    //     var configDir = File.Exists(Path.Combine(exeDir, "portable.txt")) ? exeDir : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DuckStation");
    //     var configPath = Path.Combine(configDir, "settings.ini");
    //
    //     if (!File.Exists(configPath)) return false;
    //
    //     var settingsToUpdate = new Dictionary<string, string>
    //     {
    //         { "Enabled", "true" },
    //         { "Username", username },
    //         { "Token", password },
    //         { "Hardcore", "true" }
    //     };
    //
    //     return UpdateIniFile(configPath, "RA", settingsToUpdate);
    // }

    // // PPSSPP
    // public static bool ConfigurePpspp(string exePath, string username, string apiKey, string password)
    // {
    //     var exeDir = Path.GetDirectoryName(exePath)!;
    //     var configPath = Path.Combine(exeDir, "memstick", "PSP", "SYSTEM", "ppsspp.ini");
    //
    //     if (!File.Exists(configPath)) return false;
    //
    //     var settingsToUpdate = new Dictionary<string, string>
    //     {
    //         { "Enable", "True" },
    //         { "Username", username },
    //         { "Token", password },
    //         { "Hardcore", "True" }
    //     };
    //
    //     return UpdateIniFile(configPath, "RetroAchievements", settingsToUpdate);
    // }

    // // Dolphin
    // public static bool ConfigureDolphin(string exePath, string username, string apiKey, string password)
    // {
    //     var exeDir = Path.GetDirectoryName(exePath)!;
    //     var configDir = File.Exists(Path.Combine(exeDir, "portable.txt")) ? Path.Combine(exeDir, "User", "Config") : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Dolphin Emulator", "Config");
    //     var configPath = Path.Combine(configDir, "Dolphin.ini");
    //
    //     if (!File.Exists(configPath)) return false;
    //
    //     var settingsToUpdate = new Dictionary<string, string>
    //     {
    //         { "Enabled", "True" },
    //         { "Username", username },
    //         { "Token", password },
    //         { "HardcoreMode", "True" }
    //     };
    //
    //     return UpdateIniFile(configPath, "RetroAchievements", settingsToUpdate);
    // }

    // Flycast
    public static bool ConfigureFlycast(string exePath, string username, string apiKey, string password)
    {
        var exeDir = Path.GetDirectoryName(exePath)!;
        var configPath = Path.Combine(exeDir, "emu.cfg");

        // Flycast can also store config in %appdata%\flycast
        if (!File.Exists(configPath))
        {
            configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "flycast", "emu.cfg");
        }

        if (!File.Exists(configPath)) return false;

        var settingsToUpdate = new Dictionary<string, string>
        {
            { "enable", "yes" },
            { "username", username },
            { "token", password },
            { "hardcore", "yes" }
        };

        return UpdateIniFile(configPath, "retroachievements", settingsToUpdate);
    }

    // BizHawk
    public static bool ConfigureBizHawk(string exePath, string username, string apiKey, string password)
    {
        var configPath = Path.Combine(Path.GetDirectoryName(exePath)!, "config.json");
        if (!File.Exists(configPath)) return false;

        try
        {
            var jsonContent = File.ReadAllText(configPath);
            var jsonNode = JsonNode.Parse(jsonContent);

            if (jsonNode == null) return false;

            var raNode = jsonNode["RetroAchievements"];
            if (raNode == null)
            {
                raNode = new JsonObject();
                jsonNode["RetroAchievements"] = raNode;
            }

            raNode["Enabled"] = true;
            raNode["Username"] = username;
            raNode["Token"] = password;
            raNode["HardcoreMode"] = true;

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

    // Helper for simple INI files like retroarch.cfg (key = "value")
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

                var parts = trimmedLine.Split(new[] { separator }, 2, StringSplitOptions.None);
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
}