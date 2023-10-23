using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public class SystemConfig
    {
        public string SystemName { get; set; }
        public string SystemFolder { get; set; }
        public List<string> FileFormatToSearch { get; set; }
        public bool ExtractFileBeforeLaunch { get; set; }
        public List<string> FileFormatToLaunch { get; set; }

        public class Emulator
        {
            public string EmulatorName { get; set; }
            public string EmulatorLocation { get; set; }
            public string EmulatorParameters { get; set; }
        }
        public List<Emulator> Emulators { get; set; } = new List<Emulator>();

        public static List<SystemConfig> LoadSystemConfigs(string filePath)
        {
            List<SystemConfig> configs = new List<SystemConfig>();

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                SystemConfig currentConfig = null;

                foreach (string line in lines)
                {
                    string key = line.Split('=')[0].Trim();
                    string value = line.Contains('=') ? line.Split('=')[1].Trim() : "";

                    // Use switch for better clarity and maintainability
                    switch (key)
                    {
                        case "SystemName":
                            if (currentConfig != null)
                            {
                                configs.Add(currentConfig);
                            }
                            currentConfig = new SystemConfig { SystemName = value };
                            break;
                        case "SystemFolder":
                            currentConfig.SystemFolder = value.Trim('"');
                            break;
                        case "FileFormatToSearch":
                            currentConfig.FileFormatToSearch = value.Split(',').Select(s => s.Trim()).ToList();
                            break;
                        case "ExtractFileBeforeLaunch":
                            currentConfig.ExtractFileBeforeLaunch = value.Equals("yes", StringComparison.OrdinalIgnoreCase);
                            break;
                        case "FileFormatToLaunch":
                            currentConfig.FileFormatToLaunch = value.Split(',').Select(s => s.Trim()).ToList();
                            break;
                        case "EmulatorName":
                            Emulator emulator = new Emulator { EmulatorName = value };
                            currentConfig.Emulators.Add(emulator);
                            break;
                        case "EmulatorLocation":
                            var lastEmulator = currentConfig.Emulators.LastOrDefault();
                            if (lastEmulator != null)
                            {
                                lastEmulator.EmulatorLocation = value.Trim('"');
                            }
                            break;
                        case "EmulatorParameters":
                            var lastEmulatorParams = currentConfig.Emulators.LastOrDefault();
                            if (lastEmulatorParams != null)
                            {
                                lastEmulatorParams.EmulatorParameters = value;
                            }
                            break;
                    }
                }

                // Adding the last configuration to the list
                if (currentConfig != null)
                {
                    configs.Add(currentConfig);
                }
            }

            return configs;
        }

    }
}
