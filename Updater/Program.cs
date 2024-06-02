using System.Diagnostics;

namespace Updater
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                MessageBox.Show("Invalid arguments. Usage: Updater <updateSourcePath> <updateZipPath>", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Hardcoded paths for SimpleLauncher.exe
            var appExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SimpleLauncher.exe");
            var appArgs = ""; // Add any arguments needed for SimpleLauncher.exe here

            var updateSourcePath = args[0];
            var updateZipPath = args[1];

            if (string.IsNullOrEmpty(appExePath) || string.IsNullOrEmpty(updateSourcePath) || string.IsNullOrEmpty(updateZipPath))
            {
                MessageBox.Show("Invalid file paths provided.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var appDirectory = Path.GetDirectoryName(appExePath) ?? string.Empty;
            if (string.IsNullOrEmpty(appDirectory))
            {
                MessageBox.Show("Could not determine the application directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Wait for the main application to exit
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
                        File.Copy(file, destFile, true);
                    }
                }

                // Delete the temporary update files and the update.zip file
                Directory.Delete(updateSourcePath, true);

                // Notify the user of a successful update
                MessageBox.Show("Update installed successfully. The application will now restart.", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Restart the main application
                var startInfo = new ProcessStartInfo
                {
                    FileName = appExePath,
                    Arguments = appArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = new Process
                {
                    StartInfo = startInfo
                };

                process.OutputDataReceived += (_, e) => Console.WriteLine(e.Data);
                process.ErrorDataReceived += (_, e) => Console.WriteLine(e.Data);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}\nPlease update the application manually.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
