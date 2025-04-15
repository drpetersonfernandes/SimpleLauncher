using System.IO;
using System.Xml.Linq;

namespace MAMEUtility;

public static class CopyImages
{
    public static async Task CopyImagesFromXmlAsync(string[] xmlFilePaths, string sourceDirectory, string destinationDirectory, IProgress<int> progress)
    {
        var totalFiles = xmlFilePaths.Length;
        var filesCopied = 0;

        await LogError.LogMessageAsync($"Starting image copy operation. Files to process: {totalFiles}");
        await LogError.LogMessageAsync($"Source directory: {sourceDirectory}");
        await LogError.LogMessageAsync($"Destination directory: {destinationDirectory}");

        // Validate directories exist
        if (!Directory.Exists(sourceDirectory))
        {
            await LogError.LogMessageAsync($"Source directory does not exist: {sourceDirectory}", LogLevel.Error);
            throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDirectory}");
        }

        // Create destination directory if it doesn't exist
        if (!Directory.Exists(destinationDirectory))
        {
            try
            {
                Directory.CreateDirectory(destinationDirectory);
                await LogError.LogMessageAsync($"Created destination directory: {destinationDirectory}");
            }
            catch (Exception ex)
            {
                await LogError.LogAsync(ex, $"Failed to create destination directory: {destinationDirectory}");
                throw;
            }
        }

        foreach (var xmlFilePath in xmlFilePaths)
        {
            try
            {
                await LogError.LogMessageAsync($"Processing XML file: {Path.GetFileName(xmlFilePath)}");
                await ProcessXmlFileAsync(xmlFilePath, sourceDirectory, destinationDirectory, progress);
                filesCopied++;
                var progressPercentage = (double)filesCopied / totalFiles * 100;
                progress.Report((int)progressPercentage);
                await LogError.LogMessageAsync($"Successfully processed XML file: {Path.GetFileName(xmlFilePath)}");
            }
            catch (Exception ex)
            {
                await LogError.LogAsync(ex, $"An error occurred processing {Path.GetFileName(xmlFilePath)}");
                Console.WriteLine($"An error occurred processing {Path.GetFileName(xmlFilePath)}: {ex.Message}");
            }
        }

        await LogError.LogMessageAsync($"Image copy operation completed. Processed {filesCopied} of {totalFiles} files.");
    }

    private static async Task ProcessXmlFileAsync(string xmlFilePath, string sourceDirectory, string destinationDirectory, IProgress<int> progress)
    {
        XDocument xmlDoc;

        try
        {
            xmlDoc = XDocument.Load(xmlFilePath);
            await LogError.LogMessageAsync($"Successfully loaded XML file: {Path.GetFileName(xmlFilePath)}");
        }
        catch (Exception ex)
        {
            await LogError.LogAsync(ex, $"Failed to load XML file: {xmlFilePath}");
            throw;
        }

        // Validate the XML document structure
        if (!ValidateXmlStructure(xmlDoc))
        {
            var message = $"The file {Path.GetFileName(xmlFilePath)} does not match the required XML structure. Operation cancelled.";
            await LogError.LogMessageAsync(message, LogLevel.Warning);
            Console.WriteLine(message);
            return; // Stop processing this XML file
        }

        var machineNames = xmlDoc.Descendants("Machine")
            .Select(static machine => machine.Element("MachineName")?.Value)
            .Where(static name => !string.IsNullOrEmpty(name))
            .ToList();

        var totalImages = machineNames.Count;
        var imagesCopied = 0;

        await LogError.LogMessageAsync($"Found {totalImages} machine entries in {Path.GetFileName(xmlFilePath)}");

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

        await LogError.LogMessageAsync($"Completed processing {imagesCopied} machines from {Path.GetFileName(xmlFilePath)}");
    }


    private static async Task CopyImageFileAsync(string sourceDirectory, string destinationDirectory, string? machineName, string extension)
    {
        if (machineName == null)
        {
            await LogError.LogMessageAsync($"Machine name is null for extension: {extension}", LogLevel.Warning);
            Console.WriteLine($"Machine name is null for extension: {extension}");
            return;
        }

        var sourceFile = Path.Combine(sourceDirectory, machineName + "." + extension);
        var destinationFile = Path.Combine(destinationDirectory, machineName + "." + extension);

        await Task.Run(async () =>
        {
            if (File.Exists(sourceFile))
            {
                try
                {
                    File.Copy(sourceFile, destinationFile, true);
                    Console.WriteLine($"Copied: {machineName}.{extension} to {destinationDirectory}");
                }
                catch (Exception ex)
                {
                    await LogError.LogAsync(ex, $"Failed to copy {machineName}.{extension}");
                    Console.WriteLine($"Failed to copy {machineName}.{extension}: {ex.Message}");
                }
            }
            else
            {
                // This is just informational, doesn't need to be logged as a warning
                Console.WriteLine($"File not found: {machineName}.{extension}");
            }
        });
    }

    private static bool ValidateXmlStructure(XDocument xmlDoc)
    {
        // Check if the root element is "Machines" and if it contains at least one "Machine" element
        // with both "MachineName" and "Description" child elements.
        var isValid = xmlDoc.Root?.Name.LocalName == "Machines" &&
                      xmlDoc.Descendants("Machine").Any(static machine =>
                          machine.Element("MachineName") != null &&
                          machine.Element("Description") != null);

        return isValid;
    }
}