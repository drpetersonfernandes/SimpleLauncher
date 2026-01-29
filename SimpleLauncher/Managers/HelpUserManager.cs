using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Utils;

namespace SimpleLauncher.Managers;

public class HelpUserManager
{
    private const string FilePath = "helpuser.xml";

    public List<SystemHelper> Systems { get; private set; } = [];

    public void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                // Notify developer
                const string contextMessage = "The file 'helpuser.xml' is missing.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.FileHelpUserXmlIsMissingMessageBox();

                return;
            }

            XDocument doc;
            try
            {
                XmlReaderSettings settings = new()
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };
                using var reader = XmlReader.Create(FilePath, settings);
                doc = XDocument.Load(reader, LoadOptions.None);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Unable to load 'helpuser.xml'. The file may be corrupted.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.FailedToLoadHelpUserXmlMessageBox();

                return;
            }

            var entryParseErrorOccurred = false;
            var parsedSystems = doc.Descendants("System")
                .Select(system =>
                {
                    try
                    {
                        var systemNameElement = system.Element("SystemName");
                        var systemHelperElement = system.Element("SystemHelper");

                        // Basic validation: SystemName must exist and not be empty.
                        if (systemNameElement == null || string.IsNullOrEmpty(systemNameElement.Value))
                        {
                            throw new XmlException(
                                $"SystemName element is missing or empty. System data: {system.ToString(SaveOptions.DisableFormatting).Substring(0, Math.Min(200, system.ToString(SaveOptions.DisableFormatting).Length))}");
                        }

                        return new SystemHelper
                        {
                            SystemName = systemNameElement.Value,
                            SystemHelperText = NormalizeText((string)systemHelperElement)
                        };
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        var problematicEntry = system.ToString(SaveOptions.DisableFormatting);
                        problematicEntry = problematicEntry.Substring(0, Math.Min(200, problematicEntry.Length)); // Truncate for log
                        const string contextMessage = "Failed to parse an entry in 'helpuser.xml'.";
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"{contextMessage} Entry: {problematicEntry}");

                        entryParseErrorOccurred = true; // Mark that at least one error occurred
                        return null; // Do not show a message box here; return null to filter out later
                    }
                })
                .Where(static helper => helper != null) // Filter out entries that failed to parse
                .ToList();

            // If any entry failed to parse, notify the user once.
            if (entryParseErrorOccurred)
            {
                var result = MessageBoxLibrary.CouldNotLoadHelpUserXmlMessageBox();
                if (result == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                }
                else
                {
                    Systems = []; // Return an empty list
                    return;
                }
                // If user declined reinstall, proceed with any systems that were successfully parsed.
            }

            Systems = parsedSystems; // Assign successfully parsed systems

            // If, after all processing (including potential parsing errors where user declined reinstall),
            // there are no systems, then show the "NoSystemInHelpUserXmlMessageBox".
            if (Systems.Count != 0) return;

            {
                // Notify developer
                const string contextMessage = "No valid systems found in 'helpuser.xml' after processing.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                // Notify user
                MessageBoxLibrary.NoSystemInHelpUserXmlMessageBox();
            }
            // If Systems.Count > 0, the method completes, and Systems is populated.
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Unexpected error while loading 'helpuser.xml'.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorWhileLoadingHelpUserXmlMessageBox();
        }
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // First, normalize all line endings to \n
        text = text.Replace("\r\n", "\n").Replace("\r", "\n");

        // Split by normalized line endings and process
        return string.Join(Environment.NewLine,
            text.Split('\n')
                .Select(static line => line.TrimStart()));
    }
}