namespace CreateBatchFilesForScummVMGames;

static class Program
{
    private static LogForm? _logForm;
    
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        _logForm = new LogForm();
        _logForm.Show();
        
        _logForm.LogMessage("Welcome to the Batch File Creator for ScummVM Games.");
        _logForm.LogMessage("");
        
        _logForm.LogMessage("This program creates batch files to launch your ScummVM games.");
        _logForm.LogMessage("Please follow the instructions.");
        _logForm.LogMessage("");
        
        MessageBox.Show("This program creates batch files to launch your ScummVM games.\n\nPlease follow the instructions.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        _logForm.LogMessage("Please select the ScummVM executable file (scummvm.exe):");
        _logForm.LogMessage("");
        
        string? scummvmExePath = SelectFile();

        if (string.IsNullOrEmpty(scummvmExePath))
        {
            _logForm.LogMessage("No file selected. Exiting application.");
            _logForm.LogMessage("");
            
            MessageBox.Show("No file selected. Exiting application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _logForm.LogMessage("Select the root folder containing your ScummVM game folders:");
        _logForm.LogMessage("");

        string? rootFolder = SelectFolder();

        if (string.IsNullOrEmpty(rootFolder))
        {
            _logForm.LogMessage("No folder selected. Exiting application.");
            _logForm.LogMessage("");
            
            MessageBox.Show("No folder selected. Exiting application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        CreateBatchFilesForScummVmGames(rootFolder, scummvmExePath);
        
        Application.Run(_logForm); // Keeps the application running for the log form to remain open
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
                
                _logForm?.LogMessage($"Batch file created: {batchFilePath}");
            }
            filesCreated++;
        }

        if (filesCreated > 0)
        {
            _logForm?.LogMessage("");
            _logForm?.LogMessage("All batch files have been successfully created.");
            _logForm?.LogMessage("They are located in the root folder of your ScummVM games.");
            
            MessageBox.Show("All batch files have been successfully created.\n\nThey are located in the root folder of your ScummVM games.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            _logForm?.LogMessage("No game folders found. No batch files were created.");
            
            MessageBox.Show("No game folders found. No batch files were created.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}