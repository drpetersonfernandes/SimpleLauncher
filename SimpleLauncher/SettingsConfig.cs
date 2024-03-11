using System;
using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SimpleLauncher
{
    public class AppSettings
    {
        private readonly string _filePath;
        private readonly HashSet<int> _validThumbnailSizes = [100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600];
        private readonly HashSet<int> _validGamesPerPage = [100, 200, 300, 400, 500, 600, 700, 800, 900, 1000];

        public int ThumbnailSize { get; set; }
        public int GamesPerPage { get; set; }
        public bool HideGamesWithNoCover { get; set; }
        public bool EnableGamePadNavigation { get; set; }
        public string VideoUrl { get; private set; }
        public string InfoUrl { get; private set; }
        public double MainWindowWidth { get; set; }
        public double MainWindowHeight { get; set; }
        public string MainWindowState { get; set; }

        public AppSettings(string filePath)
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

                // Validate and assign ThumbnailSize
                int thumbnailSize = int.Parse(settings.Element("ThumbnailSize")!.Value, CultureInfo.InvariantCulture);
                ThumbnailSize = _validThumbnailSizes.Contains(thumbnailSize) ? thumbnailSize : 350;
                
                // Validate and assign GamesPerPage
                int gamesPerPage = int.Parse(settings.Element("GamesPerPage")!.Value, CultureInfo.InvariantCulture);
                GamesPerPage = _validGamesPerPage.Contains(gamesPerPage) ? gamesPerPage : 200;

                // Assign boolean settings
                HideGamesWithNoCover = ParseBoolSetting(settings, "HideGamesWithNoCover");
                EnableGamePadNavigation = ParseBoolSetting(settings, "EnableGamePadNavigation");

                // Validate and assign VideoUrl
                string videoUrl = settings.Element("VideoUrl")?.Value;
                VideoUrl = !string.IsNullOrEmpty(videoUrl) ? videoUrl : "https://www.youtube.com/results?search_query=";

                // Validate and assign InfoUrl
                string infoUrl = settings.Element("InfoUrl")?.Value;
                InfoUrl = !string.IsNullOrEmpty(infoUrl) ? infoUrl : "https://www.igdb.com/search?q=";
                
                // Validate and assign MainWindowWidth
                string mainWindowWidthValue = settings.Element("MainWindowWidth")?.Value;
                bool parseSuccess = double.TryParse(mainWindowWidthValue, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var mainWindowWidth);
                if (!parseSuccess || mainWindowWidth < 890)
                {
                    mainWindowWidth = 890;
                }
                MainWindowWidth = mainWindowWidth;
                
                // Validate and assign MainWindowHeight
                string mainWindowHeightValue = settings.Element("MainWindowHeight")?.Value;
                bool parseSuccess2 = double.TryParse(mainWindowHeightValue, NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var mainWindowHeight);
                if (!parseSuccess2 || mainWindowHeight < 300)
                {
                    mainWindowHeight = 300;
                }
                MainWindowHeight = mainWindowHeight;
                
                // Validate and assign MainWindowState
                string mainWindowState = settings.Element("MainWindowState")?.Value;
                MainWindowState = !string.IsNullOrEmpty(mainWindowState) ? mainWindowState : "Normal";
               
            }
            catch (Exception ex)
            {
                // Handle error in loading or parsing setting.xml
                MainWindow.HandleError(ex, "Error in loading or parsing setting.xml");
                // Use defaults values in case of errors
                SetDefaultsAndSave();
            }
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
            ThumbnailSize = 250;
            GamesPerPage = 200;
            HideGamesWithNoCover = false;
            EnableGamePadNavigation = false;
            VideoUrl = "https://www.youtube.com/results?search_query=";
            InfoUrl = "https://www.igdb.com/search?type=1&amp;q=";
            MainWindowWidth = 890;
            MainWindowHeight = 500;
            MainWindowState = "Normal";
            Save();
        }

        public void Save()
        {
            new XElement("Settings",
                new XElement("ThumbnailSize", ThumbnailSize),
                new XElement("GamesPerPage", GamesPerPage),
                new XElement("HideGamesWithNoCover", HideGamesWithNoCover),
                new XElement("EnableGamePadNavigation", EnableGamePadNavigation),
                new XElement("VideoUrl", VideoUrl),
                new XElement("InfoUrl", InfoUrl),
                new XElement("MainWindowWidth", MainWindowWidth),
                new XElement("MainWindowHeight", MainWindowHeight),
                new XElement("MainWindowState", MainWindowState)
            ).Save(_filePath);
        }
    }
}