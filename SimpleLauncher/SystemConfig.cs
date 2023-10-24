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
        public List<string> FileFormatsToSearch { get; set; }
        public bool ExtractFileBeforeLaunch { get; set; }
        public List<string> FileFormatsToLaunch { get; set; }
        public List<Emulator> Emulators { get; set; }

        public class Emulator
        {
            public string EmulatorName { get; set; }
            public string EmulatorLocation { get; set; }
            public string EmulatorParameters { get; set; }
        }

        public static List<SystemConfig> LoadSystemConfigs(string xmlPath)
        {
            var doc = XDocument.Load(xmlPath);
            var systemConfigs = new List<SystemConfig>();

            foreach (var sysConfigElement in doc.Root.Elements("SystemConfig"))
            {
                var systemConfig = new SystemConfig
                {
                    SystemName = sysConfigElement.Element("SystemName").Value,
                    SystemFolder = sysConfigElement.Element("SystemFolder").Value,
                    ExtractFileBeforeLaunch = bool.Parse(sysConfigElement.Element("ExtractFileBeforeLaunch").Value),
                    FileFormatsToSearch = sysConfigElement.Element("FileFormatsToSearch").Elements("FormatToSearch").Select(e => e.Value).ToList(),
                    FileFormatsToLaunch = sysConfigElement.Element("FileFormatsToLaunch").Elements("FormatToLaunch").Select(e => e.Value).ToList(),
                    Emulators = sysConfigElement.Element("Emulators").Elements("Emulator").Select(emulatorElement => new Emulator
                    {
                        EmulatorName = emulatorElement.Element("EmulatorName").Value,
                        EmulatorLocation = emulatorElement.Element("EmulatorLocation").Value,
                        EmulatorParameters = emulatorElement.Element("EmulatorParameters").Value
                    }).ToList()
                };

                systemConfigs.Add(systemConfig);
            }

            return systemConfigs;
        }
    }
}
