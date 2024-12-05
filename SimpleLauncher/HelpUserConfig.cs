using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SimpleLauncher;

public class HelpUserConfig
{
    private const string FilePath = "helpuser.xml";

    public List<SystemHelper> Systems { get; private set; } = new();

    public void Load()
    {
        if (!File.Exists(FilePath))
            throw new FileNotFoundException($"The file {FilePath} does not exist.");

        XDocument doc = XDocument.Load(FilePath);

        Systems = doc.Descendants("System")
            .Select(system => new SystemHelper
            {
                SystemName = (string)system.Element("SystemName"),
                SystemHelperText = NormalizeText((string)system.Element("SystemHelper"))
            }).ToList();
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