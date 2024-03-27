using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace SimpleLauncher
{
    public class MameConfig
    {
        public string MachineName { get; private init; }
        public string Description { get; private init; }

        public static async Task<List<MameConfig>> LoadFromXml(string xmlPath)        {
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
                    string errorMessage = $"mame.xml not found.\n\nUnable to load MAME configurations.\n";
                    Exception exception = new FileNotFoundException(errorMessage);
                    await LogErrors.LogErrorAsync(exception, errorMessage);
                    MessageBox.Show(errorMessage, "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return [];
                }
            }
            catch (Exception ex)
            {
                string errorMessage = "Error loading MAME configurations from XML.";
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await LogErrors.LogErrorAsync(ex, $"{errorMessage}\n\nException details: {ex}");
                return [];
            }
        }
    }
}
