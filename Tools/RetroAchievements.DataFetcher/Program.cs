using System.Text.Json;
using MessagePack;
using System.Xml.Serialization;
using System.Xml;
using RetroAchievements.DataFetcher.Models;

namespace RetroAchievements.DataFetcher;

file static class Program
{
    private const string SettingsFilePath = "settings.xml";
    private const string ConsoleListFilePath = "consoles.txt";
    private const string OutputFileNameJson = "all_ra_games.json";
    private const string OutputFileNameMsgPack = "all_ra_games.dat";
    private const string BaseApiUrl = "https://retroachievements.org/API";

    private static async Task Main(string[] args)
    {
        if (args.Length == 1 && Path.GetExtension(args[0]).Equals(".json", StringComparison.OrdinalIgnoreCase))
        {
            await RunConversionMode(args[0]);
            return;
        }

        await RunFetchMode();
    }

    private static async Task RunConversionMode(string jsonFilePath)
    {
        LogInfo($"Conversion mode: Processing JSON file '{jsonFilePath}'...");

        try
        {
            var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            var games = JsonSerializer.Deserialize<List<GameInfo>>(jsonContent);

            if (games == null || games.Count == 0)
            {
                LogError("The JSON file is empty or invalid. No data to convert.");
            }
            else
            {
                var msgPackFilePath = Path.ChangeExtension(jsonFilePath, ".dat");
                LogInfo($"Converting {games.Count:N0} games to MessagePack...");
                var msgPackData = MessagePackSerializer.Serialize(games);
                await File.WriteAllBytesAsync(msgPackFilePath, msgPackData);
                LogSuccess($"MessagePack file saved as '{msgPackFilePath}'");
            }
        }
        catch (Exception ex)
        {
            LogError($"Conversion failed: {ex.Message}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static async Task RunFetchMode()
    {
        var settings = await LoadOrPromptSettings();
        using HttpClient client = new();

        LogInfo("Starting RetroAchievements game data fetcher...");
        LogInfo($"Authenticated as: {settings.Username}");

        var allGames = new List<GameInfo>();
        var serializerOptions = new JsonSerializerOptions { WriteIndented = true };

        try
        {
            // Fetch consoles
            LogInfo("Fetching console list...");
            var consoles = await FetchConsoles(client, settings);

            if (consoles.Count == 0)
            {
                LogError("No consoles found. Aborting.");
                Environment.Exit(1);
            }

            LogSuccess($"Found {consoles.Count:N0} consoles");
            await SaveConsoleList(consoles);
            Console.WriteLine();

            // Filter for active game systems only
            var activeConsoles = consoles.Where(static c => c.Active && c.IsGameSystem).ToList();
            LogInfo($"Processing {activeConsoles.Count} active game consoles...");

            // Fetch games for each console
            var totalGames = await FetchGamesForAllConsoles(client, settings, activeConsoles, allGames);

            Console.WriteLine();
            LogInfo($"Total games fetched: {totalGames:N0}");

            // Save results
            if (allGames.Count > 0)
            {
                await SaveGameData(allGames, serializerOptions);
            }
            else
            {
                LogWarning("No games were found to save.");
            }
        }
        catch (Exception ex)
        {
            LogError($"Critical error: {ex.Message}");
            LogError("Process incomplete.");
            Environment.Exit(1);
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static async Task<List<ConsoleInfo>> FetchConsoles(HttpClient client, RaSettings settings)
    {
        var auth = $"u={settings.Username}&y={settings.WebApiKey}";
        var url = $"{BaseApiUrl}/API_GetConsoleIDs.php?{auth}";

        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var consoles = JsonSerializer.Deserialize<List<ConsoleInfo>>(json);

        return consoles ?? new List<ConsoleInfo>();
    }

    private static async Task<int> FetchGamesForAllConsoles(
        HttpClient client,
        RaSettings settings,
        List<ConsoleInfo> consoles,
        List<GameInfo> allGames)
    {
        var totalGames = 0;
        var auth = $"u={settings.Username}&y={settings.WebApiKey}";

        for (var i = 0; i < consoles.Count; i++)
        {
            var console = consoles[i];
            LogInfo($"[{i + 1}/{consoles.Count}] Fetching games for '{console.Name}' (ID: {console.Id})...");

            try
            {
                var url = $"{BaseApiUrl}/API_GetGameList.php?{auth}&i={console.Id}&h=1&f=1";
                var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    LogWarning($"  -> Failed: HTTP {response.StatusCode}");
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                var games = JsonSerializer.Deserialize<List<GameInfo>>(json);

                if (games?.Count > 0)
                {
                    allGames.AddRange(games);
                    totalGames += games.Count;
                    LogSuccess($"  -> Found {games.Count:N0} games");
                }
                else
                {
                    LogInfo("  -> No games with achievements");
                }

                await Task.Delay(500); // Rate limiting
            }
            catch (Exception ex)
            {
                LogWarning($"  -> Error: {ex.Message}");
            }
        }

        return totalGames;
    }

    private static async Task SaveGameData(List<GameInfo> games, JsonSerializerOptions options)
    {
        LogInfo($"Saving {games.Count:N0} games to '{OutputFileNameJson}'...");
        var json = JsonSerializer.Serialize(games, options);
        await File.WriteAllTextAsync(OutputFileNameJson, json);
        LogSuccess("JSON file saved successfully");

        LogInfo($"Saving {games.Count:N0} games to '{OutputFileNameMsgPack}'...");
        var msgPack = MessagePackSerializer.Serialize(games);
        await File.WriteAllBytesAsync(OutputFileNameMsgPack, msgPack);
        LogSuccess("MessagePack file saved successfully");
    }

    private static Task<RaSettings> LoadOrPromptSettings()
    {
        var settings = new RaSettings();

        if (File.Exists(SettingsFilePath))
        {
            try
            {
                using var stream = new FileStream(SettingsFilePath, FileMode.Open, FileAccess.Read);
                using var xmlReader = XmlReader.Create(stream, new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null // Disable XML resolver for security
                });
                var serializer = new XmlSerializer(typeof(RaSettings));
                settings = (RaSettings)serializer.Deserialize(xmlReader)!;
                LogInfo($"Loaded settings for user '{settings.Username}'");
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to load settings: {ex.Message}");
                settings = new RaSettings();
            }
        }

        var hasValidSettings = !string.IsNullOrWhiteSpace(settings.Username) &&
                               !string.IsNullOrWhiteSpace(settings.WebApiKey);

        if (hasValidSettings)
        {
            Console.WriteLine($"\nCurrent username: '{settings.Username}'");
            Console.Write("Update credentials? (y/n): ");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (response != "y" && response != "yes")
            {
                return Task.FromResult(settings);
            }
        }
        else
        {
            Console.WriteLine("\n--- No valid settings found. Please enter your credentials ---");
        }

        Console.Write("Username: ");
        settings.Username = Console.ReadLine()?.Trim() ?? string.Empty;

        Console.Write("Web API Key: ");
        settings.WebApiKey = Console.ReadLine()?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(settings.Username) || string.IsNullOrWhiteSpace(settings.WebApiKey))
        {
            LogError("Username and Web API Key cannot be empty.");
            Environment.Exit(1);
        }

        try
        {
            using var stream = new FileStream(SettingsFilePath, FileMode.Create, FileAccess.Write);
            using var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings
            {
                Indent = true,
                Encoding = System.Text.Encoding.UTF8
            });
            var serializer = new XmlSerializer(typeof(RaSettings));
            serializer.Serialize(xmlWriter, settings);
            LogSuccess($"Settings saved to {SettingsFilePath}");
        }
        catch (Exception ex)
        {
            LogWarning($"Could not save settings: {ex.Message}");
        }

        return Task.FromResult(settings);
    }

    private static async Task SaveConsoleList(List<ConsoleInfo> consoles)
    {
        try
        {
            var lines = consoles.Select(c => $"{c.Id:D3}: {c.Name}");
            await File.WriteAllLinesAsync(ConsoleListFilePath, lines);
            LogInfo($"Console list saved to '{ConsoleListFilePath}' ({consoles.Count} entries)");
        }
        catch (Exception ex)
        {
            LogWarning($"Could not save console list: {ex.Message}");
        }
    }

    #region Logging Helpers

    private static void LogInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"[INFO] {DateTime.Now:T}: {message}");
        Console.ResetColor();
    }

    private static void LogSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[SUCCESS] {DateTime.Now:T}: {message}");
        Console.ResetColor();
    }

    private static void LogWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[WARNING] {DateTime.Now:T}: {message}");
        Console.ResetColor();
    }

    private static void LogError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[ERROR] {DateTime.Now:T}: {message}");
        Console.ResetColor();
    }

    #endregion
}
