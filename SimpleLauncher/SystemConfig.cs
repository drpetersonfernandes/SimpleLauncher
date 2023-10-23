using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SimpleLauncher
{
    public class SystemConfig
    {
        public string SystemName { get; set; }
        public string SystemFolder { get; set; }
        public string[] FileFormatsToSearch { get; set; }
        public bool ExtractFileBeforeLaunch { get; set; }
        public string[] FileFormatsToLaunch { get; set; }

        public class Emulator
        {
            public string EmulatorName { get; set; }
            public string EmulatorLocation { get; set; }
            public string EmulatorParameters { get; set; }
        }

        public List<Emulator> Emulators { get; set; } = new List<Emulator>();

        public static List<SystemConfig> LoadSystemConfigs(string filePath)
        {
            var doc = XDocument.Load(filePath);

            // Fetch all 'SystemConfig' nodes from the XML
            var systemConfigElements = doc.Descendants("SystemConfig");
            var systemConfigs = new List<SystemConfig>();

            foreach (var configElement in systemConfigElements)
            {
                var config = new SystemConfig
                {
                    SystemName = configElement.Element("SystemName")?.Value,
                    SystemFolder = configElement.Element("SystemFolder")?.Value,
                    FileFormatsToSearch = configElement.Descendants("FileFormatsToSearch").Descendants("FormatToSearch").Select(x => x.Value).ToArray(),
                    ExtractFileBeforeLaunch = bool.Parse(configElement.Element("ExtractFileBeforeLaunch")?.Value ?? "false"),
                    FileFormatsToLaunch = configElement.Descendants("FileFormatsToLaunch").Descendants("FormatToLaunch").Select(x => x.Value).ToArray(),
                    Emulators = new List<Emulator>()
                };

                // Extracting all emulator configurations
                var emulatorElements = configElement.Descendants().Where(e => e.Name.LocalName.StartsWith("Emulator") && char.IsDigit(e.Name.LocalName.Last()));
                foreach (var emulatorElement in emulatorElements)
                {
                    var emulator = new Emulator
                    {
                        EmulatorName = emulatorElement.Element("EmulatorName")?.Value,
                        EmulatorLocation = emulatorElement.Element("EmulatorLocation")?.Value,
                        EmulatorParameters = emulatorElement.Element("EmulatorParameters")?.Value
                    };
                    config.Emulators.Add(emulator);
                }

                systemConfigs.Add(config);
            }

            return systemConfigs;
        }


    }
}
