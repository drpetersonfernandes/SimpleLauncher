using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace SimpleLauncher;

public class SystemPlayTime
{
    public string SystemName { get; init; }
    public string PlayTime { get; set; }
}

public class SettingsManager
{
    private readonly string _filePath;
    private readonly HashSet<int> _validThumbnailSizes = [100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800];
    private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500];
    private readonly HashSet<string> _validShowGames = ["ShowAll", "ShowWithCover", "ShowWithoutCover"];
    private readonly HashSet<string> _validViewModes = ["GridView", "ListView"];
    private readonly HashSet<string> _validButtonAspectRatio = ["Square", "Wider", "Taller"];

    // Add file access semaphore to prevent concurrent access
    private static readonly SemaphoreSlim FileSemaphore = new SemaphoreSlim(1, 1);

    public int ThumbnailSize { get; set; }
    public int GamesPerPage { get; set; }
    public string ShowGames { get; set; }
    public string ViewMode { get; set; }
    public bool EnableGamePadNavigation { get; set; }
    public string VideoUrl { get; set; }
    public string InfoUrl { get; set; }
    public int MainWindowWidth { get; set; }
    public int MainWindowHeight { get; set; }
    public int MainWindowTop { get; set; }
    public int MainWindowLeft { get; set; }
    public string MainWindowState { get; set; }
    public string BaseTheme { get; set; }
    public string AccentColor { get; set; }
    public string Language { get; set; }
    public float DeadZoneX { get; set; }
    public float DeadZoneY { get; set; }
    public string ButtonAspectRatio { get; set; }

    // List to hold multiple SystemPlayTime instances
    public List<SystemPlayTime> SystemPlayTimes { get; private set; }

    private const string DefaultSettingsFilePath = "settings.xml";

    public SettingsManager() : this(DefaultSettingsFilePath)
    {
    }

    private SettingsManager(string filePath)
    {
        _filePath = filePath;
        SystemPlayTimes = [];
        Load();
    }

    private void Load()
    {
        // Wait to acquire access to the file
        FileSemaphore.Wait();

        try
        {
            if (!File.Exists(_filePath))
            {
                SetDefaultsAndSave();
                return;
            }

            try
            {
                // Use proper file handling with 'using' statement
                using (var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var settings = XElement.Load(fileStream);

                    ThumbnailSize = ValidateThumbnailSize(settings.Element("ThumbnailSize")?.Value);
                    GamesPerPage = ValidateGamesPerPage(settings.Element("GamesPerPage")?.Value);
                    ShowGames = ValidateShowGames(settings.Element("ShowGames")?.Value);
                    ViewMode = ValidateViewMode(settings.Element("ViewMode")?.Value);
                    EnableGamePadNavigation = ParseBoolSetting(settings, "ActivateGamepad");
                    VideoUrl = settings.Element("VideoUrl")?.Value ?? "https://www.youtube.com/results?search_query=";
                    InfoUrl = settings.Element("InfoUrl")?.Value ?? "https://www.igdb.com/search?q=";
                    MainWindowWidth = (int)ValidateDimension(settings.Element("MainWindowWidth")?.Value, 900);
                    MainWindowHeight = (int)ValidateDimension(settings.Element("MainWindowHeight")?.Value, 500);
                    MainWindowTop = (int)ValidateDimension(settings.Element("MainWindowTop")?.Value, 0);
                    MainWindowLeft = (int)ValidateDimension(settings.Element("MainWindowLeft")?.Value, 0);
                    MainWindowState = settings.Element("MainWindowState")?.Value ?? "Normal";
                    BaseTheme = settings.Element("BaseTheme")?.Value ?? "Light";
                    AccentColor = settings.Element("AccentColor")?.Value ?? "Blue";
                    Language = settings.Element("Language")?.Value ?? "en";
                    ButtonAspectRatio = ValidateButtonAspectRatio(settings.Element("ButtonAspectRatio")?.Value);

                    // Parse DeadZoneX value from string to float
                    if (!float.TryParse(settings.Element("DeadZoneX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var deadZoneX))
                    {
                        deadZoneX = 0.05f; // default value
                    }

                    DeadZoneX = deadZoneX;

                    // Parse DeadZoneY value from string to float
                    if (!float.TryParse(settings.Element("DeadZoneY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var deadZoneY))
                    {
                        deadZoneY = 0.02f; // default value
                    }

                    DeadZoneY = deadZoneY;

                    // Load multiple SystemPlayTime elements
                    var systemPlayTimesElement = settings.Element("SystemPlayTimes");
                    if (systemPlayTimesElement != null)
                    {
                        SystemPlayTimes.Clear(); // Clear existing entries to prevent duplicates
                        foreach (var systemPlayTimeElement in systemPlayTimesElement.Elements("SystemPlayTime"))
                        {
                            var systemPlayTime = new SystemPlayTime
                            {
                                SystemName = systemPlayTimeElement.Element("SystemName")?.Value ?? string.Empty,
                                PlayTime = systemPlayTimeElement.Element("PlayTime")?.Value ?? string.Empty
                            };
                            SystemPlayTimes.Add(systemPlayTime);
                        }
                    }
                }

                // Ensure all values are saved if they were missing
                Save();
            }
            catch (Exception ex)
            {
                SetDefaultsAndSave();

                // Notify developer
                const string contextMessage = "Error loading or parsing 'setting.xml'.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.SimpleLauncherNeedMorePrivilegesMessageBox();
            }
        }
        finally
        {
            // Always release the semaphore to prevent deadlocks
            FileSemaphore.Release();
        }
    }

    private int ValidateThumbnailSize(string value)
    {
        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && _validThumbnailSizes.Contains(parsed))
        {
            return parsed;
        }

        return 200;
    }

    private int ValidateGamesPerPage(string value)
    {
        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && _validGamesPerPage.Contains(parsed))
        {
            return parsed;
        }

        return 100;
    }

    private string ValidateShowGames(string value)
    {
        return _validShowGames.Contains(value) ? value : "ShowAll";
    }

    private string ValidateViewMode(string value)
    {
        return _validViewModes.Contains(value) ? value : "GridView";
    }

    private string ValidateButtonAspectRatio(string value)
    {
        return _validButtonAspectRatio.Contains(value) ? value : "Square";
    }

    private static double ValidateDimension(string value, double defaultValue)
    {
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : defaultValue;
    }

    private static bool ParseBoolSetting(XElement settings, string settingName)
    {
        return bool.TryParse(settings.Element(settingName)?.Value, out var value) && value;
    }

    private void SetDefaultsAndSave()
    {
        ThumbnailSize = 200;
        GamesPerPage = 100;
        ShowGames = "ShowAll";
        ViewMode = "GridView";
        EnableGamePadNavigation = false;
        VideoUrl = "https://www.youtube.com/results?search_query=";
        InfoUrl = "https://www.igdb.com/search?q=";
        MainWindowWidth = 900;
        MainWindowHeight = 500;
        MainWindowTop = 0;
        MainWindowLeft = 0;
        MainWindowState = "Normal";
        BaseTheme = "Light";
        AccentColor = "Blue";
        Language = "en";
        DeadZoneX = 0.05f;
        DeadZoneY = 0.02f;
        ButtonAspectRatio = "Square";
        SystemPlayTimes = [];
        Save();
    }

    public void Save()
    {
        // Wait to acquire access to the file
        FileSemaphore.Wait();

        try
        {
            var systemPlayTimesElement = new XElement("SystemPlayTimes");
            foreach (var systemPlayTime in SystemPlayTimes)
            {
                systemPlayTimesElement.Add(new XElement("SystemPlayTime",
                    new XElement("SystemName", systemPlayTime.SystemName),
                    new XElement("PlayTime", systemPlayTime.PlayTime)
                ));
            }

            var settingsElement = new XElement("Settings",
                new XElement("ThumbnailSize", ThumbnailSize),
                new XElement("GamesPerPage", GamesPerPage),
                new XElement("ShowGames", ShowGames),
                new XElement("ViewMode", ViewMode),
                new XElement("ActivateGamepad", EnableGamePadNavigation),
                new XElement("VideoUrl", VideoUrl),
                new XElement("InfoUrl", InfoUrl),
                new XElement("MainWindowWidth", MainWindowWidth),
                new XElement("MainWindowHeight", MainWindowHeight),
                new XElement("MainWindowTop", MainWindowTop),
                new XElement("MainWindowLeft", MainWindowLeft),
                new XElement("MainWindowState", MainWindowState),
                new XElement("BaseTheme", BaseTheme),
                new XElement("AccentColor", AccentColor),
                new XElement("Language", Language),
                new XElement("DeadZoneX", DeadZoneX),
                new XElement("DeadZoneY", DeadZoneY),
                new XElement("ButtonAspectRatio", ButtonAspectRatio),
                systemPlayTimesElement
            );

            // Add retry logic with short delays
            const int maxRetries = 3;
            for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Use proper file handling with 'using' statement and explicit FileShare.None
                    using var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                    settingsElement.Save(fileStream);

                    // Successfully saved, exit the retry loop
                    break;
                }
                catch (IOException) when (attempt < maxRetries)
                {
                    // Wait a short time before retrying
                    Thread.Sleep(100 * attempt); // Progressively longer delays
                }
            }
        }
        finally
        {
            // Always release the semaphore to prevent deadlocks
            FileSemaphore.Release();
        }
    }

    public void UpdateSystemPlayTime(string systemName, TimeSpan playTime)
    {
        if (string.IsNullOrWhiteSpace(systemName))
        {
            // Notify developer
            const string contextMessage = "The systemName is null or empty.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            return;
        }

        if (playTime == TimeSpan.Zero)
        {
            // Notify developer
            const string contextMessage = "The playTime is equal to 0 in the method UpdateSystemPlayTime.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            return;
        }

        // Find the existing System PlayTime or create a new one
        var systemPlayTime = SystemPlayTimes.FirstOrDefault(s => s.SystemName == systemName);

        if (systemPlayTime == null)
        {
            // Add new system playtime if the system doesn't exist
            systemPlayTime = new SystemPlayTime
            {
                SystemName = systemName,
                PlayTime = "00:00:00"
            };
            SystemPlayTimes.Add(systemPlayTime);
        }

        // Parse the existing playtime and add the new time
        if (!TimeSpan.TryParse(systemPlayTime.PlayTime, out var existingPlayTime))
        {
            existingPlayTime = TimeSpan.Zero;
        }

        var updatedPlayTime = existingPlayTime + playTime;

        // Update the playtime in the correct format
        systemPlayTime.PlayTime = updatedPlayTime.ToString(@"hh\:mm\:ss");

        // Save the updated play times
        Save();
    }
}