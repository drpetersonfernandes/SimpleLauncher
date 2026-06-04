using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MessagePack;
using XmlToBinaryConverter.Models;

namespace XmlToBinaryConverter.Services;

public class ConverterService
{
    public async Task ConvertXmlToBinary(string inputPath, string outputPath, IProgress<string> progress)
    {
        try
        {
            progress.Report("Reading XML file...");

            // Read XML content as string
            var xmlContent = await File.ReadAllTextAsync(inputPath);

            progress.Report("Parsing XML content...");

            // Deserialize XML to objects
            var serializer = new XmlSerializer(typeof(History));
            History? history;
            using (var reader = new StringReader(xmlContent))
            using (var xmlReader = System.Xml.XmlReader.Create(reader, new System.Xml.XmlReaderSettings
                   {
                       DtdProcessing = System.Xml.DtdProcessing.Prohibit,
                       XmlResolver = null
                   }))
            {
                try
                {
                    history = serializer.Deserialize(xmlReader) as History;
                }
                catch (InvalidOperationException ex) when (ex.InnerException != null)
                {
                    throw new InvalidOperationException($"Failed to deserialize XML content. Inner exception: {ex.InnerException.Message}", ex);
                }
            }

            if (history == null)
            {
                throw new InvalidOperationException("Failed to deserialize XML content.");
            }

            progress.Report("Serializing to binary format...");

            // Serialize to MessagePack
            var binaryData = MessagePackSerializer.Serialize(history);

            progress.Report("Saving binary file...");

            // Save to the output file
            await File.WriteAllBytesAsync(outputPath, binaryData);

            progress.Report("Conversion completed successfully!");
        }
        catch (Exception ex)
        {
            progress.Report($"Error during conversion: {ex.Message}");
            // Rethrow the exception so the caller (ViewModel) can handle it further (e.g., logging)
            throw;
        }
    }


    public async Task ConvertBinaryToXml(string inputPath, string outputPath, IProgress<string> progress)
    {
        try
        {
            progress.Report("Reading binary file...");

            // Read binary data
            var binaryData = await File.ReadAllBytesAsync(inputPath);

            progress.Report("Deserializing from binary format...");

            // Deserialize from MessagePack
            var history = MessagePackSerializer.Deserialize<History>(binaryData);

            progress.Report("Saving XML file...");

            // Serialize objects back to XML
            var serializer = new XmlSerializer(typeof(History));
            await using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, history);
                var xmlContent = writer.ToString();
                await File.WriteAllTextAsync(outputPath, xmlContent);
            }

            progress.Report("Conversion completed successfully!");
        }
        catch (Exception ex)
        {
            progress.Report($"Error during conversion: {ex.Message}");
            // Rethrow the exception so the caller (ViewModel) can handle it further (e.g., logging)
            throw;
        }
    }
}