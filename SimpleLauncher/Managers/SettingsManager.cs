using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

public class SettingsManager
{
    private readonly string _filePath;
    private readonly HashSet<int> _validThumbnailSizes = [50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800];
    private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500, 1000, 10000, 1000000];
    private readonly HashSet<string> _validShowGames = ["ShowAll", "ShowWithCover", "ShowWithoutCover"];
    private readonly HashSet<string> _validViewModes = ["GridView", "ListView"];
    private readonly HashSet<string> _validButtonAspectRatio = ["Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2"];

    private static readonly HashSet<string> KnownSettingsFields =
    [
        "ThumbnailSize", "GamesPerPage", "ShowGames", "ViewMode", "EnableGamePadNavigation",
        "VideoUrl", "InfoUrl", "BaseTheme", "AccentColor", "Language", "DeadZoneX", "DeadZoneY",
        "ButtonAspectRatio", "EnableFuzzyMatching", "FuzzyMatchingThreshold", "EnableNotificationSound",
        "CustomNotificationSoundFile", "RA_Username", "RA_ApiKey", "OverlayRetroAchievementButton",
        "OverlayOpenVideoButton", "OverlayOpenInfoButton", "AdditionalSystemFoldersExpanded",
        "Emulator1Expanded", "Emulator2Expanded", "Emulator3Expanded", "Emulator4Expanded",
        "Emulator5Expanded", "SystemPlayTimes"
    ];


    public int ThumbnailSize { get; set; }
    public int GamesPerPage { get; set; }
    public string ShowGames { get; set; }
    public string ViewMode { get; set; }
    public bool EnableGamePadNavigation { get; set; }
    public string VideoUrl { get; set; }
    public string InfoUrl { get; set; }
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
    public string RaUsername { get; set; }
    public string RaApiKey { get; set; }
    public bool OverlayRetroAchievementButton { get; set; }
    public bool OverlayOpenVideoButton { get; set; }
    public bool OverlayOpenInfoButton { get; set; }
    public bool AdditionalSystemFoldersExpanded { get; set; }
    public bool Emulator1Expanded { get; set; }
    public bool Emulator2Expanded { get; set; }
    public bool Emulator3Expanded { get; set; }
    public bool Emulator4Expanded { get; set; }
    public bool Emulator5Expanded { get; set; }

    public List<SystemPlayTime> SystemPlayTimes { get; private set; }

    private const string DefaultSettingsFilePath = "settings.xml";
    private const string DefaultNotificationSoundFileName = "click.mp3";
    private static readonly string[] Separator = new[] { "<SystemPlayTime>" };

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
        if (!File.Exists(_filePath))
        {
            // Notify user
            Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SettingsFileNotFoundCreatingDefault") ?? "Settings file not found, creating default...", Application.Current.MainWindow as MainWindow));

            SetDefaultsAndSave();
            return;
        }

        try
        {
            XElement settings;

            // Use secure XML settings to prevent XXE attacks
            var xmlSettings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            using (var reader = XmlReader.Create(_filePath, xmlSettings))
            {
                settings = XElement.Load(reader, LoadOptions.None);
            }

            // Check for and remove unrecognized fields
            var childElementNames = settings.Elements().Select(static e => e.Name.LocalName).ToList();
            var unrecognizedFields = childElementNames.Except(KnownSettingsFields).ToList();
            var needsResave = unrecognizedFields.Count != 0;

            // Notify user
            Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingSettings") ?? "Loading settings...", Application.Current.MainWindow as MainWindow));

            ThumbnailSize = ValidateThumbnailSize(settings.Element("ThumbnailSize")?.Value);
            GamesPerPage = ValidateGamesPerPage(settings.Element("GamesPerPage")?.Value);
            ShowGames = ValidateShowGames(settings.Element("ShowGames")?.Value);
            ViewMode = ValidateViewMode(settings.Element("ViewMode")?.Value);
            EnableGamePadNavigation = ParseBoolSetting(settings, "EnableGamePadNavigation");
            VideoUrl = settings.Element("VideoUrl")?.Value ?? "https://www.youtube.com/results?search_query=";
            InfoUrl = settings.Element("InfoUrl")?.Value ?? "https://www.igdb.com/search?q=";
            BaseTheme = settings.Element("BaseTheme")?.Value ?? "Light";
            AccentColor = settings.Element("AccentColor")?.Value ?? "Blue";
            Language = settings.Element("Language")?.Value ?? "en";
            ButtonAspectRatio = ValidateButtonAspectRatio(settings.Element("ButtonAspectRatio")?.Value);
            RaUsername = settings.Element("RA_Username")?.Value ?? string.Empty;
            RaApiKey = settings.Element("RA_ApiKey")?.Value ?? string.Empty;

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

            EnableNotificationSound = ParseBoolSetting(settings, "EnableNotificationSound", true);
            CustomNotificationSoundFile = settings.Element("CustomNotificationSoundFile")?.Value ?? DefaultNotificationSoundFileName;
            if (string.IsNullOrWhiteSpace(CustomNotificationSoundFile))
            {
                CustomNotificationSoundFile = DefaultNotificationSoundFileName;
            }

            OverlayRetroAchievementButton = ParseBoolSetting(settings, "OverlayRetroAchievementButton", false);
            OverlayOpenVideoButton = ParseBoolSetting(settings, "OverlayOpenVideoButton", true);
            OverlayOpenInfoButton = ParseBoolSetting(settings, "OverlayOpenInfoButton", false);

            AdditionalSystemFoldersExpanded = ParseBoolSetting(settings, "AdditionalSystemFoldersExpanded", true);
            Emulator1Expanded = ParseBoolSetting(settings, "Emulator1Expanded", true);
            Emulator2Expanded = ParseBoolSetting(settings, "Emulator2Expanded", true);
            Emulator3Expanded = ParseBoolSetting(settings, "Emulator3Expanded", true);
            Emulator4Expanded = ParseBoolSetting(settings, "Emulator4Expanded", true);
            Emulator5Expanded = ParseBoolSetting(settings, "Emulator5Expanded", true);

            SystemPlayTimes.Clear(); // Clear existing times only after a successful load
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

            if (needsResave)
            {
                Save();
            }
        }
        catch (XmlException ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "There was a XmlException while loading the file 'setting.xml'. Attempting to salvage play time data.");

            // Notify user
            MessageBoxLibrary.SettingsXmlFileIsCorruptMessageBox();

            TrySalvageSystemPlayTimes(_filePath);
            SetDefaultsAndSave();
        }
        catch (IOException ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "There was an IOException while loading the file 'setting.xml'.");

            // Notify user
            MessageBoxLibrary.SettingsXmlFileCouldNotBeLoadedMessageBox();

            SetDefaultsAndSave();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading or parsing 'setting.xml'.");

            // Notify user
            MessageBoxLibrary.SettingsXmlFileCouldNotBeLoadedMessageBox();

            SetDefaultsAndSave();
        }
    }

    private void TrySalvageSystemPlayTimes(string filePath)
    {
        try
        {
            if (!File.Exists(filePath)) return;

            var salvagedPlayTimes = new List<SystemPlayTime>();
            var fileContent = File.ReadAllText(filePath);

            // A simple, non-XML-parser way to find the data, robust against malformed XML
            var playTimesBlockStartIndex = fileContent.IndexOf("<SystemPlayTimes>", StringComparison.Ordinal);
            if (playTimesBlockStartIndex == -1) return;

            var playTimesBlockEndIndex = fileContent.IndexOf("</SystemPlayTimes>", playTimesBlockStartIndex, StringComparison.Ordinal);
            if (playTimesBlockEndIndex == -1) return;

            var playTimesBlock = fileContent.Substring(playTimesBlockStartIndex, playTimesBlockEndIndex - playTimesBlockStartIndex);

            var systemPlayTimeElements = playTimesBlock.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var element in systemPlayTimeElements.Skip(1)) // Skip the part before the first element
            {
                var systemNameMatch = System.Text.RegularExpressions.Regex.Match(element, @"<SystemName>(.*?)</SystemName>");
                var playTimeMatch = System.Text.RegularExpressions.Regex.Match(element, @"<PlayTime>(.*?)</PlayTime>");

                if (systemNameMatch.Success && playTimeMatch.Success)
                {
                    salvagedPlayTimes.Add(new SystemPlayTime
                    {
                        SystemName = systemNameMatch.Groups[1].Value,
                        PlayTime = playTimeMatch.Groups[1].Value
                    });
                }
            }

            if (salvagedPlayTimes.Count > 0)
            {
                SystemPlayTimes = salvagedPlayTimes;
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"Successfully salvaged {salvagedPlayTimes.Count} SystemPlayTime entries from corrupt settings.xml.");
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "An unexpected error occurred while trying to salvage SystemPlayTime data.");
            SystemPlayTimes.Clear(); // Ensure list is empty if salvage fails
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

    private static bool ParseBoolSetting(XElement settings, string settingName, bool defaultValue = false)
    {
        return settingName != null && (bool.TryParse(settings.Element(settingName)?.Value, out var value) ? value : defaultValue);
    }

    private void SetDefaultsAndSave()
    {
        try
        {
            ThumbnailSize = 200;
            GamesPerPage = 100;
            ShowGames = "ShowAll";
            ViewMode = "GridView";
            EnableGamePadNavigation = true;
            VideoUrl = "https://www.youtube.com/results?search_query=";
            InfoUrl = "https://www.igdb.com/search?q=";
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
            RaUsername = string.Empty;
            RaApiKey = string.Empty;
            OverlayRetroAchievementButton = false;
            OverlayOpenVideoButton = true;
            OverlayOpenInfoButton = false;
            AdditionalSystemFoldersExpanded = true;
            Emulator1Expanded = true;
            Emulator2Expanded = true;
            Emulator3Expanded = true;
            Emulator4Expanded = true;
            Emulator5Expanded = true;
            // Do not reset SystemPlayTimes here to allow salvaging from a corrupt file
            Save();

            // Notify user
            Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingSettings") ?? "Saving settings...", Application.Current.MainWindow as MainWindow));
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error saving default settings.");
        }
    }

    public void Save()
    {
        try
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
                new XElement("RA_Username", RaUsername),
                new XElement("RA_ApiKey", RaApiKey),
                new XElement("OverlayRetroAchievementButton", OverlayRetroAchievementButton),
                new XElement("OverlayOpenVideoButton", OverlayOpenVideoButton),
                new XElement("OverlayOpenInfoButton", OverlayOpenInfoButton),
                new XElement("AdditionalSystemFoldersExpanded", AdditionalSystemFoldersExpanded),
                new XElement("Emulator1Expanded", Emulator1Expanded),
                new XElement("Emulator2Expanded", Emulator2Expanded),
                new XElement("Emulator3Expanded", Emulator3Expanded),
                new XElement("Emulator4Expanded", Emulator4Expanded),
                new XElement("Emulator5Expanded", Emulator5Expanded),
                systemPlayTimesElement
            ).Save(_filePath);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error saving settings.");
        }
    }

    public void UpdateSystemPlayTime(string systemName, TimeSpan playTime)
    {
        if (string.IsNullOrWhiteSpace(systemName))
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "The systemName is null or empty.");

            return;
        }

        if (playTime == TimeSpan.Zero)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "The playTime is equal to 0 in the method UpdateSystemPlayTime.");

            return;
        }

        // Notify user
        Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("UpdatingSystemPlayTime") ?? "Updating system play time...", Application.Current.MainWindow as MainWindow));

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Invalid playtime format '{systemPlayTime.PlayTime}' for system '{systemName}'. Resetting to 00:00:00.");

            existingPlayTime = TimeSpan.Zero;
            systemPlayTime.PlayTime = "00:00:00";
        }

        var updatedPlayTime = existingPlayTime + playTime;
        systemPlayTime.PlayTime = updatedPlayTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
    }
}