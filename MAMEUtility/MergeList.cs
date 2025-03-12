using System.Xml.Linq;
using MessagePack;
using System.IO;

namespace MameUtility;

public static class MergeList
{
    // Traditional method to merge and save as XML only (kept for backward compatibility)
    public static void MergeAndSave(string[] inputFilePaths, string outputFilePath)
    {
        var mergedDoc = MergeDocumentsFromPaths(inputFilePaths);
        if (mergedDoc != null)
        {
            mergedDoc.Save(outputFilePath);
            Console.WriteLine($"Merged XML saved successfully to: {outputFilePath}");
        }
    }

    // New method to merge and save as both XML and DAT files
    public static void MergeAndSaveBoth(string[] inputFilePaths, string xmlOutputPath, string datOutputPath)
    {
        var mergedDoc = MergeDocumentsFromPaths(inputFilePaths);
        if (mergedDoc != null)
        {
            // Save as XML
            mergedDoc.Save(xmlOutputPath);
            Console.WriteLine($"Merged XML saved successfully to: {xmlOutputPath}");

            // Save as MessagePack DAT file
            var machines = ConvertXmlToMachines(mergedDoc);
            SaveMachinesToDat(machines, datOutputPath);
            Console.WriteLine($"Merged DAT file saved successfully to: {datOutputPath}");
        }
    }

    // Helper method to merge documents from paths
    private static XDocument? MergeDocumentsFromPaths(string[] inputFilePaths)
    {
        XDocument mergedDoc = new(new XElement("Machines"));

        foreach (var inputFilePath in inputFilePaths)
        {
            try
            {
                var inputDoc = XDocument.Load(inputFilePath);

                // Validate and normalize the document structure before merging
                if (!IsValidAndNormalizeStructure(inputDoc, out var normalizedRoot))
                {
                    Console.WriteLine($"The file {inputFilePath} does not have the correct XML structure and will not be merged. Operation stopped.");
                    return null; // Stop processing further files
                }

                // Merge normalized content
                if (normalizedRoot != null) mergedDoc = MergeDocuments(mergedDoc, normalizedRoot);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while loading the file {inputFilePath}: {ex.Message}");
                return null; // Stop processing if there's an error loading a file
            }
        }

        return mergedDoc;
    }

    private static bool IsValidAndNormalizeStructure(XDocument doc, out XElement? normalizedRoot)
    {
        normalizedRoot = null;

        // Check for Machines format
        if (doc.Root?.Name.LocalName == "Machines" && doc.Root.Elements("Machine").Any())
        {
            normalizedRoot = doc.Root;
            return true;
        }

        // Check for Softwares format
        if (doc.Root?.Name.LocalName == "Softwares" && doc.Root.Elements("Software").Any())
        {
            // Normalize Softwares to Machines format
            normalizedRoot = new XElement("Machines",
                doc.Root.Elements("Software").Select(software =>
                    new XElement("Machine",
                        new XElement("MachineName", software.Element("SoftwareName")?.Value),
                        software.Element("Description")
                    )
                )
            );
            return true;
        }

        // Invalid structure
        return false;
    }

    private static XDocument MergeDocuments(XDocument doc1, XElement normalizedRoot)
    {
        // Ensure that the first document has a non-null Root element before attempting to merge.
        if (doc1.Root == null)
        {
            throw new InvalidOperationException("The first document does not have a root element.");
        }

        // Add elements from normalizedRoot to the first document
        doc1.Root.Add(normalizedRoot.Elements());

        return doc1;
    }

    // Convert XML to a list of machines compatible with SimpleLauncher's MameConfig
    private static List<MachineInfo> ConvertXmlToMachines(XDocument doc)
    {
        var machines = new List<MachineInfo>();

        foreach (var machineElement in doc.Root?.Elements("Machine") ?? Enumerable.Empty<XElement>())
        {
            var machine = new MachineInfo
            {
                MachineName = machineElement.Element("MachineName")?.Value ?? string.Empty,
                Description = machineElement.Element("Description")?.Value ?? string.Empty
            };
            machines.Add(machine);
        }

        return machines;
    }

    // Save machines to MessagePack DAT file
    private static void SaveMachinesToDat(List<MachineInfo> machines, string outputFilePath)
    {
        try
        {
            // Serialize the machines' list to a MessagePack binary array
            var binary = MessagePackSerializer.Serialize(machines);

            // Write the binary data to the output file
            File.WriteAllBytes(outputFilePath, binary);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving DAT file: {ex.Message}");
        }
    }
}

// This class matches the structure in SimpleLauncher.MameConfig
[MessagePackObject]
public class MachineInfo
{
    [Key(0)]
    public string MachineName { get; set; } = string.Empty;

    [Key(1)]
    public string Description { get; set; } = string.Empty;
}