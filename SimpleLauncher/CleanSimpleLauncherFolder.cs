using System;
using System.IO;

namespace SimpleLauncher;

public static class CleanSimpleLauncherFolder
{
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string TempFolder = Path.Combine(AppDirectory, "temp");
    private static readonly string TempFolder2 = Path.Combine(AppDirectory, "temp2");
    private static readonly string TempFolder3 = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
    private static readonly string UpdateFile = Path.Combine(AppDirectory, "update.zip");

    public static void CleanupTrash()
    {
        if (Directory.Exists(TempFolder))
        {
            try
            {
                Directory.Delete(TempFolder, true);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        if (Directory.Exists(TempFolder2))
        {
            try
            {
                Directory.Delete(TempFolder2, true);
            }
            catch (Exception)
            {
                // ignore
            }
        }
        
        if (Directory.Exists(TempFolder3))
        {
            try
            {
                Directory.Delete(TempFolder2, true);
            }
            catch (Exception)
            {
                // ignore
            }
        }
            
        if (File.Exists(UpdateFile))
        {
            try
            {
                File.Delete(UpdateFile);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}