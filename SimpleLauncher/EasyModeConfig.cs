using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace SimpleLauncher
{
    [XmlRoot("EasyMode")]
    public class EasyModeConfig
    {
        [XmlElement("EasyModeSystemConfig")]
        public List<EasyModeSystemConfig> Systems { get; set; }

        public static EasyModeConfig Load(string xmlFilePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(EasyModeConfig));
            using FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open);
            return (EasyModeConfig)serializer.Deserialize(fileStream);
        }
    }

    public class EasyModeSystemConfig
    {
        public string SystemName { get; set; }
        public string SystemFolder { get; set; }
        public string SystemImageFolder { get; set; }
        public bool SystemIsMame { get; set; }

        [XmlArray("FileFormatsToSearch")]
        [XmlArrayItem("FormatToSearch")]
        public List<string> FileFormatsToSearch { get; set; }

        public bool ExtractFileBeforeLaunch { get; set; }

        [XmlArray("FileFormatsToLaunch")]
        [XmlArrayItem("FormatToLaunch")]
        public List<string> FileFormatsToLaunch { get; set; }

        [XmlElement("Emulators")]
        public EmulatorsConfig Emulators { get; set; }
    }

    public class EmulatorsConfig
    {
        [XmlElement("Emulator")]
        public EmulatorConfig Emulator { get; set; }
    }

    public class EmulatorConfig
    {
        public string EmulatorName { get; set; }
        public string EmulatorLocation { get; set; }
        public string EmulatorParameters { get; set; }
        public string EmulatorDownloadPage { get; set; }
        public string EmulatorLatestVersion { get; set; }
        public string EmulatorDownloadLink { get; set; }
        public bool EmulatorDownloadRename { get; set; }
        public string EmulatorDownloadExtractPath { get; set; }
        public string CoreLocation { get; set; }
        public string CoreLatestVersion { get; set; }
        public string CoreDownloadLink { get; set; }
        public string CoreDownloadExtractPath { get; set; }
        public string ExtrasLocation { get; set; }
        public string ExtrasLatestVersion { get; set; }
        public string ExtrasDownloadLink { get; set; }
        public string ExtrasDownloadExtractPath { get; set; }
    }
}