using System.Collections.Concurrent;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Mame.DatCreator.Models;
using Mame.DatCreator.Services;

namespace Mame.DatCreator.Processors;

public static class MameSoftwareProcessor
{
    public static Task<List<MachineInfo>> GetMachinesFromSoftwareFolderAsync(string inputFolderPath, WpfLogger logger)
    {
        return Task.Run(() =>
        {
            logger.Info($"Scanning for software list XMLs in: {inputFolderPath}");
            var files = Directory.GetFiles(inputFolderPath, "*.xml");
            if (files.Length == 0)
            {
                logger.Warning("No XML files found in the software list (hash) folder.");
                return new List<MachineInfo>();
            }

            logger.Info($"Found {files.Length} software list XML files to process.");

            var allSoftware = new ConcurrentBag<MachineInfo>();
            var warnings = new ConcurrentBag<string>();

            var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };

            Parallel.ForEach(files, file =>
            {
                try
                {
                    using var reader = XmlReader.Create(file, settings);
                    var doc = XDocument.Load(reader);
                    var softwares = doc.Descendants("software")
                        .Select(static software => new MachineInfo
                        {
                            MachineName = software.Attribute("name")?.Value ?? "",
                            Description = software.Element("description")?.Value ?? "No Description"
                        });

                    foreach (var s in softwares)
                    {
                        allSoftware.Add(s);
                    }
                }
                catch (Exception ex)
                {
                    warnings.Add($"Skipping file '{Path.GetFileName(file)}' due to an error: {ex.Message}");
                }
            });

            // Log warnings after the parallel loop to avoid Dispatcher.Invoke
            // blocking thread pool threads.
            foreach (var warning in warnings)
            {
                logger.Warning(warning);
            }

            logger.Info($"Extracted {allSoftware.Count} entries from software lists.");
            return allSoftware.ToList();
        });
    }
}
