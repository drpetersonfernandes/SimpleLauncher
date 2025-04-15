using System.IO;
using System.Xml.Linq;

namespace MAMEUtility;

public static class MameSoftwareList
{
    public static async void CreateAndSaveSoftwareList(string inputFolderPath, string outputFilePath, IProgress<int> progress, LogWindow logWindow)
    {
        try
        {
            if (!Directory.Exists(inputFolderPath))
            {
                logWindow.AppendLog("The specified folder does not exist.");
                throw new DirectoryNotFoundException("The specified folder does not exist.");
            }

            var files = Directory.GetFiles(inputFolderPath, "*.xml");
            if (files.Length == 0)
            {
                logWindow.AppendLog("No XML files found in the specified folder.");
                throw new FileNotFoundException("No XML files found in the specified folder.");
            }

            var softwareList = new List<XElement>();
            var processedCount = 0;

            foreach (var file in files)
            {
                XDocument doc;
                try
                {
                    logWindow.AppendLog($"Processing file: {file}");
                    doc = XDocument.Load(file);
                }
                catch (Exception ex)
                {
                    logWindow.AppendLog($"Skipping file '{file}' due to an error: {ex.Message}");
                    await LogError.LogAsync(ex, $"Skipping file '{file}' due to an error: {ex.Message}");

                    continue;
                }

                var softwares = doc.Descendants("software")
                    .Select(static software => new XElement("Software",
                        new XElement("SoftwareName", software.Attribute("name")?.Value),
                        new XElement("Description", software.Element("description")?.Value ?? "No Description")));

                softwareList.AddRange(softwares);

                processedCount++;
                progress?.Report(processedCount * 100 / files.Length);
                logWindow.AppendLog($"Processed {processedCount}/{files.Length} files.");
            }

            logWindow.AppendLog("Saving consolidated XML file...");
            var outputDoc = new XDocument(new XElement("Softwares", softwareList));
            outputDoc.Save(outputFilePath);
            logWindow.AppendLog($"Consolidated XML file saved to: {outputFilePath}");
        }
        catch (Exception ex)
        {
            await LogError.LogAsync(ex, "Error in method CreateAndSaveSoftwareList");
        }
    }
}