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

        public int ThumbnailSize { get; set; }
        public bool HideGamesWithNoCover { get; set; }
        public bool EnableGamePadNavigation { get; set; }
        public string VideoUrl { get; private set; }
        public string InfoUrl { get; private set; }

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

                // Assign boolean settings
                HideGamesWithNoCover = ParseBoolSetting(settings, "HideGamesWithNoCover");
                EnableGamePadNavigation = ParseBoolSetting(settings, "EnableGamePadNavigation");

                // Validate and assign VideoUrl
                string videoUrl = settings.Element("VideoUrl")?.Value;
                VideoUrl = !string.IsNullOrEmpty(videoUrl) ? videoUrl : "https://www.youtube.com/results?search_query=";

                // Validate and assign InfoUrl
                string infoUrl = settings.Element("InfoUrl")?.Value;
                InfoUrl = !string.IsNullOrEmpty(infoUrl) ? infoUrl : "https://www.igdb.com/search?type=1&amp;q=";
               
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
            HideGamesWithNoCover = false;
            EnableGamePadNavigation = false;
            VideoUrl = "https://www.youtube.com/results?search_query=";
            InfoUrl = "https://www.igdb.com/search?type=1&amp;q=";
            Save();
        }

        public void Save()
        {
            new XElement("Settings",
                new XElement("ThumbnailSize", ThumbnailSize),
                new XElement("HideGamesWithNoCover", HideGamesWithNoCover),
                new XElement("EnableGamePadNavigation", EnableGamePadNavigation),
                new XElement("VideoUrl", VideoUrl),
                new XElement("InfoUrl", InfoUrl)
            ).Save(_filePath);
        }
    }
}