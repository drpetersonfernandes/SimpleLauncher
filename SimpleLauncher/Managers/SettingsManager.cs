using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

[MessagePackObject(AllowPrivate = true)]
public class SettingsManager
{
    [IgnoreMember] private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultSettingsFilePath);
    [IgnoreMember] private readonly string _xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OldSettingsFilePath);

    [IgnoreMember] private readonly object _saveLock = new();

    [IgnoreMember] private readonly HashSet<int> _validThumbnailSizes = [50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800];
    [IgnoreMember] private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500, 1000, 10000, 1000000];

    [IgnoreMember] private readonly HashSet<string> _validShowGames = ["ShowAll", "ShowWithCover", "ShowWithoutCover"];
    [IgnoreMember] private readonly HashSet<string> _validViewModes = ["GridView", "ListView"];

    [IgnoreMember] private readonly HashSet<string> _validButtonAspectRatio = ["Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2"];

    [Key(0)] public int ThumbnailSize { get; set; }
    [Key(1)] public int GamesPerPage { get; set; }
    [Key(2)] public string ShowGames { get; set; }
    [Key(3)] public string ViewMode { get; set; }
    [Key(4)] public bool EnableGamePadNavigation { get; set; }
    [Key(5)] public string VideoUrl { get; set; }
    [Key(6)] public string InfoUrl { get; set; }
    [Key(7)] public string BaseTheme { get; set; }
    [Key(8)] public string AccentColor { get; set; }
    [Key(9)] public string Language { get; set; }
    [Key(10)] public float DeadZoneX { get; set; }
    [Key(11)] public float DeadZoneY { get; set; }
    [Key(12)] public string ButtonAspectRatio { get; set; }
    [Key(13)] public bool EnableFuzzyMatching { get; set; }
    [Key(14)] public double FuzzyMatchingThreshold { get; set; }
    [IgnoreMember] public const float DefaultDeadZoneX = 0.05f;
    [IgnoreMember] public const float DefaultDeadZoneY = 0.02f;
    [Key(15)] public bool EnableNotificationSound { get; set; }
    [Key(16)] public string CustomNotificationSoundFile { get; set; }
    [Key(17)] public string RaUsername { get; set; }
    [Key(18)] public string RaApiKey { get; set; }
    [Key(19)] public string RaPassword { get; set; }
    [Key(30)] public string RaToken { get; set; }
    [Key(20)] public bool OverlayRetroAchievementButton { get; set; }
    [Key(21)] public bool OverlayOpenVideoButton { get; set; }
    [Key(22)] public bool OverlayOpenInfoButton { get; set; }
    [Key(23)] public bool AdditionalSystemFoldersExpanded { get; set; }
    [Key(24)] public bool Emulator1Expanded { get; set; }
    [Key(25)] public bool Emulator2Expanded { get; set; }
    [Key(26)] public bool Emulator3Expanded { get; set; }
    [Key(27)] public bool Emulator4Expanded { get; set; }
    [Key(28)] public bool Emulator5Expanded { get; set; }
    [Key(29)] public List<SystemPlayTime> SystemPlayTimes { get; set; } = [];

    [IgnoreMember] private const string DefaultSettingsFilePath = "settings.dat";
    [IgnoreMember] private const string OldSettingsFilePath = "settings.xml";
    [IgnoreMember] private const string DefaultNotificationSoundFileName = "click.mp3";

    public void Load()
    {
        // 1. Try loading from MessagePack (.dat)
        if (File.Exists(_filePath))
        {
            try
            {
                var bytes = File.ReadAllBytes(_filePath);
                var loaded = MessagePackSerializer.Deserialize<SettingsManager>(bytes);
                CopyFrom(loaded);
                return;
            }
            catch (Exception ex)
            {
                if (App.ServiceProvider != null)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading settings.dat. Attempting fallback.");
                }
            }
        }

        // 2. Fallback: Try migrating from old XML (.xml)
        if (File.Exists(_xmlFilePath))
        {
            if (MigrateFromXml())
            {
                return;
            }
        }

        // 3. If nothing exists or migration failed, set defaults
        SetDefaultsAndSave();
    }

    private void CopyFrom(SettingsManager other)
    {
        ThumbnailSize = other.ThumbnailSize;
        GamesPerPage = other.GamesPerPage;
        ShowGames = other.ShowGames;
        ViewMode = other.ViewMode;
        EnableGamePadNavigation = other.EnableGamePadNavigation;
        VideoUrl = other.VideoUrl;
        InfoUrl = other.InfoUrl;
        BaseTheme = other.BaseTheme;
        AccentColor = other.AccentColor;
        Language = other.Language;
        DeadZoneX = other.DeadZoneX;
        DeadZoneY = other.DeadZoneY;
        ButtonAspectRatio = other.ButtonAspectRatio;
        EnableFuzzyMatching = other.EnableFuzzyMatching;
        FuzzyMatchingThreshold = other.FuzzyMatchingThreshold;
        EnableNotificationSound = other.EnableNotificationSound;
        CustomNotificationSoundFile = other.CustomNotificationSoundFile;
        RaUsername = other.RaUsername;
        RaApiKey = other.RaApiKey;
        RaPassword = other.RaPassword;
        RaToken = other.RaToken;
        OverlayRetroAchievementButton = other.OverlayRetroAchievementButton;
        OverlayOpenVideoButton = other.OverlayOpenVideoButton;
        OverlayOpenInfoButton = other.OverlayOpenInfoButton;
        AdditionalSystemFoldersExpanded = other.AdditionalSystemFoldersExpanded;
        Emulator1Expanded = other.Emulator1Expanded;
        Emulator2Expanded = other.Emulator2Expanded;
        Emulator3Expanded = other.Emulator3Expanded;
        Emulator4Expanded = other.Emulator4Expanded;
        Emulator5Expanded = other.Emulator5Expanded;
        SystemPlayTimes = other.SystemPlayTimes ?? [];
    }

    private bool MigrateFromXml()
    {
        try
        {
            DebugLogger.Log("Migrating settings from settings.xml to settings.dat...");
            XElement settings;
            var xmlSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };

            using (var reader = XmlReader.Create(_xmlFilePath, xmlSettings))
            {
                settings = XElement.Load(reader);
            }

            ThumbnailSize = ValidateThumbnailSize(settings.Element("ThumbnailSize")?.Value);
            GamesPerPage = ValidateGamesPerPage(settings.Element("GamesPerPage")?.Value);
            ShowGames = ValidateShowGames(settings.Element("ShowGames")?.Value);
            ViewMode = ValidateViewMode(settings.Element("ViewMode")?.Value);
            EnableGamePadNavigation = !bool.TryParse(settings.Element("EnableGamePadNavigation")?.Value, out var gp) || gp;
            VideoUrl = settings.Element("VideoUrl")?.Value ?? App.Configuration["Urls:YouTubeSearch"] ?? "https://www.youtube.com/results?search_query=";
            InfoUrl = settings.Element("InfoUrl")?.Value ?? App.Configuration["Urls:IgdbSearch"] ?? "https://www.igdb.com/search?q=";
            BaseTheme = settings.Element("BaseTheme")?.Value ?? "Light";
            AccentColor = settings.Element("AccentColor")?.Value ?? "Blue";
            Language = settings.Element("Language")?.Value ?? "en";
            ButtonAspectRatio = ValidateButtonAspectRatio(settings.Element("ButtonAspectRatio")?.Value);
            RaUsername = settings.Element("RA_Username")?.Value ?? string.Empty;
            RaApiKey = settings.Element("RA_ApiKey")?.Value ?? string.Empty;
            RaPassword = settings.Element("RA_Password")?.Value ?? string.Empty;
            RaToken = settings.Element("RA_Token")?.Value ?? string.Empty; // Migrate token if it existed in XML (unlikely but safe)
            DeadZoneX = float.TryParse(settings.Element("DeadZoneX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzx) ? dzx : DefaultDeadZoneX;
            DeadZoneY = float.TryParse(settings.Element("DeadZoneY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzy) ? dzy : DefaultDeadZoneY;
            EnableFuzzyMatching = !bool.TryParse(settings.Element("EnableFuzzyMatching")?.Value, out var fm) || fm;
            FuzzyMatchingThreshold = double.TryParse(settings.Element("FuzzyMatchingThreshold")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fmt) ? fmt : 0.80;
            EnableNotificationSound = !bool.TryParse(settings.Element("EnableNotificationSound")?.Value, out var ens) || ens;
            CustomNotificationSoundFile = settings.Element("CustomNotificationSoundFile")?.Value ?? DefaultNotificationSoundFileName;
            OverlayRetroAchievementButton = bool.TryParse(settings.Element("OverlayRetroAchievementButton")?.Value, out var ora) && ora;
            OverlayOpenVideoButton = !bool.TryParse(settings.Element("OverlayOpenVideoButton")?.Value, out var ovb) || ovb;
            OverlayOpenInfoButton = bool.TryParse(settings.Element("OverlayOpenInfoButton")?.Value, out var oib) && oib;

            var playTimes = settings.Element("SystemPlayTimes");
            if (playTimes != null)
            {
                foreach (var pt in playTimes.Elements("SystemPlayTime"))
                {
                    SystemPlayTimes.Add(new SystemPlayTime
                    {
                        SystemName = pt.Element("SystemName")?.Value ?? "",
                        PlayTime = pt.Element("PlayTime")?.Value ?? "00:00:00"
                    });
                }
            }

            Save(); // Save to .dat

            // Delete old file
            File.Delete(_xmlFilePath);
            DebugLogger.Log("Migration successful. settings.xml deleted.");
            return true;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to migrate settings from XML.");
            return false;
        }
    }

    public void Save()
    {
        lock (_saveLock)
        {
            try
            {
                var tempPath = _filePath + ".tmp";
                var bytes = MessagePackSerializer.Serialize(this);
                File.WriteAllBytes(tempPath, bytes);
                File.Move(tempPath, _filePath, true);
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error saving settings.dat");
            }
        }
    }

    private int ValidateThumbnailSize(string value)
    {
        return int.TryParse(value, out var p) && _validThumbnailSizes.Contains(p) ? p : 250;
    }

    private int ValidateGamesPerPage(string value)
    {
        return int.TryParse(value, out var p) && _validGamesPerPage.Contains(p) ? p : 200;
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

    private void SetDefaultsAndSave()
    {
        ThumbnailSize = 250;
        GamesPerPage = 200;
        ShowGames = "ShowAll";
        ViewMode = "GridView";
        EnableGamePadNavigation = false;
        VideoUrl = App.Configuration["Urls:YouTubeSearch"] ?? "https://www.youtube.com/results?search_query=";
        InfoUrl = App.Configuration["Urls:IgdbSearch"] ?? "https://www.igdb.com/search?q=";
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
        OverlayRetroAchievementButton = false;
        OverlayOpenVideoButton = true;
        OverlayOpenInfoButton = false;
        AdditionalSystemFoldersExpanded = true;
        Emulator1Expanded = true;
        Emulator2Expanded = true;
        Emulator3Expanded = true;
        Emulator4Expanded = true;
        Emulator5Expanded = true;
        SystemPlayTimes = [];
        Save();
    }

    public void UpdateSystemPlayTime(string systemName, TimeSpan playTime)
    {
        if (string.IsNullOrWhiteSpace(systemName) || playTime == TimeSpan.Zero) return;

        lock (_saveLock)
        {
            var item = SystemPlayTimes.FirstOrDefault(s => s.SystemName == systemName);
            if (item == null)
            {
                item = new SystemPlayTime { SystemName = systemName, PlayTime = "00:00:00" };
                SystemPlayTimes.Add(item);
            }

            if (TimeSpan.TryParse(item.PlayTime, CultureInfo.InvariantCulture, out var existing))
            {
                item.PlayTime = (existing + playTime).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
            }
        }
    }
}