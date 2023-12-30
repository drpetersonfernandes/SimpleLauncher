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
        public string MachineName { get; set; }
        public string Description { get; set; }

        public static List<MameConfig> LoadFromXml(string xmlPath)
        {
            try
            {
                if (File.Exists(xmlPath))
                {
                    XDocument xmlDoc = XDocument.Load(xmlPath);
                    return xmlDoc.Descendants("Machine")
                                 .Select(m => new MameConfig
                                 {
                                     MachineName = m.Element("MachineName")?.Value,
                                     Description = m.Element("Description")?.Value
                                 }).ToList();
                }
                else
                {
                    MessageBox.Show("mame.xml not found.", "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return [];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return [];
            }
        }
    }
}
