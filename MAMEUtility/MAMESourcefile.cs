using System.IO;
using System.Xml.Linq;

namespace MAMEUtility;

public static class MameSourcefile
{
    public static async Task CreateAndSaveMameSourcefileAsync(XDocument inputDoc, string outputFolderMameSourcefile, IProgress<int> progress)
    {
        Console.WriteLine($"Output folder for MAME Sourcefile: {outputFolderMameSourcefile}");

        try
        {
            // Extract unique source files
            var sourceFiles = inputDoc.Descendants("machine")
                .Select(static m => (string?)m.Attribute("sourcefile"))
                .Distinct()
                .Where(static s => !string.IsNullOrEmpty(s));

            var enumerable = sourceFiles.ToList();
            var totalSourceFiles = enumerable.Count;
            var sourceFilesProcessed = 0;

            // Iterate over each source file and create an XML for each
            foreach (var sourceFile in enumerable)
            {
                // Check if the source file name is valid
                if (string.IsNullOrWhiteSpace(sourceFile))
                {
                    Console.WriteLine("Skipping invalid source file.");
                    continue; // Skip to the next source file
                }

                // Remove the ".cpp" extension from the source file name
                var safeSourceFileName = Path.GetFileNameWithoutExtension(sourceFile);

                // Replace or remove invalid characters from the file name
                safeSourceFileName = ReplaceInvalidFileNameChars(safeSourceFileName);

                // Construct the output file path
                var outputFilePath = Path.Combine(outputFolderMameSourcefile, $"{safeSourceFileName}.xml");

                // Create and save the filtered document
                await CreateAndSaveFilteredDocumentAsync(inputDoc, outputFilePath, sourceFile);

                sourceFilesProcessed++;
                var progressPercentage = (double)sourceFilesProcessed / totalSourceFiles * 100;
                progress.Report((int)progressPercentage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            await LogError.LogAsync(ex, "Error in method MAMESourcefile.CreateAndSaveMameSourcefileAsync");
        }
    }

    private static async Task CreateAndSaveFilteredDocumentAsync(XContainer inputDoc, string outputPath, string sourceFile)
    {
        // Create a new XML document for machines based on the predicate
        XDocument filteredDoc = new(
            new XElement("Machines",
                from machine in inputDoc.Descendants("machine")
                where Predicate(machine)
                select new XElement("Machine",
                    new XElement("MachineName", machine.Attribute("name")?.Value),
                    new XElement("Description", machine.Element("description")?.Value)
                )
            )
        );

        // Save the filtered XML document
        try
        {
            await Task.Run(() => filteredDoc.Save(outputPath));
            Console.WriteLine($"Successfully created: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create file for {sourceFile}. Error: {ex.Message}");
            await LogError.LogAsync(ex, "Error in method MAMESourcefile.CreateAndSaveFilteredDocumentAsync");
        }

        return;

        // Filtering condition based on the source file
        bool Predicate(XElement machine)
        {
            return (string?)machine.Attribute("sourcefile") == sourceFile;
        }
    }

    private static string ReplaceInvalidFileNameChars(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var invalidChar in invalidChars)
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        return fileName;
    }
}