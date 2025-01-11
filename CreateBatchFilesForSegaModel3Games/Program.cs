namespace CreateBatchFilesForSegaModel3Games;

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

        _logForm.LogMessage("Welcome to the Batch File Creator for Sega Model 3 Games.");
        _logForm.LogMessage("");

        _logForm.LogMessage("This program creates batch files to launch your Sega Model 3 games.");
        _logForm.LogMessage("Please follow the instructions.");
        _logForm.LogMessage("");

        MessageBox.Show("This program creates batch files to launch your Sega Model 3 games.\n\nPlease follow the instructions.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        _logForm.LogMessage("Please select the Supermodel executable file (Supermodel.exe):");
        _logForm.LogMessage("");

        string? supermodelExePath = SelectFile();

        if (string.IsNullOrEmpty(supermodelExePath))
        {
            _logForm.LogMessage("No file selected. Exiting application.");
            _logForm.LogMessage("");
            
            MessageBox.Show("No file selected. Exiting application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _logForm.LogMessage("Select the folder containing your Sega Model 3 ROM zip files:");
        _logForm.LogMessage("");

        string? romFolder = SelectFolder();

        if (string.IsNullOrEmpty(romFolder))
        {
            _logForm.LogMessage("No folder selected. Exiting application.");
            _logForm.LogMessage("");
            
            MessageBox.Show("No folder selected. Exiting application.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CreateBatchFilesForModel3Games(romFolder, supermodelExePath);

        Application.Run(_logForm);
    }

    private static string? SelectFolder()
    {
        using var fbd = new FolderBrowserDialog();
        fbd.Description = "Please select the folder where your Sega Model 3 ROM zip files are located.";

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
        ofd.Title = "Please select the Supermodel executable file (Supermodel.exe)";
        ofd.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
        ofd.FilterIndex = 1;
        ofd.RestoreDirectory = true;

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            return ofd.FileName;
        }
        return null;
    }

    private static void CreateBatchFilesForModel3Games(string romFolder, string supermodelExePath)
    {
        string[] romFiles = Directory.GetFiles(romFolder, "*.zip");
        int filesCreated = 0;

        foreach (string romFilePath in romFiles)
        {
            string romFileName = Path.GetFileNameWithoutExtension(romFilePath); // Use this to name the batch file
            string batchFilePath = Path.Combine(romFolder, romFileName + ".bat");

            using (StreamWriter sw = new(batchFilePath))
            {
                sw.WriteLine($"cd /d \"{Path.GetDirectoryName(supermodelExePath)}\"");
                sw.WriteLine($"\"{supermodelExePath}\" \"{romFilePath}\" -fullscreen -show-fps");

                _logForm?.LogMessage($"Batch file created: {batchFilePath}");
            }
            filesCreated++;
        }

        if (filesCreated > 0)
        {
            _logForm?.LogMessage("");
            _logForm?.LogMessage("All batch files have been successfully created.");
            _logForm?.LogMessage("They are located in the same folder as your ROM zip files.");
            
            MessageBox.Show("All batch files have been successfully created.\n\nThey are located in the same folder as your ROM zip files.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            _logForm?.LogMessage("No ROM zip files found. No batch files were created.");
            
            MessageBox.Show("No ROM zip files found. No batch files were created.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
