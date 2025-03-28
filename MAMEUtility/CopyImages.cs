using System.IO;
using System.Xml.Linq;

namespace MAMEUtility;

public static class CopyImages
{
    public static async Task CopyImagesFromXmlAsync(string[] xmlFilePaths, string sourceDirectory, string destinationDirectory, IProgress<int> progress)
    {
        var totalFiles = xmlFilePaths.Length;
        var filesCopied = 0;

        foreach (var xmlFilePath in xmlFilePaths)
        {
            try
            {
                await ProcessXmlFileAsync(xmlFilePath, sourceDirectory, destinationDirectory, progress);
                filesCopied++;
                var progressPercentage = (double)filesCopied / totalFiles * 100;
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

        var totalImages = machineNames.Count;
        var imagesCopied = 0;

        foreach (var machineName in machineNames)
        {
            // Here, 'machineName' is enforced to be non-null by the previous checks, so the null-forgiving operator '!' is used.
            await CopyImageFileAsync(sourceDirectory, destinationDirectory, machineName, "png");
            await CopyImageFileAsync(sourceDirectory, destinationDirectory, machineName, "jpg");
            await CopyImageFileAsync(sourceDirectory, destinationDirectory, machineName, "jpeg");

            imagesCopied++;
            var progressPercentage = (double)imagesCopied / totalImages * 100;
            progress.Report((int)progressPercentage);
        }
    }


    private static Task CopyImageFileAsync(string sourceDirectory, string destinationDirectory, string? machineName, string extension)
    {
        if (machineName == null)
        {
            Console.WriteLine($"Machine name is null for extension: {extension}");
            return Task.CompletedTask;
        }

        var sourceFile = Path.Combine(sourceDirectory, machineName + "." + extension);
        var destinationFile = Path.Combine(destinationDirectory, machineName + "." + extension);

        return Task.Run(() =>
        {
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, destinationFile, overwrite: true);
                Console.WriteLine($"Copied: {machineName}.{extension} to {destinationDirectory}");
            }
            else
            {
                Console.WriteLine($"File not found: {machineName}.{extension}");
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