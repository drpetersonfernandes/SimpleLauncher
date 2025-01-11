namespace CreateBatchFilesForWindowsGames;

static class Program
{
    private static LogForm? _logForm;
    
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        _logForm = new LogForm();
        _logForm.Show();
        
        _logForm.LogMessage("Welcome to the Batch File Creator for Microsoft Windows Games.");
        _logForm.LogMessage("");
        _logForm.LogMessage("This program create batch files to launch your Microsoft Windows games.");
        _logForm.LogMessage("Please follow the instructions.");
        _logForm.LogMessage("");
        
        MessageBox.Show("This program create batch files to launch your Microsoft Windows games.\n\nPlease follow the instructions.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        while (true)
        {
            // Step 1: Ask user to select the game executable file
            _logForm.LogMessage("Select the game executable file:");
            _logForm.LogMessage("");
            
            string? gameExePath = SelectFile();

            if (string.IsNullOrEmpty(gameExePath))
            {
                _logForm.LogMessage("No file selected. Exiting program.");
                
                MessageBox.Show("No file selected. Exiting program.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Define the game folder path to use in batch file naming
            string gameFolderPath = Path.GetDirectoryName(gameExePath) ?? "";
            string gameFileName = Path.GetFileName(gameExePath);

            // Step 2: Ask user to choose the output file location and name using SaveFileDialog
            string? batchFilePath = GetBatchFilePath(gameFolderPath);
            if (string.IsNullOrEmpty(batchFilePath))
            {
                _logForm.LogMessage("No output file selected. Exiting program.");
                
                MessageBox.Show("No output file selected. Exiting program.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Step 3: Create the batch file
            using (StreamWriter sw = new(batchFilePath))
            {
                sw.WriteLine("@echo off");
                sw.WriteLine($"cd /d \"{gameFolderPath}\"");
                sw.WriteLine($"start {gameFileName}");
            }

            _logForm.LogMessage($"Batch file '{Path.GetFileName(batchFilePath)}' has been successfully created.");

            MessageBox.Show($"Batch file '{Path.GetFileName(batchFilePath)}' has been successfully created.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Step 4: Ask if the user wants to create another batch file
            _logForm.LogMessage("");
            _logForm.LogMessage("Do you want to create another batch file?");
            _logForm.LogMessage("");
            
            DialogResult result = MessageBox.Show("Do you want to create another batch file?", "Continue", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                _logForm.LogMessage("Batch file creation process completed.");
                
                MessageBox.Show("Batch file creation process completed.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            }
        }
        
        Application.Run(_logForm); // Keeps the application running for the log form to remain open
    }

    private static string? SelectFile()
    {
        using var ofd = new OpenFileDialog();
        ofd.Title = "Please select the game executable file (e.g., game.exe)";
        ofd.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
        ofd.FilterIndex = 1;
        ofd.RestoreDirectory = true;

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            return ofd.FileName;
        }
        return null;
    }

    private static string? GetBatchFilePath(string gameFolderPath)
    {
        using var sfd = new SaveFileDialog();
        sfd.Title = "Save Batch File";
        sfd.Filter = "Batch files (*.bat)|*.bat";
        sfd.DefaultExt = "bat";
        sfd.AddExtension = true;
        sfd.RestoreDirectory = true;

        // Suggest a name using the game folder name
        string folderName = Path.GetFileName(gameFolderPath.TrimEnd(Path.DirectorySeparatorChar));
        sfd.FileName = $"{folderName}.bat";

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            return sfd.FileName;
        }
        return null;
    }
}