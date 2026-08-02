using System.Xml.Linq;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.SettingsManager.EmulatorSettings;

/// <summary>
/// Represents the user-configurable settings for the Azahar emulator, persisted to the system configuration under the "Azahar" section.
/// </summary>
public class AzaharSettings : IEmulatorSettings
{
    private const string SectionName = "Azahar";

    /// <summary>
    /// Gets or sets the graphics API used by the emulator (e.g., 1 for OpenGL).
    /// </summary>
    public int GraphicsApi { get; set; } = 1;

    /// <summary>
    /// Gets or sets the internal resolution scaling factor applied during emulation.
    /// </summary>
    public int ResolutionFactor { get; set; } = 1;

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool UseVsync { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether shaders are compiled asynchronously to reduce stuttering.
    /// </summary>
    public bool AsyncShaderCompilation { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool Fullscreen { get; set; } = true;

    /// <summary>
    /// Gets or sets the audio volume percentage.
    /// </summary>
    public int Volume { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator uses the New 3DS hardware profile.
    /// </summary>
    public bool IsNew3Ds { get; set; } = true;

    /// <summary>
    /// Gets or sets the screen layout option used by the emulator (e.g., 0 for default).
    /// </summary>
    public int LayoutOption { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether audio stretching is enabled.
    /// </summary>
    public bool EnableAudioStretching { get; set; } = true;

    /// <summary>
    /// Loads the Azahar settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        GraphicsApi = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(GraphicsApi), 1);
        ResolutionFactor = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResolutionFactor), 1);
        UseVsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(UseVsync), true);
        AsyncShaderCompilation = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AsyncShaderCompilation), true);
        Fullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Fullscreen), true);
        Volume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(Volume), 100);
        IsNew3Ds = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, "IsNew3ds", true);
        LayoutOption = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(LayoutOption), 0);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
        EnableAudioStretching = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(EnableAudioStretching), true);
    }


    /// <summary>
    /// Serializes the Azahar settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the Azahar settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("GraphicsApi", GraphicsApi),
            new XElement("ResolutionFactor", ResolutionFactor),
            new XElement("UseVsync", UseVsync),
            new XElement("AsyncShaderCompilation", AsyncShaderCompilation),
            new XElement("Fullscreen", Fullscreen),
            new XElement("Volume", Volume),
            new XElement("IsNew3ds", IsNew3Ds),
            new XElement("LayoutOption", LayoutOption),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch),
            new XElement("EnableAudioStretching", EnableAudioStretching));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is an Azahar settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not AzaharSettings src) return;

        GraphicsApi = src.GraphicsApi;
        ResolutionFactor = src.ResolutionFactor;
        UseVsync = src.UseVsync;
        AsyncShaderCompilation = src.AsyncShaderCompilation;
        Fullscreen = src.Fullscreen;
        Volume = src.Volume;
        IsNew3Ds = src.IsNew3Ds;
        LayoutOption = src.LayoutOption;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
        EnableAudioStretching = src.EnableAudioStretching;
    }


    /// <summary>
    /// Resets all Azahar settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new AzaharSettings());
    }
}
