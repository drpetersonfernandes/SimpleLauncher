namespace CreateBatchFilesForXbox360XBLAGames;

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

        _logForm.LogMessage("Welcome to the Batch File Creator for Xbox 360 XBLA Games.");
        _logForm.LogMessage("");

        _logForm.LogMessage("This program creates batch files to launch your Xbox 360 XBLA games.");
        _logForm.LogMessage("Please follow the instructions.");
        _logForm.LogMessage("");

        MessageBox.Show("This program creates batch files to launch your Xbox 360 XBLA games.\n\n" +
                        "Please follow the instructions.",
            "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

        _logForm.LogMessage("Please select the Xenia executable file (xenia.exe):");
        _logForm.LogMessage("");

        string? xeniaExePath = SelectFile();

        if (string.IsNullOrEmpty(xeniaExePath))
        {
            _logForm.LogMessage("No file selected. Exiting application.");
            _logForm.LogMessage("");

            MessageBox.Show("No file selected. Exiting application.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _logForm.LogMessage("Select the root folder containing your Xbox 360 XBLA game folders:");
        _logForm.LogMessage("");

        string? rootFolder = SelectFolder();

        if (string.IsNullOrEmpty(rootFolder))
        {
            _logForm.LogMessage("No folder selected. Exiting application.");
            _logForm.LogMessage("");

            MessageBox.Show("No folder selected. Exiting application.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        CreateBatchFilesForXeniaGames(rootFolder, xeniaExePath);

        Application.Run(); // Keeps the application running for the log form to remain open
    }

    private static string? SelectFolder()
    {
        using var fbd = new FolderBrowserDialog();
        fbd.Description = "Please select the root folder where your Xbox 360 XBLA game folders are located.";

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
        ofd.Title = "Please select the Xenia executable file (xenia.exe)";
        ofd.Filter = "exe files (*.exe)|*.exe|All files (*.*)|*.*";
        ofd.FilterIndex = 1;
        ofd.RestoreDirectory = true;

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            return ofd.FileName;
        }
        return null;
    }

    private static void CreateBatchFilesForXeniaGames(string rootFolder, string xeniaExePath)
    {
        string[] gameDirectories = Directory.GetDirectories(rootFolder);
        int filesCreated = 0;

        foreach (string gameDirectory in gameDirectories)
        {
            string gameFolderName = Path.GetFileName(gameDirectory);
            string batchFilePath = Path.Combine(rootFolder, gameFolderName + ".bat");

            // Find the game file in 000D0000 folder
            string? gameFilePath = FindGameFileIn000D0000Folder(gameDirectory);

            if (string.IsNullOrEmpty(gameFilePath))
            {
                _logForm?.LogMessage($"No game file found in {gameFolderName}. Skipping...");
                continue;
            }

            using (StreamWriter sw = new(batchFilePath))
            {
                sw.WriteLine($"\"{xeniaExePath}\" \"{gameFilePath}\"");

                _logForm?.LogMessage($"Batch file created: {batchFilePath}");
            }
            filesCreated++;
        }

        if (filesCreated > 0)
        {
            _logForm?.LogMessage("");
            _logForm?.LogMessage("All batch files have been successfully created.");
            _logForm?.LogMessage("They are located in the root folder of your Xbox 360 XBLA games.");

            MessageBox.Show("All batch files have been successfully created.\n\n" +
                            "They are located in the root folder of your Xbox 360 XBLA games.",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            _logForm?.LogMessage("No valid game folders found. No batch files were created.");

            MessageBox.Show("No valid game folders found. No batch files were created.",
                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string? FindGameFileIn000D0000Folder(string gameDirectory)
    {
        try
        {
            string[] directories = Directory.GetDirectories(gameDirectory, "000D0000", SearchOption.AllDirectories);

            if (directories.Length > 0)
            {
                string[] files = Directory.GetFiles(directories[0]);

                if (files.Length > 0)
                {
                    return files[0]; // Return the first file found
                }
            }
        }
        catch (Exception ex)
        {
            _logForm?.LogMessage($"Error finding game file in {gameDirectory}: {ex.Message}");
        }

        return null;
    }
}
