using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

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
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

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
                doc = XDocument.Load(reader);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Unable to load 'helpuser.xml'. The file may be corrupted.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

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
                        // Log detailed error for the developer
                        var problematicEntry = system.ToString(SaveOptions.DisableFormatting);
                        problematicEntry = problematicEntry.Substring(0, Math.Min(200, problematicEntry.Length)); // Truncate for log
                        const string contextMessage = "Failed to parse an entry in 'helpuser.xml'.";
                        _ = LogErrors.LogErrorAsync(ex, $"{contextMessage} Entry: {problematicEntry}");

                        entryParseErrorOccurred = true; // Mark that at least one error occurred
                        return null; // Do not show a message box here; return null to filter out later
                    }
                })
                .Where(static helper => helper != null) // Filter out entries that failed to parse
                .ToList();

            // If any entry failed to parse, notify the user once.
            if (entryParseErrorOccurred)
            {
                // MessageBoxLibrary.CouldNotLoadHelpUserXmlMessageBox() returns:
                // - true if the user chooses "No" (do not reinstall)
                // - false if the user chooses "Yes" (reinstall and shutdown is initiated)
                var userDeclinedReinstall = MessageBoxLibrary.CouldNotLoadHelpUserXmlMessageBox();
                if (!userDeclinedReinstall) // User chose "Yes" to reinstall
                {
                    // ReinstallSimpleLauncher.StartUpdaterAndShutdown() was called.
                    // The application is expected to shut down. We can set Systems to empty and return.
                    Systems = [];
                    return;
                }
                // If user declined reinstall, proceed with any systems that were successfully parsed.
            }

            Systems = parsedSystems; // Assign successfully parsed systems

            // If, after all processing (including potential parsing errors where user declined reinstall),
            // there are no systems, then show the "NoSystemInHelpUserXmlMessageBox".
            if (Systems.Count != 0) return;

            {
                // Log that no valid systems were found, regardless of the reason (empty file or all failed parse and no reinstall)
                const string contextMessage = "No valid systems found in 'helpuser.xml' after processing.";
                var ex = new Exception(contextMessage); // Use a more specific log message
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.NoSystemInHelpUserXmlMessageBox();
            }
            // If Systems.Count > 0, the method completes, and Systems is populated.
        }
        catch (Exception ex) // Catch-all for unexpected errors during the Load process
        {
            // Notify developer
            const string contextMessage = "Unexpected error while loading 'helpuser.xml'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorWhileLoadingHelpUserXmlMessageBox();
        }
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Process each line to remove leading spaces while keeping line breaks
        var lines = text.Split(['\r', '\n'], StringSplitOptions.None); // Preserve empty lines
        return string.Join(Environment.NewLine, lines.Select(static line => line.TrimStart()));
    }
}