using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace SimpleLauncher;

public class HelpUserConfig
{
    private const string FilePath = "helpuser.xml";

    public List<SystemHelper> Systems { get; private set; } = new();

    public void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                MessageBox.Show($"The file 'helpuser.xml' is missing.\n\n" +
                                $"Please reinstall 'Simple Launcher.'",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            XDocument doc;

            try
            {
                doc = XDocument.Load(FilePath);
            }
            catch (Exception)
            {
                MessageBox.Show($"Unable to load 'helpuser.xml'. The file may be corrupted.\n\n" +
                                $"Please reinstall 'Simple Launcher.'",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                return;
            }

            Systems = doc.Descendants("System")
                .Select(system =>
                {
                    try
                    {
                        return new SystemHelper
                        {
                            SystemName = (string)system.Element("SystemName"),
                            SystemHelperText = NormalizeText((string)system.Element("SystemHelper"))
                        };
                    }
                    catch (Exception)
                    {
                        MessageBox.Show($"Warning: Failed to parse the file 'helpuser.xml'.\n\n" +
                                        $"Please reinstall 'Simple Launcher.'",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        
                        return null; // Ignore invalid system entries
                    }
                })
                .Where(helper => helper != null) // Filter out invalid entries
                .ToList();

            if (!Systems.Any())
            {
                MessageBox.Show($"Warning: No valid systems found in the file 'helpuser.xml'.\n\n" +
                                $"Please reinstall 'Simple Launcher.'",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception)
        {
            MessageBox.Show($"Unexpected error while loading 'helpuser.xml'.\n\n" +
                            $"Please reinstall 'Simple Launcher.'",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Process each line to remove leading spaces while keeping line breaks
        var lines = text.Split(['\r', '\n'], StringSplitOptions.None); // Preserve empty lines
        return string.Join(Environment.NewLine, lines.Select(line => line.TrimStart()));
    }
}

public class SystemHelper
{
    public string SystemName { get; init; }
    public string SystemHelperText { get; init; }
}