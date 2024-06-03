using System.Diagnostics;

namespace Updater
{
    public partial class UpdateForm : Form
    {
        private readonly string[] _args;
        private delegate void LogDelegate(string message);
        
        public UpdateForm(string[] args)
        {
            InitializeComponent();
            _args = args ?? throw new ArgumentNullException(nameof(args));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var updateThread = new Thread(UpdateProcess);
            updateThread.Start();
        }

        private void UpdateProcess()
        {
            if (_args.Length < 4)
            {
                Log("Invalid arguments. Usage: Updater <appExePath> <updateSourcePath> <updateZipPath> <appArgs>");
                return;
            }

            var appExePath = _args[0];
            var updateSourcePath = _args[1];
            var updateZipPath = _args[2];

            if (string.IsNullOrEmpty(appExePath) || string.IsNullOrEmpty(updateSourcePath) || string.IsNullOrEmpty(updateZipPath))
            {
                Log("Invalid file paths provided.");
                return;
            }

            var appDirectory = Path.GetDirectoryName(appExePath) ?? string.Empty;
            if (string.IsNullOrEmpty(appDirectory))
            {
                Log("Could not determine the application directory.");
                return;
            }

            try
            {
                // Wait for the main application to exit
                Log("Waiting for the main application to exit...");
                Thread.Sleep(3000);

                // Files to be ignored during the update
                var ignoredFiles = new[]
                {
                    "Updater.deps.json",
                    "Updater.dll",
                    "Updater.exe",
                    "Updater.pdb",
                    "Updater.runtimeconfig.json"
                };

                // Copy new files to the application directory
                foreach (var file in Directory.GetFiles(updateSourcePath))
                {
                    var fileName = Path.GetFileName(file);
                    if (!ignoredFiles.Contains(fileName))
                    {
                        var destFile = Path.Combine(appDirectory, fileName);
                        Log($"Copying {fileName}...");
                        File.Copy(file, destFile, true);
                    }
                }

                // Delete the temporary update files and the update.zip file
                Log("Deleting temporary update files...");
                Directory.Delete(updateSourcePath, true);
                var updateFilePath = Path.Combine(appDirectory, "update.zip");
                File.Delete(updateFilePath);

                // Notify the user of a successful update
                Log("Update installed successfully. The application will now restart.");
                MessageBox.Show("Update installed successfully. The application will now restart.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Restart the main application
                var simpleLauncherExePath = Path.Combine(appDirectory, "SimpleLauncher.exe");
                var startInfo = new ProcessStartInfo
                {
                    FileName = simpleLauncherExePath,
                    UseShellExecute = false,
                    WorkingDirectory = appDirectory
                };

                Process.Start(startInfo);
                
                // Close the update Window
                Close();
            }
            catch (Exception ex)
            {
                Log($"Update failed: {ex.Message}\nPlease update the application manually.");
                MessageBox.Show("Update failed.\nPlease update the application manually.", "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Close the update Window
                Close();
            }
        }

        private void Log(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new LogDelegate(Log), message);
                return;
            }
            logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
        }
    }
}

