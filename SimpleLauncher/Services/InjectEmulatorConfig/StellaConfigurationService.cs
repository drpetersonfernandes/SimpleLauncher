using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Data.Sqlite;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class StellaConfigurationService
{
    public static void InjectSettings(string emulatorPath, SettingsManager.SettingsManager settings)
    {
        var emuDir = Path.GetDirectoryName(emulatorPath);
        if (string.IsNullOrEmpty(emuDir))
            throw new InvalidOperationException("Emulator directory not found.");

        var configPath = Path.Combine(emuDir, "stella.sqlite3");

        if (!File.Exists(configPath))
        {
            var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", "Stella", "stella.sqlite3");
            if (File.Exists(samplePath))
            {
                File.Copy(samplePath, configPath);
                DebugLogger.Log($"[StellaConfig] Created new stella.sqlite3 from sample: {configPath}");
            }
            else
            {
                throw new FileNotFoundException("stella.sqlite3 not found and sample is missing.", samplePath);
            }
        }

        DebugLogger.Log($"[StellaConfig] Injecting configuration into: {configPath}");

        var updates = new Dictionary<string, string>
        {
            { "fullscreen", settings.StellaFullscreen ? "1" : "0" },
            { "vsync", settings.StellaVsync ? "true" : "false" },
            { "video", settings.StellaVideoDriver },
            { "tia.correct_aspect", settings.StellaCorrectAspect ? "true" : "false" },
            { "tv.filter", settings.StellaTvFilter.ToString(CultureInfo.InvariantCulture) },
            { "tv.scanlines", settings.StellaScanlines.ToString(CultureInfo.InvariantCulture) },
            { "audio.enabled", settings.StellaAudioEnabled ? "1" : "0" },
            { "audio.volume", settings.StellaAudioVolume.ToString(CultureInfo.InvariantCulture) },
            { "dev.timemachine", settings.StellaTimeMachine ? "1" : "0" },
            { "confirmexit", settings.StellaConfirmExit ? "1" : "0" }
        };

        var connectionString = new SqliteConnectionStringBuilder { DataSource = configPath }.ToString();
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        foreach (var kvp in updates)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                "INSERT INTO settings (setting, value) VALUES ($setting, $value) " +
                "ON CONFLICT(setting) DO UPDATE SET value = excluded.value;";
            command.Parameters.AddWithValue("$setting", kvp.Key);
            command.Parameters.AddWithValue("$value", kvp.Value);
            command.ExecuteNonQuery();
        }

        transaction.Commit();

        DebugLogger.Log("[StellaConfig] Injection successful.");
    }
}