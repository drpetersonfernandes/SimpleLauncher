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
            }
            catch
            {
                // If there's an error in loading or parsing, use defaults
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
            ThumbnailSize = 350;
            HideGamesWithNoCover = false;
            EnableGamePadNavigation = true;
            Save();
        }

        public void Save()
        {
            new XElement("Settings",
                new XElement("ThumbnailSize", ThumbnailSize),
                new XElement("HideGamesWithNoCover", HideGamesWithNoCover),
                new XElement("EnableGamePadNavigation", EnableGamePadNavigation)
            ).Save(_filePath);
        }
    }
}