using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Utils;

namespace SimpleLauncher.Services.GameScan;

public class ScanAmazonGames
{
    public static async Task ScanAmazonGamesAsync(ILogErrors logErrors, string windowsRomsPath, string windowsImagesPath, HashSet<string> ignoredGameNames)
    {
        try
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Amazon Games\Data\Games\Sql\GameInstallInfo.sqlite");

            if (!File.Exists(dbPath)) return;

            // Use a connection string that opens in ReadOnly mode to avoid locking issues
            var connectionString = $"Data Source={dbPath};Mode=ReadOnly";

            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, ProductTitle, InstallDirectory FROM DbSet WHERE Installed = 1";

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                try
                {
                    if (!reader.IsDBNull(0))
                    {
                        var gameId = reader.GetString(0);
                        var title = reader.GetString(1);
                        var installDir = reader.GetString(2);

                        if (string.IsNullOrEmpty(title) || ignoredGameNames.Contains(title)) continue;
                        if (!Directory.Exists(installDir)) continue;

                        var sanitizedGameName = SanitizeInputSystemName.SanitizeFolderName(title);
                        var shortcutPath = Path.Combine(windowsRomsPath, $"{sanitizedGameName}.url");

                        // Amazon Games Protocol
                        var shortcutContent = $"[InternetShortcut]\nURL=amazon-games://play/{gameId}";
                        await File.WriteAllTextAsync(shortcutPath, shortcutContent);

                        await GameScannerService.FindAndSaveGameImageAsync(logErrors, title, installDir, sanitizedGameName, windowsImagesPath);
                    }
                }
                catch (Exception ex)
                {
                    await logErrors.LogErrorAsync(ex, "Error processing an Amazon game entry.");
                }
            }
        }
        catch (Exception ex)
        {
            // Log but don't crash if SQLite is missing or DB is locked
            DebugLogger.Log($"[ScanAmazonGames] Error scanning Amazon games: {ex.Message}");
            await logErrors.LogErrorAsync(ex, "Error scanning Amazon games.");
        }
    }
}