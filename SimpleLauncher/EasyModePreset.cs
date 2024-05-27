using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SimpleLauncher
{
    [XmlRoot("EmulatorList")]
    public class EasyModePreset
    {
        [XmlElement("Emulator")]
        public List<Emulator> Emulators { get; set; }

        public static EasyModePreset LoadFromFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(EasyModePreset));
            using StreamReader reader = new StreamReader(filePath);
            return (EasyModePreset)serializer.Deserialize(reader);
        }
    }

    public class Emulator
    {
        public string EmulatorName { get; set; }
        public string EmulatorWebsite { get; set; }
        public string EmulatorDownloadPage { get; set; }
        public string EmulatorLatestVersion { get; set; }
        public string EmulatorDownloadBinary { get; set; }
        public string EmulatorDownloadCore { get; set; }
        public string EmulatorDownloadExtras { get; set; }
        
        [XmlArray("RelatedSystems")]
        [XmlArrayItem("RelatedSystem")]
        public List<string> RelatedSystems { get; set; }
    }
}