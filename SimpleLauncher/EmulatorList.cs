using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SimpleLauncher
{
    [XmlRoot("EmulatorList")]
    public class EmulatorList
    {
        [XmlElement("Emulator")]
        public List<Emulator> Emulators { get; set; }

        public static EmulatorList LoadFromFile(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(EmulatorList));
            using StreamReader reader = new StreamReader(filePath);
            return (EmulatorList)serializer.Deserialize(reader);
        }
    }

    public class Emulator
    {
        public string EmulatorName { get; set; }
        public string EmulatorWebsite { get; set; }
        public string EmulatorDownloadPage { get; set; }
        public string EmulatorDownloadBinary { get; set; }
        public string EmulatorDownloadCore { get; set; }
        public string EmulatorDownloadExtras { get; set; }
        
        [XmlArray("RelatedSystems")]
        [XmlArrayItem("RelatedSystem")]
        public List<string> RelatedSystems { get; set; }
    }
}