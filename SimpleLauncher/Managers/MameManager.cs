using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

[MessagePackObject]
public class MameManager
{
    [Key(0)]
    public string MachineName { get; set; } = string.Empty;

    [Key(1)]
    public string Description { get; set; } = string.Empty;

    private static readonly string DefaultDatPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mame.dat");

    public static List<MameManager> LoadFromDat(string datPath = null)
    {
        datPath ??= DefaultDatPath;

        // Check if the mame.dat file exists
        if (!File.Exists(datPath))
        {
            // Notify developer
            const string contextMessage = "The file 'mame.dat' could not be found in the application folder.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ReinstallSimpleLauncherFileMissingMessageBox();

            return [];
        }

        try
        {
            // Read the binary data from the DAT file
            var binaryData = File.ReadAllBytes(datPath);

            // Deserialize the binary data to a list of MameManager objects
            return MessagePackSerializer.Deserialize<List<MameManager>>(binaryData);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "The file mame.dat could not be loaded or is corrupted.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ReinstallSimpleLauncherFileCorruptedMessageBox();

            return [];
        }
    }
}