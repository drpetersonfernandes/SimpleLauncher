using System;
using System.Collections.Generic;
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
                XDocument xmlDoc = XDocument.Load(xmlPath);
                return xmlDoc.Descendants("Machine")
                    .Select(m => new MameConfig
                    {
                        MachineName = m.Element("MachineName")?.Value,
                        Description = m.Element("Description")?.Value
                    }).ToList();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error loading MAME configurations from XML.\n\nException detail: {ex}";
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                await LogErrors.LogErrorAsync(ex, $"{errorMessage}\n\nException details: {ex}");
                return [];
            }
        }
    }
}