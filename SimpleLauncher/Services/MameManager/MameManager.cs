using MessagePack;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.MameManager;

[MessagePackObject]
public class MameManager
{
    [Key(0)]
    public string MachineName { get; set; } = "";

    [Key(1)]
    public string Description { get; set; } = "";

    private static readonly string DefaultDatPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mame.dat");

    public static List<MameManager> LoadFromDat(ILogErrors logErrors, string datPath = null, IMessageBoxLibraryService messageBox = null)
    {
        datPath ??= DefaultDatPath;

        if (!File.Exists(datPath))
        {
            // Notify developer
            const string contextMessage = "The file 'mame.dat' could not be found in the application folder.";
            logErrors.LogAndForget(null, contextMessage);

            // Notify user
            if (messageBox != null)
            {
                _ = messageBox.ReinstallSimpleLauncherFileMissingMessageBoxAsync();
            }

            return []; // return an empty list
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
            logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            if (messageBox != null)
            {
                _ = messageBox.ReinstallSimpleLauncherFileCorruptedMessageBoxAsync();
            }

            return []; // return an empty list
        }
    }
}