using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

public class AzaharSettings : IEmulatorSettings
{
    private const string SectionName = "Azahar";

    public int GraphicsApi { get; set; } = 1;
    public int ResolutionFactor { get; set; } = 1;
    public bool UseVsync { get; set; } = true;
    public bool AsyncShaderCompilation { get; set; } = true;
    public bool Fullscreen { get; set; } = true;
    public int Volume { get; set; } = 100;
    public bool IsNew3Ds { get; set; } = true;
    public int LayoutOption { get; set; }
    public bool ShowSettingsBeforeLaunch { get; set; }
    public bool EnableAudioStretching { get; set; } = true;

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

    public void ResetDefaults()
    {
        CopyFrom(new AzaharSettings());
    }
}
