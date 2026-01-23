using System.Collections.Concurrent;
using System.IO;
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

            Parallel.ForEach(files, file =>
            {
                try
                {
                    var doc = XDocument.Load(file);
                    var softwares = doc.Descendants("software")
                        .Select(static software => new MachineInfo
                        {
                            MachineName = software.Attribute("name")?.Value ?? string.Empty,
                            Description = software.Element("description")?.Value ?? "No Description"
                        });

                    foreach (var s in softwares)
                    {
                        allSoftware.Add(s);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warning($"Skipping file '{Path.GetFileName(file)}' due to an error: {ex.Message}");
                }
            });

            logger.Info($"Extracted {allSoftware.Count} entries from software lists.");
            return allSoftware.ToList();
        });
    }
}