using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace SimpleLauncher;

public class MameConfig
{
    public string MachineName { get; private init; }
    public string Description { get; private init; }

    private static readonly string DefaultXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mame.xml");

    public static List<MameConfig> LoadFromXml(string xmlPath = null)
    {
        xmlPath ??= DefaultXmlPath;

        // Check if the mame.xml file exists
        if (!File.Exists(xmlPath))
        {
            string contextMessage = $"The file 'mame.xml' could not be found in the application folder.";
            Exception ex = new Exception(contextMessage);
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));

            ReinstallSimpleLauncherFileMissing();

            return new List<MameConfig>();
        }

        try
        {
            XDocument xmlDoc = XDocument.Load(xmlPath);
            return xmlDoc.Descendants("Machine")
                .Select(m => new MameConfig
                {
                    MachineName = m.Element("MachineName")?.Value,
                    Description = m.Element("Description")?.Value
                }).ToList();
        }
        catch (Exception ex)
        {
            string contextMessage = $"The file mame.xml could not be loaded or is corrupted.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
                
            ReinstallSimpleLauncherFileCorrupted();

            return new List<MameConfig>();
        }
    }

    private static void ReinstallSimpleLauncherFileCorrupted()
    {
        var result = MessageBox.Show("The application could not load the file 'mame.xml' or it is corrupted.\n\n" +
                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
        else
        {
            MessageBox.Show("Please reinstall 'Simple Launcher' manually.\n\n" +
                            "The application will Shutdown",
                "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                    
            // Shutdown the application and exit
            Application.Current.Shutdown();
            Environment.Exit(0);    
        }
    }

    private static void ReinstallSimpleLauncherFileMissing()
    {
        var result = MessageBox.Show("The file 'mame.xml' could not be found in the application folder.\n\n" +
                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
            "File Missing", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
        else
        {
            MessageBox.Show("Please reinstall 'Simple Launcher' manually.\n\n" +
                            "The application will Shutdown",
                "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                    
            // Shutdown the application and exit
            Application.Current.Shutdown();
            Environment.Exit(0);    
        }
    }
}