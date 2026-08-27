using System.Xml.Linq;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Defines methods for loading, saving, copying, and resetting emulator settings from XML.
/// </summary>
public interface IEmulatorSettings
{
    /// <summary>
    /// Loads emulator settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the settings data.</param>
    void LoadFromXml(XElement settings);

    /// <summary>
    /// Serializes the current emulator settings to an XML element.
    /// </summary>
    /// <returns>An XML element representing the current settings.</returns>
    XElement ToXElement();

    /// <summary>
    /// Copies the settings from another emulator settings instance into this instance.
    /// </summary>
    /// <param name="other">The source settings to copy from.</param>
    void CopyFrom(IEmulatorSettings other);

    /// <summary>
    /// Resets all emulator settings to their default values.
    /// </summary>
    void ResetDefaults();
}