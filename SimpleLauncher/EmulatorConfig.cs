using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace SimpleLauncher
{
    public class EmulatorConfig
    {
        public string SystemName { get; set; }
        public string EmulatorName { get; set; }
        public string EmulatorLocation { get; set; }
        public string EmulatorParameters { get; set; }

        public override string ToString()
        {
            return EmulatorName; // Display the program name in the ComboBox
        }
    }

    public class EmulatorConfigLoader
    {
        public static List<EmulatorConfig> Load(string filePath)
        {
            List<EmulatorConfig> EmulatorConfigs = new List<EmulatorConfig>();
            EmulatorConfig currentEmulatorConfig = null;

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ": " }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        if (key == "System")
                        {
                            if (currentEmulatorConfig != null)  // If there's already an existing entry, add it to the list
                            {
                                EmulatorConfigs.Add(currentEmulatorConfig);
                            }

                            currentEmulatorConfig = new EmulatorConfig { SystemName = value };
                        }
                        else if (key == "EmulatorName")
                        {
                            currentEmulatorConfig.EmulatorName = value;
                        }
                        else if (key == "EmulatorLocation")
                        {
                            currentEmulatorConfig.EmulatorLocation = value;
                        }
                        else if (key == "EmulatorParameters")
                        {
                            currentEmulatorConfig.EmulatorParameters = value;
                        }
                    }
                }

                // Add the last emulator entry if it's not null
                if (currentEmulatorConfig != null)
                {
                    EmulatorConfigs.Add(currentEmulatorConfig);
                }
            }
            else
            {
                MessageBox.Show("emulator.ini not found");
            }

            return EmulatorConfigs;
        }
    }
}
