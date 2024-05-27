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
            using (FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open))
            {
                return (EasyModeConfig)serializer.Deserialize(fileStream);
            }
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

        public EmulatorConfig Emulator { get; set; }
    }

    public class EmulatorConfig
    {
        public string EmulatorName { get; set; }
        public string EmulatorLocation { get; set; }
        public string EmulatorParameters { get; set; }
        public string EmulatorDownloadPage { get; set; }
        public string EmulatorLatestVersion { get; set; }
        public string EmulatorBinaryDownload { get; set; }
        public bool EmulatorBinaryRename { get; set; }
        public string EmulatorBinaryExtractPathTo { get; set; }
        public string EmulatorCoreDownload { get; set; }
        public string EmulatorCoreExtractPath { get; set; }
        public string EmulatorExtrasDownload { get; set; }
        public string EmulatorExtrasExtractPath { get; set; }
    }
}
