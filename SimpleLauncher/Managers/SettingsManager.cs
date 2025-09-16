using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

public class SettingsManager
{
    private readonly string _filePath;
    private readonly HashSet<int> _validThumbnailSizes = [50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800];
    private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500];
    private readonly HashSet<string> _validShowGames = ["ShowAll", "ShowWithCover", "ShowWithoutCover"];
    private readonly HashSet<string> _validViewModes = ["GridView", "ListView"];
    private readonly HashSet<string> _validButtonAspectRatio = ["Square", "Wider", "SuperWider", "Taller", "SuperTaller"];

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

    public bool EnableFuzzyMatching { get; set; }
    public double FuzzyMatchingThreshold { get; set; }

    public const float DefaultDeadZoneX = 0.05f;
    public const float DefaultDeadZoneY = 0.02f;
    public bool EnableNotificationSound { get; set; }
    public string CustomNotificationSoundFile { get; set; }

    public List<SystemPlayTime> SystemPlayTimes { get; private set; }

    private const string DefaultSettingsFilePath = "settings.xml";
    private const string DefaultNotificationSoundFileName = "click.mp3";

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
        SystemPlayTimes.Clear();

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
            EnableGamePadNavigation = ParseBoolSetting(settings, "EnableGamePadNavigation");
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

            if (!float.TryParse(settings.Element("DeadZoneX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var deadZoneX))
            {
                deadZoneX = DefaultDeadZoneX;
            }

            DeadZoneX = deadZoneX;

            if (!float.TryParse(settings.Element("DeadZoneY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var deadZoneY))
            {
                deadZoneY = DefaultDeadZoneY;
            }

            DeadZoneY = deadZoneY;

            EnableFuzzyMatching = ParseBoolSetting(settings, "EnableFuzzyMatching");
            if (!double.TryParse(settings.Element("FuzzyMatchingThreshold")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fuzzyThreshold))
            {
                fuzzyThreshold = 0.80;
            }

            FuzzyMatchingThreshold = fuzzyThreshold;

            EnableNotificationSound = ParseBoolSetting(settings, "EnableNotificationSound", true); // Default to true if not found
            CustomNotificationSoundFile = settings.Element("CustomNotificationSoundFile")?.Value ?? DefaultNotificationSoundFileName;
            if (string.IsNullOrWhiteSpace(CustomNotificationSoundFile)) // Ensure it's not empty
            {
                CustomNotificationSoundFile = DefaultNotificationSoundFileName;
            }


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

            Save();
        }
        catch (XmlException ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "There was a XmlException while loading the file 'setting.xml'.");

            // Notify user
            MessageBoxLibrary.SimpleLauncherNeedMorePrivilegesMessageBox();

            SetDefaultsAndSave();
        }
        catch (IOException ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "There was an IOException while loading the file 'setting.xml'.");

            // Notify user
            MessageBoxLibrary.SimpleLauncherNeedMorePrivilegesMessageBox();

            SetDefaultsAndSave();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error loading or parsing 'setting.xml'.");

            // Notify user
            MessageBoxLibrary.SimpleLauncherNeedMorePrivilegesMessageBox();

            SetDefaultsAndSave();
        }
    }

    private int ValidateThumbnailSize(string value)
    {
        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && _validThumbnailSizes.Contains(parsed))
            return parsed;

        return 200;
    }

    private int ValidateGamesPerPage(string value)
    {
        if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) && _validGamesPerPage.Contains(parsed))
            return parsed;

        return 100;
    }

    private string ValidateShowGames(string value)
    {
        return !string.IsNullOrEmpty(value) && _validShowGames.Contains(value) ? value : "ShowAll";
    }

    private string ValidateViewMode(string value)
    {
        return !string.IsNullOrEmpty(value) && _validViewModes.Contains(value) ? value : "GridView";
    }

    private string ValidateButtonAspectRatio(string value)
    {
        return !string.IsNullOrEmpty(value) && _validButtonAspectRatio.Contains(value) ? value : "Square";
    }

    private static double ValidateDimension(string value, double defaultValue)
    {
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : defaultValue;
    }

    private static bool ParseBoolSetting(XElement settings, string settingName, bool defaultValue = false)
    {
        return settingName != null && (bool.TryParse(settings.Element(settingName)?.Value, out var value) ? value : defaultValue);
    }

    private void SetDefaultsAndSave()
    {
        ThumbnailSize = 200;
        GamesPerPage = 100;
        ShowGames = "ShowAll";
        ViewMode = "GridView";
        EnableGamePadNavigation = true;
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
        DeadZoneX = DefaultDeadZoneX;
        DeadZoneY = DefaultDeadZoneY;
        ButtonAspectRatio = "Square";
        EnableFuzzyMatching = true;
        FuzzyMatchingThreshold = 0.80;
        EnableNotificationSound = true;
        CustomNotificationSoundFile = DefaultNotificationSoundFileName;
        SystemPlayTimes = [];
        Save();
    }

    public void Save()
    {
        var systemPlayTimesElement = new XElement("SystemPlayTimes");
        foreach (var systemPlayTime in SystemPlayTimes.Where(static s => !string.IsNullOrWhiteSpace(s.SystemName)))
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
            new XElement("EnableGamePadNavigation", EnableGamePadNavigation),
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
            new XElement("DeadZoneX", DeadZoneX.ToString(CultureInfo.InvariantCulture)),
            new XElement("DeadZoneY", DeadZoneY.ToString(CultureInfo.InvariantCulture)),
            new XElement("ButtonAspectRatio", ButtonAspectRatio),
            new XElement("EnableFuzzyMatching", EnableFuzzyMatching),
            new XElement("FuzzyMatchingThreshold", FuzzyMatchingThreshold.ToString(CultureInfo.InvariantCulture)),
            new XElement("EnableNotificationSound", EnableNotificationSound),
            new XElement("CustomNotificationSoundFile", CustomNotificationSoundFile),
            systemPlayTimesElement
        ).Save(_filePath);
    }

    public void UpdateSystemPlayTime(string systemName, TimeSpan playTime)
    {
        if (string.IsNullOrWhiteSpace(systemName))
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(null, "The systemName is null or empty.");

            return;
        }

        if (playTime == TimeSpan.Zero)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(null, "The playTime is equal to 0 in the method UpdateSystemPlayTime.");

            return;
        }

        var systemPlayTime = SystemPlayTimes.FirstOrDefault(s => s.SystemName == systemName);
        if (systemPlayTime == null)
        {
            systemPlayTime = new SystemPlayTime { SystemName = systemName, PlayTime = "00:00:00" };
            SystemPlayTimes.Add(systemPlayTime);
        }

        var existingPlayTime = TimeSpan.Zero;
        try
        {
            if (!string.IsNullOrWhiteSpace(systemPlayTime.PlayTime))
            {
                existingPlayTime = TimeSpan.Parse(systemPlayTime.PlayTime, CultureInfo.InvariantCulture);
            }
        }
        catch (FormatException ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, $"Invalid playtime format '{systemPlayTime.PlayTime}' for system '{systemName}'. Resetting to 00:00:00.");

            existingPlayTime = TimeSpan.Zero;
            systemPlayTime.PlayTime = "00:00:00";
        }

        var updatedPlayTime = existingPlayTime + playTime;
        systemPlayTime.PlayTime = updatedPlayTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
    }
}