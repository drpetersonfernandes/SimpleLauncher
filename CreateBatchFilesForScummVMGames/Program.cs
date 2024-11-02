using System.Runtime.InteropServices;

static class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    [STAThread]
    static void Main()
    {
        // Attach the console
        AllocConsole();  

        MessageBox.Show("This program creates batch files to launch your ScummVM games.\n\nPlease follow the instructions.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        Console.WriteLine("Select the ScummVM executable:");
        string? scummvmExePath = SelectFile();

        if (string.IsNullOrEmpty(scummvmExePath))
        {
            Console.WriteLine("No file selected. Exiting application.");
            MessageBox.Show("No file selected. Exiting application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Console.WriteLine("Select the root folder containing your ScummVM game folders:");
        string? rootFolder = SelectFolder();

        if (string.IsNullOrEmpty(rootFolder))
        {
            Console.WriteLine("No folder selected. Exiting application.");
            MessageBox.Show("No folder selected. Exiting application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        CreateBatchFilesForScummVmGames(rootFolder, scummvmExePath);
    }

    private static string? SelectFolder()
    {
        using var fbd = new FolderBrowserDialog();
        fbd.Description = "Please select the root folder where your ScummVM game folders are located.";

        DialogResult result = fbd.ShowDialog();

        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
        {
            return fbd.SelectedPath;
        }
        return null;
    }

    private static string? SelectFile()
    {
        using var ofd = new OpenFileDialog();
        ofd.Title = "Please select the ScummVM executable file (scummvm.exe)";
        ofd.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
        ofd.FilterIndex = 1;
        ofd.RestoreDirectory = true;

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            return ofd.FileName;
        }
        return null;
    }

    private static void CreateBatchFilesForScummVmGames(string rootFolder, string scummvmExePath)
    {
        string[] gameDirectories = Directory.GetDirectories(rootFolder);
        int filesCreated = 0;

        foreach (string gameDirectory in gameDirectories)
        {
            string gameFolderName = Path.GetFileName(gameDirectory);
            string batchFilePath = Path.Combine(rootFolder, gameFolderName + ".bat");

            using (StreamWriter sw = new(batchFilePath))
            {
                sw.WriteLine($"\"{scummvmExePath}\" -p \"{gameDirectory}\" --auto-detect --fullscreen");
                Console.WriteLine($"Batch file created: {batchFilePath}");
            }
            filesCreated++;
        }

        if (filesCreated > 0)
        {
            Console.WriteLine("All batch files have been successfully created.");
            MessageBox.Show("All batch files have been successfully created.\n\nThey are located in the root folder of your ScummVM games.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            Console.WriteLine("No game folders found. No batch files were created.");
            MessageBox.Show("No game folders found. No batch files were created.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}