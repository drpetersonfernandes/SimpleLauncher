using System.Globalization;
using System.IO;
using System.Xml.Linq;

namespace SimpleLauncher
{
    public class AppSettings
    {
        private readonly string _filePath;

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
                // If the file doesn't exist, use defaults
                ThumbnailSize = 350;
                HideGamesWithNoCover = false;
                EnableGamePadNavigation = true;
                Save(); // Save the defaults to a new file
                return;
            }

            XElement settings = XElement.Load(_filePath);
            ThumbnailSize = int.Parse(settings.Element("ThumbnailSize").Value, CultureInfo.InvariantCulture);
            HideGamesWithNoCover = bool.Parse(settings.Element("HideGamesWithNoCover").Value);
            EnableGamePadNavigation = bool.Parse(settings.Element("EnableGamePadNavigation").Value);
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
