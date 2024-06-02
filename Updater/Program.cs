using System.Diagnostics;

namespace Updater
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                MessageBox.Show("Invalid arguments. Usage: Updater <appExePath> <updateSourcePath> <updateZipPath> <appArgs>", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var appExePath = args[0];
            var updateSourcePath = args[1];
            var updateZipPath = args[2];
            var appArgs = args[3];

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
                File.Delete(updateZipPath);

                // Notify the user of a successful update
                MessageBox.Show("Update installed successfully. The application will now restart.", "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Restart the main application
                Process.Start(appExePath, appArgs);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update failed: {ex.Message}\nPlease update the application manually.", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
