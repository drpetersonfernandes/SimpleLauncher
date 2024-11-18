using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleLauncher
{
    public class SystemPlayTime
    {
        public string SystemName { get; set; }
        public string PlayTime { get; set; }
    }

    public class SettingsConfig
    {
        private readonly string _filePath;
        private readonly HashSet<int> _validThumbnailSizes = [100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600];
        private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500];
        private readonly HashSet<string> _validShowGames = ["ShowAll", "ShowWithCover", "ShowWithoutCover"];
        private readonly HashSet<string> _validViewModes =
        [
            "GridView", "ListView"
        ];

        public int ThumbnailSize { get; set; }
        public int GamesPerPage { get; set; }
        public string ShowGames { get; set; }
        public string ViewMode { get; set; }  // New ViewMode property
        public bool EnableGamePadNavigation { get; set; }
        public string VideoUrl { get; set; }
        public string InfoUrl { get; set; }
        public double MainWindowWidth { get; set; }
        public double MainWindowHeight { get; set; }
        public double MainWindowTop { get; set; }
        public double MainWindowLeft { get; set; }
        public string MainWindowState { get; set; }
        public string BaseTheme { get; set; }
        public string AccentColor { get; set; }

        // List to hold multiple SystemPlayTime instances
        public List<SystemPlayTime> SystemPlayTimes { get; set; }

        private const string DefaultSettingsFilePath = "settings.xml";

        public SettingsConfig() : this(DefaultSettingsFilePath) { }

        private SettingsConfig(string filePath)
        {
            _filePath = filePath;
            SystemPlayTimes = new List<SystemPlayTime>();
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
                XElement settings = XElement.Load(_filePath);

                ThumbnailSize = ValidateThumbnailSize(settings.Element("ThumbnailSize")?.Value);
                GamesPerPage = ValidateGamesPerPage(settings.Element("GamesPerPage")?.Value);
                ShowGames = ValidateShowGames(settings.Element("ShowGames")?.Value);
                ViewMode = ValidateViewMode(settings.Element("ViewMode")?.Value);
                EnableGamePadNavigation = ParseBoolSetting(settings, "EnableGamePadNavigation");
                VideoUrl = settings.Element("VideoUrl")?.Value ?? "https://www.youtube.com/results?search_query=";
                InfoUrl = settings.Element("InfoUrl")?.Value ?? "https://www.igdb.com/search?q=";
                MainWindowWidth = ValidateDimension(settings.Element("MainWindowWidth")?.Value, 900);
                MainWindowHeight = ValidateDimension(settings.Element("MainWindowHeight")?.Value, 500);
                MainWindowTop = ValidateDimension(settings.Element("MainWindowTop")?.Value, 0);
                MainWindowLeft = ValidateDimension(settings.Element("MainWindowLeft")?.Value, 0);
                MainWindowState = settings.Element("MainWindowState")?.Value ?? "Normal";
                BaseTheme = settings.Element("BaseTheme")?.Value ?? "Light";
                AccentColor = settings.Element("AccentColor")?.Value ?? "Blue";

                // Load multiple SystemPlayTime elements
                XElement systemPlayTimesElement = settings.Element("SystemPlayTimes");
                if (systemPlayTimesElement != null)
                {
                    foreach (XElement systemPlayTimeElement in systemPlayTimesElement.Elements("SystemPlayTime"))
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

                string contextMessage = $"Error loading or parsing 'setting.xml' from SettingsConfig class.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));

                MessageBox.Show(@"Simple Launcher does not have enough privileges to write to the file 'settings.xml'.\n\nPlease grant the application more privileges, or it won't work properly.\n\nTry running it with administrative privileges.",
                    @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int ValidateThumbnailSize(string value)
        {
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int parsed) && _validThumbnailSizes.Contains(parsed))
            {
                return parsed;
            }
            return 200;
        }

        private int ValidateGamesPerPage(string value)
        {
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int parsed) && _validGamesPerPage.Contains(parsed))
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

        private double ValidateDimension(string value, double defaultValue)
        {
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                return parsed;
            }
            return defaultValue;
        }

        private static bool ParseBoolSetting(XElement settings, string settingName)
        {
            if (bool.TryParse(settings.Element(settingName)?.Value, out bool value))
            {
                return value;
            }
            return false;
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
            SystemPlayTimes = new List<SystemPlayTime>();
            Save();
        }

        public void Save()
        {
            XElement systemPlayTimesElement = new XElement("SystemPlayTimes");
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
                systemPlayTimesElement
            ).Save(_filePath);
        }
        
        public void UpdateSystemPlayTime(string systemName, TimeSpan playTime)
        {
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
            TimeSpan existingPlayTime = TimeSpan.Parse(systemPlayTime.PlayTime);
            TimeSpan updatedPlayTime = existingPlayTime + playTime;

            // Update the playtime in the correct format
            systemPlayTime.PlayTime = updatedPlayTime.ToString(@"hh\:mm\:ss");
        }
    }
}