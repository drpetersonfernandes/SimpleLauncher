using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;

namespace SimpleLauncher;

public class SystemPlayTime
{
    public string SystemName { get; init; }
    public string PlayTime { get; set; }
}

public class SettingsConfig
{
    private readonly string _filePath;
    private readonly HashSet<int> _validThumbnailSizes = [100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600];
    private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500, 1000];
    private readonly HashSet<string> _validShowGames = ["ShowAll", "ShowWithCover", "ShowWithoutCover"];
    private readonly HashSet<string> _validViewModes =
    [
        "GridView", "ListView"
    ];

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

    // List to hold multiple SystemPlayTime instances
    public List<SystemPlayTime> SystemPlayTimes { get; private set; }

    private const string DefaultSettingsFilePath = "settings.xml";

    public SettingsConfig() : this(DefaultSettingsFilePath) { }

    private SettingsConfig(string filePath)
    {
        _filePath = filePath;
        SystemPlayTimes = [];
        Load();
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
        {
            SetDefaultsAndSave();
            return;
        }

        try
        {
            var settings = XElement.Load(_filePath);

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

            // Load multiple SystemPlayTime elements
            var systemPlayTimesElement = settings.Element("SystemPlayTimes");
            if (systemPlayTimesElement != null)
            {
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

            // Ensure all values are saved if they were missing
            Save();
        }
        catch (Exception ex)
        {
            SetDefaultsAndSave();

            // Notify developer
            var contextMessage = $"Error loading or parsing 'setting.xml'.\n\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.SimpleLauncherNeedMorePrivilegesMessageBox();
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
        SystemPlayTimes = [];
        Save();
    }

    public void Save()
    {
        var systemPlayTimesElement = new XElement("SystemPlayTimes");
        foreach (var systemPlayTime in SystemPlayTimes)
        {
            systemPlayTimesElement.Add(new XElement("SystemPlayTime",
                new XElement("SystemName", systemPlayTime.SystemName),
                new XElement("PlayTime", systemPlayTime.PlayTime)
            ));
        }

        new XElement("Settings",
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
            systemPlayTimesElement
        ).Save(_filePath);
    }
        
    public void UpdateSystemPlayTime(string systemName, TimeSpan playTime)
    {
        if (string.IsNullOrWhiteSpace(systemName))
        {
            // Notify developer
            const string contextMessage = "The systemName is null or empty.";
            Exception ex = new();
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));
                
            return;
        }

        if (playTime == TimeSpan.Zero)
        {
            // Notify developer
            const string contextMessage = "The playTime is equal to 0 in the method UpdateSystemPlayTime.";
            Exception ex = new();
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));
                
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
        var existingPlayTime = TimeSpan.Parse(systemPlayTime.PlayTime);
        var updatedPlayTime = existingPlayTime + playTime;

        // Update the playtime in the correct format
        systemPlayTime.PlayTime = updatedPlayTime.ToString(@"hh\:mm\:ss");
    }
}