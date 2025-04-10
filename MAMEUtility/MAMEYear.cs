using System.IO;
using System.Xml.Linq;

namespace MAMEUtility;

public static class MameYear
{
    public static async Task CreateAndSaveMameYear(XDocument inputDoc, string outputFolderMameYear, IProgress<int> progress)
    {
        Console.WriteLine($"Output folder for MAME Year: {outputFolderMameYear}");

        await Task.Run(() =>
        {
            try
            {
                // Extract unique years
                var years = inputDoc.Descendants("machine")
                    .Select(m => (string?)m.Element("year"))
                    .Distinct()
                    .Where(y => !string.IsNullOrEmpty(y));

                var enumerable = years.ToList();
                var totalYears = enumerable.Count;
                var yearsProcessed = 0;

                // Iterate over each unique year
                foreach (var year in enumerable)
                {
                    if (year != null)
                    {
                        // Filter machines based on year
                        var machinesForYear = inputDoc.Descendants("machine")
                            .Where(m => (string?)m.Element("year") == year);

                        // Create XML document for the year
                        XDocument yearDoc = new(
                            new XElement("Machines",
                                from machine in machinesForYear
                                select new XElement("Machine",
                                    new XElement("MachineName", machine.Attribute("name")?.Value ?? ""),
                                    new XElement("Description", machine.Element("description")?.Value ?? "")
                                )
                            )
                        );

                        // Save the XML document for the year
                        var outputFilePath = Path.Combine(outputFolderMameYear, $"{year.Replace("?", "X")}.xml");
                        yearDoc.Save(outputFilePath);
                        Console.WriteLine($"Successfully created XML file for year {year}: {outputFilePath}");

                        yearsProcessed++;
                        var progressPercentage = (double)yearsProcessed / totalYears * 100;
                        progress.Report((int)progressPercentage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        });
    }
}