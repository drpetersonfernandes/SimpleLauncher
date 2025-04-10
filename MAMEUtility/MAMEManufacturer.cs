using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MAMEUtility;

public partial class MameManufacturer
{
    public static async Task CreateAndSaveMameManufacturerAsync(XDocument inputDoc, string outputFolderMameManufacturer, IProgress<int> progress)
    {
        Console.WriteLine($"Output folder for MAME Manufacturer: {outputFolderMameManufacturer}");

        try
        {
            // Extract unique manufacturers
            var manufacturers = inputDoc.Descendants("machine")
                .Select(m => (string?)m.Element("manufacturer"))
                .Distinct()
                .Where(m => !string.IsNullOrEmpty(m));

            var enumerable = manufacturers.ToList();
            var totalManufacturers = enumerable.Count;
            var manufacturersProcessed = 0;

            // Iterate over each manufacturer and create an XML for each
            foreach (var manufacturer in enumerable)
            {
                if (manufacturer != null)
                {
                    var safeManufacturerName = RemoveExtraWhitespace(manufacturer
                            .Replace("<", "")
                            .Replace(">", "")
                            .Replace(":", "")
                            .Replace("\"", "")
                            .Replace("/", "")
                            .Replace("\\", "")
                            .Replace("|", "")
                            .Replace("?", "")
                            .Replace("*", "")
                            .Replace("unknown", "UnknownManufacturer")
                            .Trim())
                        .Replace("&amp;", "&"); // Replace &amp; with & in the filename.

                    var outputFilePath = Path.Combine(outputFolderMameManufacturer, $"{safeManufacturerName}.xml");
                    Console.WriteLine($"Attempting to create file for: {safeManufacturerName}.xml");

                    await CreateAndSaveFilteredDocumentAsync(inputDoc, outputFilePath, manufacturer, safeManufacturerName);

                    manufacturersProcessed++;
                    var progressPercentage = (double)manufacturersProcessed / totalManufacturers * 100;
                    progress.Report((int)progressPercentage);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }

    private static async Task CreateAndSaveFilteredDocumentAsync(XDocument inputDoc, string outputPath, string manufacturer, string safeManufacturerName)
    {
        var filteredDoc = CreateFilteredDocument(inputDoc, manufacturer);

        try
        {
            await Task.Run(() => filteredDoc.Save(outputPath));
            Console.WriteLine($"Successfully created: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create file for {safeManufacturerName}. Error: {ex.Message}");
        }
    }

    private static XDocument CreateFilteredDocument(XDocument inputDoc, string manufacturer)
    {
        bool Predicate(XElement machine) =>
            (machine.Element("manufacturer")?.Value ?? "") == manufacturer &&
            //!(machine.Attribute("name")?.Value.Contains("bootleg", StringComparison.InvariantCultureIgnoreCase) ?? false) &&
            //!(machine.Element("description")?.Value.Contains("bootleg", StringComparison.InvariantCultureIgnoreCase) ?? false) &&
            //!(machine.Element("description")?.Value.Contains("prototype", StringComparison.InvariantCultureIgnoreCase) ?? false) &&
            //!(machine.Element("description")?.Value.Contains("playchoice", StringComparison.InvariantCultureIgnoreCase) ?? false) &&
            //machine.Attribute("cloneof") == null &&
            !(machine.Attribute("name")?.Value.Contains("bios", StringComparison.InvariantCultureIgnoreCase) ?? false) &&
            !(machine.Element("description")?.Value.Contains("bios", StringComparison.InvariantCultureIgnoreCase) ?? false) &&
            (machine.Element("driver")?.Attribute("emulation")?.Value ?? "") == "good";

        // Retrieve the matched machines
        var matchedMachines = inputDoc.Descendants("machine").Where(Predicate).ToList();

        // Create a new XML document for machines based on the matched machines
        XDocument filteredDoc = new(
            new XElement("Machines",
                from machine in matchedMachines
                select new XElement("Machine",
                    new XElement("MachineName", RemoveExtraWhitespace(machine.Attribute("name")?.Value ?? "").Replace("&amp;", "&")),
                    new XElement("Description", RemoveExtraWhitespace(machine.Element("description")?.Value ?? "").Replace("&amp;", "&"))
                )
            )
        );
        return filteredDoc;
    }

    private static string RemoveExtraWhitespace(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return MyRegex().Replace(input, " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MyRegex();
}