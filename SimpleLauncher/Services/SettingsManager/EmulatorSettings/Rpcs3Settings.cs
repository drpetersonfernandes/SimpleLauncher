using System.Xml.Linq;

namespace SimpleLauncher.Services.SettingsManager.EmulatorSettings;

using Interfaces;

public class Rpcs3Settings : IEmulatorSettings
{
    private const string SectionName = "Rpcs3";

    public string Renderer { get; set; } = "Vulkan";
    public string Resolution { get; set; } = "1280x720";
    public string AspectRatio { get; set; } = "16:9";
    public bool Vsync { get; set; }
    public int ResolutionScale { get; set; } = 100;
    public int AnisotropicFilter { get; set; }
    public string PpuDecoder { get; set; } = "Recompiler (LLVM)";
    public string SpuDecoder { get; set; } = "Recompiler (LLVM)";
    public string AudioRenderer { get; set; } = "Cubeb";
    public bool AudioBuffering { get; set; } = true;
    public bool StartFullscreen { get; set; }
    public bool ShowSettingsBeforeLaunch { get; set; }

    public void LoadFromXml(XElement settings)
    {
        var s = settings.Element(SectionName);
        Renderer = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Renderer), "Vulkan");
        Resolution = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(Resolution), "1280x720");
        AspectRatio = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AspectRatio), "16:9");
        Vsync = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(Vsync), false);
        ResolutionScale = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(ResolutionScale), 100);
        AnisotropicFilter = EmulatorXmlHelpers.ReadInt(s, SectionName, settings, nameof(AnisotropicFilter), 0);
        PpuDecoder = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(PpuDecoder), "Recompiler (LLVM)");
        SpuDecoder = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(SpuDecoder), "Recompiler (LLVM)");
        AudioRenderer = EmulatorXmlHelpers.ReadString(s, SectionName, settings, nameof(AudioRenderer), "Cubeb");
        AudioBuffering = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(AudioBuffering), true);
        StartFullscreen = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(StartFullscreen), false);
        ShowSettingsBeforeLaunch = EmulatorXmlHelpers.ReadBool(s, SectionName, settings, nameof(ShowSettingsBeforeLaunch), false);
    }

    public XElement ToXElement()
    {
        return new XElement(SectionName,
            new XElement("Renderer", Renderer),
            new XElement("Resolution", Resolution),
            new XElement("AspectRatio", AspectRatio),
            new XElement("Vsync", Vsync),
            new XElement("ResolutionScale", ResolutionScale),
            new XElement("AnisotropicFilter", AnisotropicFilter),
            new XElement("PpuDecoder", PpuDecoder),
            new XElement("SpuDecoder", SpuDecoder),
            new XElement("AudioRenderer", AudioRenderer),
            new XElement("AudioBuffering", AudioBuffering),
            new XElement("StartFullscreen", StartFullscreen),
            new XElement("ShowSettingsBeforeLaunch", ShowSettingsBeforeLaunch));
    }

    public void CopyFrom(IEmulatorSettings other)
    {
        if (other is not Rpcs3Settings src) return;

        Renderer = src.Renderer;
        Resolution = src.Resolution;
        AspectRatio = src.AspectRatio;
        Vsync = src.Vsync;
        ResolutionScale = src.ResolutionScale;
        AnisotropicFilter = src.AnisotropicFilter;
        PpuDecoder = src.PpuDecoder;
        SpuDecoder = src.SpuDecoder;
        AudioRenderer = src.AudioRenderer;
        AudioBuffering = src.AudioBuffering;
        StartFullscreen = src.StartFullscreen;
        ShowSettingsBeforeLaunch = src.ShowSettingsBeforeLaunch;
    }

    public void ResetDefaults()
    {
        CopyFrom(new Rpcs3Settings());
    }
}
