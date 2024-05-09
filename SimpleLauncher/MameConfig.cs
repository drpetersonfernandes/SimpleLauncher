using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace SimpleLauncher
{
    public class MameConfig
    {
        public string MachineName { get; private init; }
        public string Description { get; private init; }

        public static List<MameConfig> LoadFromXml(string xmlPath)
        {
            // Check if the mame.xml file exists
            if (!File.Exists(xmlPath))
            {
                MessageBox.Show("The file mame.xml could not be found.\n\nThe application will be Shutdown.\n\nPlease reinstall Simple Launcher to restore this file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Shutdown the application and exit
                Application.Current.Shutdown();
                Environment.Exit(0);

                return new List<MameConfig>();
            }

            try
            {
                XDocument xmlDoc = XDocument.Load(xmlPath);
                return xmlDoc.Descendants("Machine")
                    .Select(m => new MameConfig
                    {
                        MachineName = m.Element("MachineName")?.Value,
                        Description = m.Element("Description")?.Value
                    }).ToList();
            }
            catch (Exception)
            {
                MessageBox.Show("I could not load the file mame.xml or it is corrupted.\n\nThe application will be Shutdown.\n\nPlease reinstall Simple Launcher to restore this file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Shutdown current application instance
                Application.Current.Shutdown();
                Environment.Exit(0);

                return new List<MameConfig>();
            }
        }
    }
}