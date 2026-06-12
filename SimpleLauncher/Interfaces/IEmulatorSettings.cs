using System.Xml.Linq;

namespace SimpleLauncher.Interfaces;

public interface IEmulatorSettings
{
    void LoadFromXml(XElement settings);
    XElement ToXElement();
    void CopyFrom(IEmulatorSettings other);
    void ResetDefaults();
}
