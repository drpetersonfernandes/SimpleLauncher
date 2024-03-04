using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using System.IO;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public class SystemConfig
    {
        public string SystemName { get; private init; }
        public string SystemFolder { get; private init; }
        public bool SystemIsMAME { get; private init; }
        public List<string> FileFormatsToSearch { get; private init; }
        public bool ExtractFileBeforeLaunch { get; private init; }
        public List<string> FileFormatsToLaunch { get; private init; }
        public List<Emulator> Emulators { get; private init; }

        public class Emulator
        {
            public string EmulatorName { get; init; }
            public string EmulatorLocation { get; init; }
            public string EmulatorParameters { get; init; }
        }

        public static List<SystemConfig> LoadSystemConfigs(string xmlPath)
        {
            try
            {
                if (!File.Exists(xmlPath))
                    throw new FileNotFoundException($"The file {xmlPath} was not found.");

                var doc = XDocument.Load(xmlPath);
                var systemConfigs = new List<SystemConfig>();

                foreach (var sysConfigElement in doc.Root!.Elements("SystemConfig"))
                {
                    if (sysConfigElement.Element("SystemName") == null || string.IsNullOrEmpty(sysConfigElement.Element("SystemName")!.Value))
                        throw new InvalidOperationException("Missing or empty SystemName in XML.");

                    if (sysConfigElement.Element("SystemFolder") == null || string.IsNullOrEmpty(sysConfigElement.Element("SystemFolder")!.Value))
                        throw new InvalidOperationException("Missing or empty SystemFolder in XML.");

                    if (!bool.TryParse(sysConfigElement.Element("SystemIsMAME")?.Value, out bool systemIsMAME))
                        throw new InvalidOperationException("Invalid or missing value for SystemIsMAME.");

                    var formatsToSearch = sysConfigElement.Element("FileFormatsToSearch")?.Elements("FormatToSearch").Select(e => e.Value).ToList();
                    if (formatsToSearch == null || formatsToSearch.Count == 0)
                        throw new InvalidOperationException("FileFormatsToSearch should have at least one value.");

                    if (!bool.TryParse(sysConfigElement.Element("ExtractFileBeforeLaunch")?.Value, out bool extractFileBeforeLaunch))
                        throw new InvalidOperationException("Invalid or missing value for ExtractFileBeforeLaunch.");

                    var formatsToLaunch = sysConfigElement.Element("FileFormatsToLaunch")?.Elements("FormatToLaunch").Select(e => e.Value).ToList();
                    if (extractFileBeforeLaunch && (formatsToLaunch == null || formatsToLaunch.Count == 0))
                        throw new InvalidOperationException("FileFormatsToLaunch should have at least one value when ExtractFileBeforeLaunch is true.");

                    var emulators = sysConfigElement.Element("Emulators")?.Elements("Emulator").Select(emulatorElement =>
                    {
                        if (string.IsNullOrEmpty(emulatorElement.Element("EmulatorName")?.Value))
                            throw new InvalidOperationException("EmulatorName should not be empty or null.");

                        return new Emulator
                        {
                            EmulatorName = emulatorElement.Element("EmulatorName")!.Value,
                            EmulatorLocation = emulatorElement.Element("EmulatorLocation")!.Value,
                            EmulatorParameters = emulatorElement.Element("EmulatorParameters")?.Value // It's okay if this is null or empty
                        };
                    }).ToList();

                    if (emulators == null || emulators.Count == 0)
                        throw new InvalidOperationException("Emulators list should not be empty or null.");

                    systemConfigs.Add(new SystemConfig
                    {
                        SystemName = sysConfigElement.Element("SystemName")?.Value,
                        SystemFolder = sysConfigElement.Element("SystemFolder")?.Value,
                        SystemIsMAME = systemIsMAME,
                        ExtractFileBeforeLaunch = extractFileBeforeLaunch,
                        FileFormatsToSearch = formatsToSearch,
                        FileFormatsToLaunch = formatsToLaunch,
                        Emulators = emulators
                    });
                }

                return systemConfigs;
            }
            catch (Exception ex)
            {
                string contextMessage = $"Error loading system configurations from XML: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);

                MessageBox.Show($"The system.xml is broken: {ex.Message}\n\nPlease fix system.xml and try to run the program again", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Wait for up to 2 seconds for the logTask to complete
                logTask.Wait(TimeSpan.FromSeconds(2));

                return null;
            }

        }
    }
}