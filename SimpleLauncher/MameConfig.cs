using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SimpleLauncher;

public class MameConfig
{
    public string MachineName { get; private init; }
    public string Description { get; private init; }

    private static readonly string DefaultXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mame.xml");

    public static List<MameConfig> LoadFromXml(string xmlPath = null)
    {
        xmlPath ??= DefaultXmlPath;

        // Check if the mame.xml file exists
        if (!File.Exists(xmlPath))
        {
            // Notify developer
            const string contextMessage = "The file 'mame.xml' could not be found in the application folder.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ReinstallSimpleLauncherFileMissingMessageBox();

            return [];
        }

        try
        {
            var xmlDoc = XDocument.Load(xmlPath);
            return xmlDoc.Descendants("Machine")
                .Select(m => new MameConfig
                {
                    MachineName = m.Element("MachineName")?.Value,
                    Description = m.Element("Description")?.Value
                }).ToList();
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"The file mame.xml could not be loaded or is corrupted.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ReinstallSimpleLauncherFileCorruptedMessageBox();

            return [];
        }
    }
}