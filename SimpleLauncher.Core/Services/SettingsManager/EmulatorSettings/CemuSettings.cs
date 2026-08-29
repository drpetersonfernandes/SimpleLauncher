using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
///     Represents the user-configurable settings for the Cemu emulator, persisted to the system configuration under the
///     "Cemu" section.
/// </summary>
public class CemuSettings : IEmulatorSettings
{
    private const string SectionName = "Cemu";

    /// <summary>
    ///     Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; }

    /// <summary>
    ///     Gets or sets the graphics API used by the emulator (e.g., 1 for Vulkan).
    /// </summary>
    public int GraphicApi { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the vertical synchronization mode (e.g., 1 for on).
    /// </summary>
    public int Vsync { get; set; } = 1;

    /// <summary>
    ///     Gets or sets a value indicating whether shaders are compiled asynchronously to reduce stuttering.
    /// </summary>
    public bool AsyncCompile { get; set; } = true;

    /// <summary>
    ///     Gets or sets the TV audio volume percentage.
    /// </summary>
    public int TvVolume { get; set; } = 50;

    /// <summary>
    ///     Gets or sets the console system language used by the emulator (e.g., 1 for English).
    /// </summary>
    public int ConsoleLanguage { get; set; } = 1;

    /// <summary>
    ///     Gets or sets a value indicating whether Discord rich presence is enabled.
    /// </summary>
    public bool DiscordPresence { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    ///     Loads the Cemu settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), false);
        GraphicApi = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(GraphicApi), 1);
        Vsync = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Vsync), 1);
        AsyncCompile = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AsyncCompile), true);
        TvVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(TvVolume), 50);
        ConsoleLanguage = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ConsoleLanguage), 1);
        DiscordPresence = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(DiscordPresence), true);
        ShowSettingsBeforeLaunch =
            EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>
    ///     Serializes the Cemu settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Cemu settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Fullscreen", Fullscreen),
            new XElement("GraphicApi", GraphicApi),
            new XElement("Vsync", Vsync),
            new XElement("AsyncCompile", AsyncCompile),
            new XElement("TvVolume", TvVolume),
            new XElement("ConsoleLanguage", ConsoleLanguage),
            new XElement("DiscordPresence", DiscordPresence),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }


    /// <summary>
    ///     Copies the values from another emulator settings instance if it is a Cemu settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not CemuSettings src) return;

        Fullscreen = src.Fullscreen;
        GraphicApi = src.GraphicApi;
        Vsync = src.Vsync;
        AsyncCompile = src.AsyncCompile;
        TvVolume = src.TvVolume;
        ConsoleLanguage = src.ConsoleLanguage;
        DiscordPresence = src.DiscordPresence;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }


    /// <summary>
    ///     Resets all Cemu settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new CemuSettings());
    }
}