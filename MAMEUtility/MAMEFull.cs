using System.ComponentModel;
using System.Xml.Linq;

namespace MAMEUtility;

public static class MameFull
{
    public static async Task CreateAndSaveMameFullAsync(XDocument inputDoc, string outputFilePathMameFull, BackgroundWorker worker)
    {
        Console.WriteLine($"Output folder for MAME Full: {outputFilePathMameFull}");

        await Task.Run(() =>
        {
            var totalMachines = inputDoc.Descendants("machine").Count();
            var machinesProcessed = 0;

            Console.WriteLine($"Total machines: {totalMachines}");

            foreach (var machine in inputDoc.Descendants("machine"))
            {
                var machineName = machine.Attribute("name")?.Value;

                Console.WriteLine($"Processing machine: {machineName}");

                machinesProcessed++;
                var progressPercentage = (int)((double)machinesProcessed / totalMachines * 100);
                worker.ReportProgress(progressPercentage);

                Console.WriteLine($"Progress: {machinesProcessed}/{totalMachines}");
            }

            Console.WriteLine("Saving to file...");

            XDocument allMachineDetailsDoc = new(
                new XElement("Machines",
                    from machine in inputDoc.Descendants("machine")
                    select new XElement("Machine",
                        new XElement("MachineName", machine.Attribute("name")?.Value),
                        new XElement("Description", machine.Element("description")?.Value)
                    )
                )
            );

            allMachineDetailsDoc.Save(outputFilePathMameFull);
            worker.ReportProgress(100);
        });
    }
}