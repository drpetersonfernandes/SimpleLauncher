using System.Globalization;
using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.AppDataFile;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

namespace SimpleLauncher.Core.Services.SettingsManager;

public class SettingsManager : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly ICredentialProtector _credentialProtector;

    private readonly DataFileLocation _fileLocation;
    private readonly ReaderWriterLockSlim _settingsLock = new(LockRecursionPolicy.SupportsRecursion);

    private readonly HashSet<int> _validThumbnailSizes = [50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800];
    private readonly HashSet<int> _validThumbnailSizesForSystem = [50, 100, 150];
    private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500, 1000, 10000, 1000000];
    private readonly HashSet<string> _validShowGames = ["ShowAll", "ShowWithCover", "ShowWithoutCover"];
    private readonly HashSet<string> _validViewModes = ["GridView", "ListView"];
    private readonly HashSet<string> _validButtonAspectRatio = ["Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2"];
    private readonly HashSet<string> _validFilenameDisplayModes = ["Original", "CleanUp", "NoFilename"];
    private readonly HashSet<string> _validFontSizes = ["Small", "Normal", "Big"];
    private readonly HashSet<string> _validStyleVariants = ["Default"];
    private readonly HashSet<string> _validBaseThemes = ["Light", "Dark", "Adaptive", "HighContrast", "Midnight"];
    private readonly HashSet<string> _validAccentColors = ["Amber", "Blue", "Brown", "Cobalt", "Crimson", "Cyan", "Emerald", "Green", "Indigo", "Lime", "Magenta", "Maroon", "Mauve", "Olive", "OliveDrab", "Orange", "Pink", "Plum", "Purple", "Red", "Sienna", "SkyBlue", "Steel", "Taupe", "Teal", "Violet", "Yellow"];
    private bool _disposed;

    // Application Settings
    public int ThumbnailSize { get; set; } = 250;
    public int ThumbnailSizeForSystem { get; set; } = 50;
    public int GamesPerPage { get; set; } = 200;
    public string ShowGames { get; set; } = "ShowAll";
    public string ViewMode { get; set; } = "GridView";
    public bool EnableGamePadNavigation { get; set; }
    public string VideoUrl { get; set; }
    public string InfoUrl { get; set; }
    public string BaseTheme { get; set; } = "Dark";
    public string AccentColor { get; set; } = "Blue";
    public string StyleVariant { get; set; } = "Default";
    public string Language { get; set; } = "en";
    public float DeadZoneX { get; set; } = DefaultDeadZoneX;
    public float DeadZoneY { get; set; } = DefaultDeadZoneY;
    public string ButtonAspectRatio { get; set; } = "Square";
    public string FilenameDisplayMode { get; set; } = "Original";
    public bool DisplayMachineName { get; set; }
    public string FilenameFontSize { get; set; } = "Normal";
    public string MachineNameFontSize { get; set; } = "Normal";
    public bool EnableFuzzyMatching { get; set; } = true;
    public double FuzzyMatchingThreshold { get; set; } = 0.80;
    public const float DefaultDeadZoneX = 0.05f;
    public const float DefaultDeadZoneY = 0.02f;
    public bool EnableNotificationSound { get; set; } = true;
    public string CustomNotificationSoundFile { get; set; } = DefaultNotificationSoundFileName;
    public string RaUsername { get; set; } = string.Empty;
    public string RaApiKey { get; set; } = string.Empty;
    public string RaPassword { get; set; } = string.Empty;
    public string RaToken { get; set; } = string.Empty;
    public bool OverlayRetroAchievementButton { get; set; }
    public bool OverlayOpenVideoButton { get; set; } = true;
    public bool OverlayOpenInfoButton { get; set; }
    public bool AdditionalSystemFoldersExpanded { get; set; } = true;
    public bool Emulator1Expanded { get; set; } = true;
    public bool Emulator2Expanded { get; set; } = true;
    public bool Emulator3Expanded { get; set; } = true;
    public bool Emulator4Expanded { get; set; } = true;
    public bool Emulator5Expanded { get; set; } = true;
    public List<SystemPlayTime> SystemPlayTimes { get; set; } = [];

    // Emulator Settings (composition)
    public AresSettings Ares { get; set; } = new();
    public AzaharSettings Azahar { get; set; } = new();
    public BlastemSettings Blastem { get; set; } = new();
    public CemuSettings Cemu { get; set; } = new();
    public DaphneSettings Daphne { get; set; } = new();
    public DolphinSettings Dolphin { get; set; } = new();
    public DuckStationSettings DuckStation { get; set; } = new();
    public FlycastSettings Flycast { get; set; } = new();
    public MameSettings Mame { get; set; } = new();
    public MednafenSettings Mednafen { get; set; } = new();
    public MesenSettings Mesen { get; set; } = new();
    public Pcsx2Settings Pcsx2 { get; set; } = new();
    public RaineSettings Raine { get; set; } = new();
    public RedreamSettings Redream { get; set; } = new();
    public RetroArchSettings RetroArch { get; set; } = new();
    public Rpcs3Settings Rpcs3 { get; set; } = new();
    public SegaModel2Settings SegaModel2 { get; set; } = new();
    public StellaSettings Stella { get; set; } = new();
    public SupermodelSettings Supermodel { get; set; } = new();
    public XeniaSettings Xenia { get; set; } = new();
    public YumirSettings Yumir { get; set; } = new();

    private const string DefaultSettingsFilePath = "settings.xml";
    private const string DefaultNotificationSoundFileName = "click.mp3";
    private const string EncryptedPrefix = "DPAPI:";

    private string EncryptString(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return plainText;

        try
        {
            return _credentialProtector.Protect(plainText);
        }
        catch
        {
            return plainText;
        }
    }

    private string DecryptString(string storedValue)
    {
        if (string.IsNullOrEmpty(storedValue)) return storedValue;

        if (storedValue.StartsWith(EncryptedPrefix, StringComparison.Ordinal))
        {
            var legacyValue = storedValue[EncryptedPrefix.Length..];
            try
            {
                return _credentialProtector.Unprotect(legacyValue);
            }
            catch
            {
                return legacyValue;
            }
        }

        try
        {
            return _credentialProtector.Unprotect(storedValue);
        }
        catch
        {
            return storedValue;
        }
    }

    public SettingsManager(IConfiguration configuration, ILogErrors logErrors, ICredentialProtector credentialProtector, IMessageBoxLibraryService messageBox = null)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _credentialProtector = credentialProtector;
        _messageBox = messageBox;
        _fileLocation = new DataFileLocation(DefaultSettingsFilePath);

        VideoUrl = configuration.GetValue<string>("Urls:YouTubeSearch") ?? "https://www.youtube.com/results?search_query=";
        InfoUrl = configuration.GetValue<string>("Urls:IgdbSearch") ?? "https://www.igdb.com/search?q=";
    }

    public bool IsPortableMode => _fileLocation.IsPortableMode;

    public void Load()
    {
        XElement settings = null;

        // Read from disk without holding any lock — disk I/O is slow and
        // does not mutate shared state, so concurrent readers are unaffected.
        if (File.Exists(_fileLocation.FilePath))
        {
            try
            {
                settings = XElement.Load(_fileLocation.FilePath);
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error loading settings.xml.");
            }
        }

        // Take a write lock only for the in-memory property updates.
        _settingsLock.EnterWriteLock();
        try
        {
            if (settings != null)
            {
                LoadFromXml(settings);
            }
            else
            {
                SetDefaultsAndSave();
            }
        }
        finally
        {
            _settingsLock.ExitWriteLock();
        }
    }

    private void CopyFrom(SettingsManager other)
    {
        ArgumentNullException.ThrowIfNull(other);

        // Application Settings
        ThumbnailSize = other.ThumbnailSize;
        ThumbnailSizeForSystem = other.ThumbnailSizeForSystem;
        GamesPerPage = other.GamesPerPage;
        ShowGames = other.ShowGames;
        ViewMode = other.ViewMode;
        EnableGamePadNavigation = other.EnableGamePadNavigation;
        VideoUrl = other.VideoUrl;
        InfoUrl = other.InfoUrl;
        BaseTheme = other.BaseTheme;
        AccentColor = other.AccentColor;
        StyleVariant = other.StyleVariant;
        Language = other.Language;
        DeadZoneX = other.DeadZoneX;
        DeadZoneY = other.DeadZoneY;
        ButtonAspectRatio = other.ButtonAspectRatio;
        FilenameDisplayMode = other.FilenameDisplayMode;
        DisplayMachineName = other.DisplayMachineName;
        FilenameFontSize = other.FilenameFontSize;
        MachineNameFontSize = other.MachineNameFontSize;
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

        SystemPlayTimes = other.SystemPlayTimes?
            .Select(static pt => new SystemPlayTime { SystemName = pt.SystemName, PlayTimeSeconds = pt.PlayTimeSeconds })
            .ToList() ?? [];

        // Emulator Settings (delegate to each emulator's CopyFrom)
        Ares.CopyFrom(other.Ares);
        Azahar.CopyFrom(other.Azahar);
        Blastem.CopyFrom(other.Blastem);
        Cemu.CopyFrom(other.Cemu);
        Daphne.CopyFrom(other.Daphne);
        Dolphin.CopyFrom(other.Dolphin);
        DuckStation.CopyFrom(other.DuckStation);
        Flycast.CopyFrom(other.Flycast);
        Mame.CopyFrom(other.Mame);
        Mednafen.CopyFrom(other.Mednafen);
        Mesen.CopyFrom(other.Mesen);
        Pcsx2.CopyFrom(other.Pcsx2);
        Raine.CopyFrom(other.Raine);
        Redream.CopyFrom(other.Redream);
        RetroArch.CopyFrom(other.RetroArch);
        Rpcs3.CopyFrom(other.Rpcs3);
        SegaModel2.CopyFrom(other.SegaModel2);
        Stella.CopyFrom(other.Stella);
        Supermodel.CopyFrom(other.Supermodel);
        Xenia.CopyFrom(other.Xenia);
        Yumir.CopyFrom(other.Yumir);
    }

    private void LoadFromXml(XElement settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        // Application Settings Fallback Logic
        var app = settings.Element("Application");
        ThumbnailSize = ValidateThumbnailSize(app?.Element("ThumbnailSize")?.Value ?? settings.Element("ThumbnailSize")?.Value);
        ThumbnailSizeForSystem = ValidateThumbnailSizeForSystem(app?.Element("ThumbnailSizeForSystem")?.Value ?? settings.Element("ThumbnailSizeForSystem")?.Value);
        GamesPerPage = ValidateGamesPerPage(app?.Element("GamesPerPage")?.Value ?? settings.Element("GamesPerPage")?.Value);
        ShowGames = ValidateShowGames(app?.Element("ShowGames")?.Value ?? settings.Element("ShowGames")?.Value);
        ViewMode = ValidateViewMode(app?.Element("ViewMode")?.Value ?? settings.Element("ViewMode")?.Value);

        if (bool.TryParse(app?.Element("EnableGamePadNavigation")?.Value ?? settings.Element("EnableGamePadNavigation")?.Value, out var gp))
        {
            EnableGamePadNavigation = gp;
        }

        VideoUrl = app?.Element("VideoUrl")?.Value ?? settings.Element("VideoUrl")?.Value ?? VideoUrl;
        InfoUrl = app?.Element("InfoUrl")?.Value ?? settings.Element("InfoUrl")?.Value ?? InfoUrl;
        BaseTheme = ValidateBaseTheme(app?.Element("BaseTheme")?.Value ?? settings.Element("BaseTheme")?.Value ?? BaseTheme);
        AccentColor = ValidateAccentColor(app?.Element("AccentColor")?.Value ?? settings.Element("AccentColor")?.Value ?? AccentColor);
        StyleVariant = ValidateStyleVariant(app?.Element("StyleVariant")?.Value ?? settings.Element("StyleVariant")?.Value);
        Language = app?.Element("Language")?.Value ?? settings.Element("Language")?.Value ?? Language;
        ButtonAspectRatio = ValidateButtonAspectRatio(app?.Element("ButtonAspectRatio")?.Value ?? settings.Element("ButtonAspectRatio")?.Value);
        FilenameDisplayMode = ValidateFilenameDisplayMode(app?.Element("FilenameDisplayMode")?.Value ?? settings.Element("FilenameDisplayMode")?.Value);
        if (bool.TryParse(app?.Element("DisplayMachineName")?.Value ?? settings.Element("DisplayMachineName")?.Value, out var dmn))
        {
            DisplayMachineName = dmn;
        }

        FilenameFontSize = ValidateFontSize(app?.Element("FilenameFontSize")?.Value ?? settings.Element("FilenameFontSize")?.Value);
        MachineNameFontSize = ValidateFontSize(app?.Element("MachineNameFontSize")?.Value ?? settings.Element("MachineNameFontSize")?.Value);

        RaUsername = app?.Element("RaUsername")?.Value ?? settings.Element("RaUsername")?.Value ?? settings.Element("RA_Username")?.Value ?? RaUsername;
        RaApiKey = app?.Element("RaApiKey")?.Value ?? settings.Element("RaApiKey")?.Value ?? settings.Element("RA_ApiKey")?.Value ?? RaApiKey;
        RaPassword = DecryptString(app?.Element("RaPassword")?.Value ?? settings.Element("RaPassword")?.Value ?? RaPassword);
        RaToken = DecryptString(app?.Element("RaToken")?.Value ?? settings.Element("RaToken")?.Value ?? RaToken);

        if (float.TryParse(app?.Element("DeadZoneX")?.Value ?? settings.Element("DeadZoneX")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzx))
        {
            DeadZoneX = dzx;
        }

        if (float.TryParse(app?.Element("DeadZoneY")?.Value ?? settings.Element("DeadZoneY")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dzy))
        {
            DeadZoneY = dzy;
        }

        if (bool.TryParse(app?.Element("EnableFuzzyMatching")?.Value ?? settings.Element("EnableFuzzyMatching")?.Value, out var fm))
        {
            EnableFuzzyMatching = fm;
        }

        if (double.TryParse(app?.Element("FuzzyMatchingThreshold")?.Value ?? settings.Element("FuzzyMatchingThreshold")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var fmt))
        {
            FuzzyMatchingThreshold = fmt;
        }

        if (bool.TryParse(app?.Element("EnableNotificationSound")?.Value ?? settings.Element("EnableNotificationSound")?.Value, out var ens))
        {
            EnableNotificationSound = ens;
        }

        CustomNotificationSoundFile = app?.Element("CustomNotificationSoundFile")?.Value ?? settings.Element("CustomNotificationSoundFile")?.Value ?? CustomNotificationSoundFile;
        if (bool.TryParse(app?.Element("OverlayRetroAchievementButton")?.Value ?? settings.Element("OverlayRetroAchievementButton")?.Value, out var ora))
        {
            OverlayRetroAchievementButton = ora;
        }

        if (bool.TryParse(app?.Element("OverlayOpenVideoButton")?.Value ?? settings.Element("OverlayOpenVideoButton")?.Value, out var ovb))
        {
            OverlayOpenVideoButton = ovb;
        }

        if (bool.TryParse(app?.Element("OverlayOpenInfoButton")?.Value ?? settings.Element("OverlayOpenInfoButton")?.Value, out var oib))
        {
            OverlayOpenInfoButton = oib;
        }

        if (bool.TryParse(app?.Element("AdditionalSystemFoldersExpanded")?.Value ?? settings.Element("AdditionalSystemFoldersExpanded")?.Value, out var asfe))
        {
            AdditionalSystemFoldersExpanded = asfe;
        }

        if (bool.TryParse(app?.Element("Emulator1Expanded")?.Value ?? settings.Element("Emulator1Expanded")?.Value, out var e1E))
        {
            Emulator1Expanded = e1E;
        }

        if (bool.TryParse(app?.Element("Emulator2Expanded")?.Value ?? settings.Element("Emulator2Expanded")?.Value, out var e2E))
        {
            Emulator2Expanded = e2E;
        }

        if (bool.TryParse(app?.Element("Emulator3Expanded")?.Value ?? settings.Element("Emulator3Expanded")?.Value, out var e3E))
        {
            Emulator3Expanded = e3E;
        }

        if (bool.TryParse(app?.Element("Emulator4Expanded")?.Value ?? settings.Element("Emulator4Expanded")?.Value, out var e4E))
        {
            Emulator4Expanded = e4E;
        }

        if (bool.TryParse(app?.Element("Emulator5Expanded")?.Value ?? settings.Element("Emulator5Expanded")?.Value, out var e5E))
        {
            Emulator5Expanded = e5E;
        }

        // Delegate emulator settings loading to each emulator's LoadFromXml
        Ares.LoadFromXml(settings);
        Azahar.LoadFromXml(settings);
        Blastem.LoadFromXml(settings);
        Cemu.LoadFromXml(settings);
        Daphne.LoadFromXml(settings);
        Dolphin.LoadFromXml(settings);
        DuckStation.LoadFromXml(settings);
        Flycast.LoadFromXml(settings);
        Mame.LoadFromXml(settings);
        Mednafen.LoadFromXml(settings);
        Mesen.LoadFromXml(settings);
        Pcsx2.LoadFromXml(settings);
        Raine.LoadFromXml(settings);
        Redream.LoadFromXml(settings);
        RetroArch.LoadFromXml(settings);
        Rpcs3.LoadFromXml(settings);
        SegaModel2.LoadFromXml(settings);
        Stella.LoadFromXml(settings);
        Supermodel.LoadFromXml(settings);
        Xenia.LoadFromXml(settings);
        Yumir.LoadFromXml(settings);

        // SystemPlayTimes
        var playTimes = settings.Element("SystemPlayTimes");
        if (playTimes != null)
        {
            SystemPlayTimes.Clear();
            foreach (var pt in playTimes.Elements("SystemPlayTime"))
            {
                var playTimeValue = pt.Element("PlayTime")?.Value ?? "0";
                if (long.TryParse(playTimeValue, out var seconds))
                {
                    // New format: stored as total seconds
                }
                else if (TimeSpan.TryParse(playTimeValue, CultureInfo.InvariantCulture, out var ts))
                {
                    seconds = (long)ts.TotalSeconds;
                }
                else
                {
                    seconds = 0;
                }

                SystemPlayTimes.Add(new SystemPlayTime
                {
                    SystemName = pt.Element("SystemName")?.Value ?? "",
                    PlayTimeSeconds = seconds
                });
            }
        }
    }

    public Task SaveAsync()
    {
        SettingsManager snapshot;
        _settingsLock.EnterReadLock();
        try
        {
            snapshot = new SettingsManager(_configuration, _logErrors, _credentialProtector, _messageBox);
            snapshot.CopyFrom(this);
        }
        finally
        {
            _settingsLock.ExitReadLock();
        }

        return Task.Run(async () =>
        {
            var tempPath = _fileLocation.TempFilePath;
            const int maxRetries = 3;
            var retryDelayMs = 500;
            Exception lastException = null;

            var settingsDirectory = Path.GetDirectoryName(_fileLocation.FilePath);
            if (!string.IsNullOrEmpty(settingsDirectory) && !Directory.Exists(settingsDirectory))
            {
                try
                {
                    Directory.CreateDirectory(settingsDirectory);
                }
                catch (Exception ex)
                {
                    _logErrors.LogAndForget(ex, "Error creating settings directory.");
                }
            }

            var attempt = 0;
            while (attempt < maxRetries)
            {
                try
                {
                    var root = BuildXElement(snapshot);

                    byte[] xmlBytes;
                    using (var ms = new MemoryStream())
                    {
                        root.Save(ms);
                        xmlBytes = ms.ToArray();
                    }

                    if (xmlBytes.Length == 0)
                    {
                        throw new InvalidOperationException("Generated settings XML is empty.");
                    }

                    File.WriteAllBytes(tempPath, xmlBytes);
                    File.Move(tempPath, _fileLocation.FilePath, true);
                    return;
                }
                catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
                {
                    lastException = ex;
                    attempt++;

                    if (IsPortableMode && attempt >= maxRetries)
                    {
                        try
                        {
                            if (_fileLocation.TryFallbackToLocalAppData())
                            {
                                tempPath = _fileLocation.TempFilePath;
                                attempt = 0;
                                continue;
                            }
                        }
                        catch
                        {
                            // Fallback failed
                        }
                    }

                    if (attempt < maxRetries)
                    {
                        try
                        {
                            if (File.Exists(tempPath))
                            {
                                File.Delete(tempPath);
                            }
                        }
                        catch
                        {
                            // Ignore cleanup errors
                        }

                        Thread.Sleep(retryDelayMs);
                        retryDelayMs *= 2;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    break;
                }
            }

            _logErrors.LogAndForget(lastException, "Error saving settings.xml");

            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }

            if (_messageBox != null) await _messageBox.FailedToSaveSettingsMessageBox();
        });
    }

    private static XElement BuildXElement(SettingsManager s)
    {
        return new XElement("Settings",
            // Application Settings
            new XElement("Application",
                new XElement("ThumbnailSize", s.ThumbnailSize),
                new XElement("ThumbnailSizeForSystem", s.ThumbnailSizeForSystem),
                new XElement("GamesPerPage", s.GamesPerPage),
                new XElement("ShowGames", s.ShowGames),
                new XElement("ViewMode", s.ViewMode),
                new XElement("EnableGamePadNavigation", s.EnableGamePadNavigation),
                new XElement("VideoUrl", s.VideoUrl),
                new XElement("InfoUrl", s.InfoUrl),
                new XElement("BaseTheme", s.BaseTheme),
                new XElement("AccentColor", s.AccentColor),
                new XElement("StyleVariant", s.StyleVariant),
                new XElement("Language", s.Language),
                new XElement("DeadZoneX", s.DeadZoneX.ToString(CultureInfo.InvariantCulture)),
                new XElement("DeadZoneY", s.DeadZoneY.ToString(CultureInfo.InvariantCulture)),
                new XElement("ButtonAspectRatio", s.ButtonAspectRatio),
                new XElement("FilenameDisplayMode", s.FilenameDisplayMode),
                new XElement("DisplayMachineName", s.DisplayMachineName),
                new XElement("FilenameFontSize", s.FilenameFontSize),
                new XElement("MachineNameFontSize", s.MachineNameFontSize),
                new XElement("EnableFuzzyMatching", s.EnableFuzzyMatching),
                new XElement("FuzzyMatchingThreshold", s.FuzzyMatchingThreshold.ToString(CultureInfo.InvariantCulture)),
                new XElement("EnableNotificationSound", s.EnableNotificationSound),
                new XElement("CustomNotificationSoundFile", s.CustomNotificationSoundFile),
                new XElement("RaUsername", s.RaUsername),
                new XElement("RaApiKey", s.RaApiKey),
                new XElement("RaPassword", s.EncryptString(s.RaPassword)),
                new XElement("RaToken", s.EncryptString(s.RaToken)),
                new XElement("OverlayRetroAchievementButton", s.OverlayRetroAchievementButton),
                new XElement("OverlayOpenVideoButton", s.OverlayOpenVideoButton),
                new XElement("OverlayOpenInfoButton", s.OverlayOpenInfoButton),
                new XElement("AdditionalSystemFoldersExpanded", s.AdditionalSystemFoldersExpanded),
                new XElement("Emulator1Expanded", s.Emulator1Expanded),
                new XElement("Emulator2Expanded", s.Emulator2Expanded),
                new XElement("Emulator3Expanded", s.Emulator3Expanded),
                new XElement("Emulator4Expanded", s.Emulator4Expanded),
                new XElement("Emulator5Expanded", s.Emulator5Expanded)
            ),

            // Delegate emulator serialization to each emulator's ToXElement
            s.Ares.ToXElement(),
            s.Azahar.ToXElement(),
            s.Blastem.ToXElement(),
            s.Cemu.ToXElement(),
            s.Daphne.ToXElement(),
            s.Dolphin.ToXElement(),
            s.DuckStation.ToXElement(),
            s.Flycast.ToXElement(),
            s.Mame.ToXElement(),
            s.Mednafen.ToXElement(),
            s.Mesen.ToXElement(),
            s.Pcsx2.ToXElement(),
            s.Raine.ToXElement(),
            s.Redream.ToXElement(),
            s.RetroArch.ToXElement(),
            s.Rpcs3.ToXElement(),
            s.SegaModel2.ToXElement(),
            s.Stella.ToXElement(),
            s.Supermodel.ToXElement(),
            s.Xenia.ToXElement(),
            s.Yumir.ToXElement(),

            // SystemPlayTimes
            new XElement("SystemPlayTimes",
                s.SystemPlayTimes.Select(static pt =>
                    new XElement("SystemPlayTime",
                        new XElement("SystemName", pt.SystemName),
                        new XElement("PlayTime", pt.PlayTimeSeconds)
                    )
                )
            )
        );
    }

    private int ValidateThumbnailSize(string value)
    {
        return int.TryParse(value, out var p) && _validThumbnailSizes.Contains(p) ? p : 250;
    }

    private int ValidateThumbnailSizeForSystem(string value)
    {
        return int.TryParse(value, out var p) && _validThumbnailSizesForSystem.Contains(p) ? p : 50;
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

    private string ValidateFilenameDisplayMode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Original";

        return _validFilenameDisplayModes.Contains(value) ? value : "Original";
    }

    private string ValidateFontSize(string value)
    {
        return _validFontSizes.Contains(value) ? value : "Normal";
    }

    private string ValidateStyleVariant(string value)
    {
        return _validStyleVariants.Contains(value) ? value : "Default";
    }

    private string ValidateBaseTheme(string value)
    {
        return _validBaseThemes.Contains(value) ? value : "Dark";
    }

    private string ValidateAccentColor(string value)
    {
        return _validAccentColors.Contains(value) ? value : "Blue";
    }

    public void ResetToDefaults()
    {
        CopyFrom(new SettingsManager(_configuration, _logErrors, _credentialProtector, _messageBox));
    }

    private void SetDefaultsAndSave()
    {
        ResetToDefaults();
        _ = SaveAsync();
    }

    public void UpdateSystemPlayTime(string systemName, TimeSpan playTime)
    {
        if (string.IsNullOrWhiteSpace(systemName) || playTime == TimeSpan.Zero) return;

        _settingsLock.EnterWriteLock();
        try
        {
            var item = SystemPlayTimes.FirstOrDefault(s => s.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                item = new SystemPlayTime { SystemName = systemName, PlayTimeSeconds = 0 };
                SystemPlayTimes.Add(item);
            }

            item.PlayTimeSeconds += (long)playTime.TotalSeconds;
        }
        finally
        {
            _settingsLock.ExitWriteLock();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        _settingsLock?.Dispose();

        GC.SuppressFinalize(this);
    }
}
