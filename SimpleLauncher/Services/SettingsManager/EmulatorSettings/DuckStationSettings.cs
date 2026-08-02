using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

/// <summary>
/// Represents the user-configurable settings for the DuckStation emulator, persisted to the system configuration under the "DuckStation" section.
/// </summary>
public class DuckStationSettings : IEmulatorSettings
{
    private const string SectionName = "DuckStation";

    /// <summary>
    /// Gets or sets a value indicating whether the emulator starts in fullscreen mode.
    /// </summary>
    public bool StartFullscreen { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator pauses when the window loses focus.
    /// </summary>
    public bool PauseOnFocusLoss { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the emulator saves state when exiting a game.
    /// </summary>
    public bool SaveStateOnExit { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether rewind support is enabled.
    /// </summary>
    public bool RewindEnable { get; set; }

    /// <summary>
    /// Gets or sets the number of run-ahead frames used to reduce input latency.
    /// </summary>
    public int RunaheadFrameCount { get; set; }

    /// <summary>
    /// Gets or sets the graphics renderer used by the emulator (e.g., "Automatic").
    /// </summary>
    public string Renderer { get; set; } = "Automatic";

    /// <summary>
    /// Gets or sets the internal resolution scaling factor applied during emulation.
    /// </summary>
    public int ResolutionScale { get; set; } = 2;

    /// <summary>
    /// Gets or sets the texture filtering mode (e.g., "Nearest").
    /// </summary>
    public string TextureFilter { get; set; } = "Nearest";

    /// <summary>
    /// Gets or sets a value indicating whether the widescreen rendering hack is enabled.
    /// </summary>
    public bool WidescreenHack { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether PGXP (geometry precision) is enabled.
    /// </summary>
    public bool PgxpEnable { get; set; }

    /// <summary>
    /// Gets or sets the aspect ratio of the emulation window (e.g., "16:9").
    /// </summary>
    public string AspectRatio { get; set; } = "16:9";

    /// <summary>
    /// Gets or sets a value indicating whether vertical synchronization is enabled.
    /// </summary>
    public bool Vsync { get; set; }

    /// <summary>
    /// Gets or sets the audio output volume percentage.
    /// </summary>
    public int OutputVolume { get; set; } = 100;

    /// <summary>
    /// Gets or sets a value indicating whether the audio output is muted.
    /// </summary>
    public bool OutputMuted { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the emulator settings window is shown before launching a game.
    /// </summary>
    public bool ShowSettingsBeforeLaunch { get; set; }


    /// <summary>
    /// Loads the DuckStation settings from the specified XML element.
    /// </summary>
    /// <param name="settings">The XML element containing the system configuration.</param>
    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        StartFullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(StartFullscreen), false);
        PauseOnFocusLoss = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(PauseOnFocusLoss), true);
        SaveStateOnExit = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(SaveStateOnExit), true);
        RewindEnable = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(RewindEnable), false);
        RunaheadFrameCount = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(RunaheadFrameCount), 0);
        Renderer = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Renderer), "Automatic");
        ResolutionScale = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResolutionScale), 2);
        TextureFilter = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(TextureFilter), "Nearest");
        WidescreenHack = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(WidescreenHack), false);
        PgxpEnable = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(PgxpEnable), false);
        AspectRatio = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectRatio), "16:9");
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), false);
        OutputVolume = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(OutputVolume), 100);
        OutputMuted = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(OutputMuted), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }


    /// <summary>
    /// Serializes the DuckStation settings into an XML element for persistence.
    /// </summary>
    /// <returns>The XML element containing the DuckStation settings.</returns>
    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("StartFullscreen", StartFullscreen),
            new XElement("PauseOnFocusLoss", PauseOnFocusLoss),
            new XElement("SaveStateOnExit", SaveStateOnExit),
            new XElement("RewindEnable", RewindEnable),
            new XElement("RunaheadFrameCount", RunaheadFrameCount),
            new XElement("Renderer", Renderer),
            new XElement("ResolutionScale", ResolutionScale),
            new XElement("TextureFilter", TextureFilter),
            new XElement("WidescreenHack", WidescreenHack),
            new XElement("PgxpEnable", PgxpEnable),
            new XElement("AspectRatio", AspectRatio),
            new XElement("Vsync", Vsync),
            new XElement("OutputVolume", OutputVolume),
            new XElement("OutputMuted", OutputMuted),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }


    /// <summary>
    /// Copies the values from another emulator settings instance if it is a DuckStation settings instance.
    /// </summary>
    /// <param name="other">The other emulator settings instance to copy from.</param>
    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not DuckStationSettings src) return;

        StartFullscreen = src.StartFullscreen;
        PauseOnFocusLoss = src.PauseOnFocusLoss;
        SaveStateOnExit = src.SaveStateOnExit;
        RewindEnable = src.RewindEnable;
        RunaheadFrameCount = src.RunaheadFrameCount;
        Renderer = src.Renderer;
        ResolutionScale = src.ResolutionScale;
        TextureFilter = src.TextureFilter;
        WidescreenHack = src.WidescreenHack;
        PgxpEnable = src.PgxpEnable;
        AspectRatio = src.AspectRatio;
        Vsync = src.Vsync;
        OutputVolume = src.OutputVolume;
        OutputMuted = src.OutputMuted;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }


    /// <summary>
    /// Resets all DuckStation settings to their default values.
    /// </summary>
    public void ResetDefaults()
    {
        CopyFrom(new DuckStationSettings());
    }
}
