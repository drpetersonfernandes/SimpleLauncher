using System.Xml;
using System.Xml.Linq;
using Mame.DatCreator.Models;
using Mame.DatCreator.Services;

namespace Mame.DatCreator.Processors;

public static class MameFullProcessor
{
    public static Task<List<MachineInfo>> GetMachinesFromFullXmlAsync(string inputFilePath, WpfLogger logger)
    {
        return Task.Run(() =>
        {
            logger.Info($"Loading MAME full driver XML from: {inputFilePath}");
            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
            using var reader = XmlReader.Create(inputFilePath, settings);
            var inputDoc = XDocument.Load(reader);
            var machines = new List<MachineInfo>();

            var machineElements = inputDoc.Descendants("machine").ToList();
            logger.Info($"Found {machineElements.Count} machine entries in the MAME full driver XML.");

            foreach (var m in machineElements)
            {
                machines.Add(new MachineInfo
                {
                    MachineName = m.Attribute("name")?.Value ?? "",
                    Description = m.Element("description")?.Value ?? ""
                });
            }

            return machines;
        });
    }
}