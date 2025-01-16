using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace SimpleLauncher;

public class HelpUserConfig
{
    private const string FilePath = "helpuser.xml";

    public List<SystemHelper> Systems { get; private set; } = new();

    public void Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                string contextMessage = $"The file 'helpuser.xml' is missing.";
                Exception ex = new Exception(contextMessage);
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                var result = MessageBox.Show("The file 'helpuser.xml' is missing.\n\n" +
                                             "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
                }
                else
                {
                    return;
                }
                return;
            }

            XDocument doc;

            try
            {
                doc = XDocument.Load(FilePath);
            }
            catch (Exception ex)
            {
                string contextMessage = "Unable to load 'helpuser.xml'. The file may be corrupted.";
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                var result = MessageBox.Show("Unable to load 'helpuser.xml'. The file may be corrupted.\n\n" +
                                             "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
                }
                else
                {
                    return;
                }

                return;
            }

            Systems = doc.Descendants("System")
                .Select(system =>
                {
                    try
                    {
                        return new SystemHelper
                        {
                            SystemName = (string)system.Element("SystemName"),
                            SystemHelperText = NormalizeText((string)system.Element("SystemHelper"))
                        };
                    }
                    catch (Exception ex)
                    {
                        string contextMessage = $"Warning: Failed to parse the file 'helpuser.xml'.";
                        Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                        logTask.Wait(TimeSpan.FromSeconds(2));
                        
                        var result = MessageBox.Show("Warning: Failed to parse the file 'helpuser.xml'.\n\n" +
                                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
                            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                        if (result == MessageBoxResult.Yes)
                        {
                            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
                        }
                        else
                        {
                            return null;
                        }
                        
                        return null; // Ignore invalid system entries
                    }
                })
                .Where(helper => helper != null) // Filter out invalid entries
                .ToList();

            if (!Systems.Any())
            {
                string contextMessage = $"No valid systems found in the file 'helpuser.xml'.";
                Exception ex = new Exception(contextMessage);
                Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                var result = MessageBox.Show("No valid systems found in the file 'helpuser.xml'.\n\n" +
                                             "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
                }
            }
        }
        catch (Exception ex)
        {
            string contextMessage = "Unexpected error while loading 'helpuser.xml'.";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            var result = MessageBox.Show("Unexpected error while loading 'helpuser.xml'.\n\n" +
                                         "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
                "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
            }
        }
    }


    private static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        // Process each line to remove leading spaces while keeping line breaks
        var lines = text.Split(['\r', '\n'], StringSplitOptions.None); // Preserve empty lines
        return string.Join(Environment.NewLine, lines.Select(line => line.TrimStart()));
    }
}

public class SystemHelper
{
    public string SystemName { get; init; }
    public string SystemHelperText { get; init; }
}