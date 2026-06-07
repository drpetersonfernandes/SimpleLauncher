using System.Globalization;
using System.Xml.Linq;

namespace SimpleLauncher.Core.Services.SettingsManager;

public static class EmulatorXmlHelpers
{
    public static bool ReadBool(XElement section, string sectionName, XElement root, string propertyName, bool fallback)
    {
        var raw = section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value;
        return bool.TryParse(raw, out var val) ? val : fallback;
    }

    public static int ReadInt(XElement section, string sectionName, XElement root, string propertyName, int fallback)
    {
        var raw = section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value;
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) ? val : fallback;
    }

    public static double ReadDouble(XElement section, string sectionName, XElement root, string propertyName, double fallback)
    {
        var raw = section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value;
        return double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : fallback;
    }

    public static string ReadString(XElement section, string sectionName, XElement root, string propertyName, string fallback)
    {
        return section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value ?? fallback;
    }

    public static float ReadFloat(XElement section, string sectionName, XElement root, string propertyName, float fallback)
    {
        var raw = section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value;
        return float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : fallback;
    }
}
