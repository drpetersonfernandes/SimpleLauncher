using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public class SettingsConfig
    {
        private readonly string _filePath;
        private readonly HashSet<int> _validThumbnailSizes = new HashSet<int> { 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600 };
        private readonly HashSet<int> _validGamesPerPage = new HashSet<int> { 100, 200, 300, 400, 500 };
        private readonly HashSet<string> _validShowGames = new HashSet<string> { "ShowAll", "ShowWithCover", "ShowWithoutCover" };

        public int ThumbnailSize { get; set; }
        public int GamesPerPage { get; set; }
        public string ShowGames { get; set; }
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

        private const string DefaultSettingsFilePath = "settings.xml";

        public SettingsConfig() : this(DefaultSettingsFilePath) { }

        private SettingsConfig(string filePath)
        {
            _filePath = filePath;
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

                // Ensure all values are saved if they were missing
                Save();
            }
            catch (Exception exception)
            {
                SetDefaultsAndSave();
                Task logTask = LogErrors.LogErrorAsync(exception, "Error in loading or parsing setting.xml.");
                logTask.Wait(TimeSpan.FromSeconds(2));
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
            Save();
        }

        public void Save()
        {
            new XElement("Settings",
                new XElement("ThumbnailSize", ThumbnailSize),
                new XElement("GamesPerPage", GamesPerPage),
                new XElement("ShowGames", ShowGames),
                new XElement("EnableGamePadNavigation", EnableGamePadNavigation),
                new XElement("VideoUrl", VideoUrl),
                new XElement("InfoUrl", InfoUrl),
                new XElement("MainWindowWidth", MainWindowWidth),
                new XElement("MainWindowHeight", MainWindowHeight),
                new XElement("MainWindowTop", MainWindowTop),
                new XElement("MainWindowLeft", MainWindowLeft),
                new XElement("MainWindowState", MainWindowState),
                new XElement("BaseTheme", BaseTheme),
                new XElement("AccentColor", AccentColor)
            ).Save(_filePath);
        }
    }
}
