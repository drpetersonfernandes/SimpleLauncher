using System.Globalization;
using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager;

/// <summary>
/// Provides static helper methods for reading typed values from emulator XML configuration sections with fallback defaults.
/// </summary>
public static class EmulatorXmlHelpers
{
    /// <summary>
    /// Reads a boolean value from an XML element, falling back to an alternate location and then a default value.
    /// </summary>
    public static bool ReadBool(XElement section, string sectionName, XElement root, string propertyName, bool fallback)
    {
        var raw = section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value;
        return bool.TryParse(raw, out var val) ? val : fallback;
    }

    /// <summary>
    /// Reads an integer value from an XML element, falling back to an alternate location and then a default value.
    /// </summary>
    public static int ReadInt(XElement section, string sectionName, XElement root, string propertyName, int fallback)
    {
        var raw = section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value;
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var val) ? val : fallback;
    }

    /// <summary>
    /// Reads a double value from an XML element, falling back to an alternate location and then a default value.
    /// </summary>
    public static double ReadDouble(XElement section, string sectionName, XElement root, string propertyName, double fallback)
    {
        var raw = section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value;
        return double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : fallback;
    }

    /// <summary>
    /// Reads a string value from an XML element, falling back to an alternate location and then a default value.
    /// </summary>
    public static string ReadString(XElement section, string sectionName, XElement root, string propertyName, string fallback)
    {
        return section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value ?? fallback;
    }

    /// <summary>
    /// Reads a float value from an XML element, falling back to an alternate location and then a default value.
    /// </summary>
    public static float ReadFloat(XElement section, string sectionName, XElement root, string propertyName, float fallback)
    {
        var raw = section?.Element(propertyName)?.Value ?? root.Element($"{sectionName}{propertyName}")?.Value;
        return float.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : fallback;
    }
}
