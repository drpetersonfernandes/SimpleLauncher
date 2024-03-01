using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher
{
    public class LoadFiles
    {
        public static async Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Console.WriteLine($@"Directory Path: {directoryPath}");

                    if (!Directory.Exists(directoryPath))
                    {
                        Console.WriteLine(@"Directory doesn't exist!");
                        return [];
                    }

                    var foundFiles = fileExtensions.SelectMany(ext => Directory.GetFiles(directoryPath, ext)).ToList();

                    Console.WriteLine($@"Found {foundFiles.Count} files.");
                    return foundFiles;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"An error occurred: {ex.Message}");
                    return [];
                }
            });
        }

        public static List<string> FilterFiles(List<string> files, string startLetter)
        {
            if (string.IsNullOrEmpty(startLetter))
                return files; // If no startLetter is provided, no filtering is required

            if (startLetter == "#")
            {
                return files.Where(file => char.IsDigit(Path.GetFileName(file)[0])).ToList();
            }
            else
            {
                return files.Where(file => Path.GetFileName(file).StartsWith(startLetter, StringComparison.OrdinalIgnoreCase)).ToList();
            }
        }

        public static async Task<List<string>> LoadGamesAsync(string systemName, string filterLetter = "A")
        {
            // Load all system configs
            var allConfigs = SystemConfig.LoadSystemConfigs("system.xml");
            var targetSystemConfig = allConfigs.FirstOrDefault(sc => sc.SystemName == systemName);

            if (targetSystemConfig == null)
            {
                Console.WriteLine($@"System '{systemName}' not found in config.");
                return [];
            }

            var fileExtensions = targetSystemConfig.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList();
            var gameFiles = await GetFilesAsync(targetSystemConfig.SystemFolder, fileExtensions);
            var filteredGames = FilterFiles(gameFiles, filterLetter);
            return filteredGames;
        }

        public static int CountFiles(string folderPath, List<string> extensions)
        {
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show($"The directory {folderPath} does not exist.", "Directory Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return 0;
            }

            try
            {
                // Get all files in the directory that match the specified extensions
                var files = Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(file => extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

                return files.Count();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while counting files: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }
        }


    }
}
