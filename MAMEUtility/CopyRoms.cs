using System.IO;
using System.Xml.Linq;

namespace MAMEUtility
{
    public static class CopyRoms
    {
        public static async Task CopyRomsFromXmlAsync(string[] xmlFilePaths, string sourceDirectory, string destinationDirectory, IProgress<int> progress)
        {
            var totalFiles = xmlFilePaths.Length;
            var filesProcessed = 0;

            foreach (var xmlFilePath in xmlFilePaths)
            {
                try
                {
                    await ProcessXmlFileAsync(xmlFilePath, sourceDirectory, destinationDirectory, progress);
                    filesProcessed++;
                    var progressPercentage = (double)filesProcessed / totalFiles * 100;
                    progress.Report((int)progressPercentage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred processing {Path.GetFileName(xmlFilePath)}: {ex.Message}");
                }
            }
        }

        private static async Task ProcessXmlFileAsync(string xmlFilePath, string sourceDirectory, string destinationDirectory, IProgress<int> progress)
        {
            var xmlDoc = XDocument.Load(xmlFilePath);

            // Validate the XML document structure
            if (!ValidateXmlStructure(xmlDoc))
            {
                Console.WriteLine($"The file {Path.GetFileName(xmlFilePath)} does not match the required XML structure. Operation cancelled.");
                return; // Stop processing this XML file
            }

            var machineNames = xmlDoc.Descendants("Machine")
                                     .Select(machine => machine.Element("MachineName")?.Value)
                                     .Where(name => !string.IsNullOrEmpty(name))
                                     .ToList();

            var totalRoms = machineNames.Count;
            var romsProcessed = 0;

            foreach (var machineName in machineNames)
            {
                await CopyRomAsync(sourceDirectory, destinationDirectory, machineName!);

                romsProcessed++;
                var progressPercentage = (double)romsProcessed / totalRoms * 100;
                progress.Report((int)progressPercentage);
            }
        }

        private static Task CopyRomAsync(string sourceDirectory, string destinationDirectory, string machineName)
        {
            return Task.Run(() =>
            {
                Console.WriteLine($"Attempting to copy ROM for machine: {machineName}");
                try
                {
                    var sourceFile = Path.Combine(sourceDirectory, machineName + ".zip");
                    var destinationFile = Path.Combine(destinationDirectory, machineName + ".zip");

                    Console.WriteLine($"Source file path: {sourceFile}");
                    Console.WriteLine($"Destination file path: {destinationFile}");

                    if (File.Exists(sourceFile))
                    {
                        File.Copy(sourceFile, destinationFile, overwrite: true);
                        Console.WriteLine($"Successfully copied: {machineName}.zip to {destinationDirectory}");
                    }
                    else
                    {
                        Console.WriteLine($"File not found: {sourceFile}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred copying ROM for {machineName}: {ex.Message}");
                }
            });
        }

        private static bool ValidateXmlStructure(XDocument xmlDoc)
        {
            // Check if the root element is "Machines" and if it contains at least one "Machine" element
            // with both "MachineName" and "Description" child elements.
            var isValid = xmlDoc.Root?.Name.LocalName == "Machines" &&
                          xmlDoc.Descendants("Machine").Any(machine =>
                              machine.Element("MachineName") != null &&
                              machine.Element("Description") != null);

            return isValid;
        }

    }
}