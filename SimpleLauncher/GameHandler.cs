using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleLauncher;

namespace SimpleLauncher
{
    public class GameHandler
    {
        public async Task<List<string>> GetFilesAsync(string directoryPath, List<string> fileExtensions)
        {
            return await Task.Run(() =>
            {
                try
                {
                    Console.WriteLine($"Directory Path: {directoryPath}"); // Debug line

                    if (!Directory.Exists(directoryPath))
                    {
                        Console.WriteLine("Directory doesn't exist!"); // Debug line
                        return new List<string>();
                    }

                    var foundFiles = fileExtensions.SelectMany(ext => Directory.GetFiles(directoryPath, ext)).ToList();

                    Console.WriteLine($"Found {foundFiles.Count} files."); // Debug line
                    return foundFiles;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    return new List<string>();
                }
            });
        }



        public List<string> FilterFiles(List<string> files, string startLetter)
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

        public async Task<List<string>> LoadGamesAsync(string systemName, string filterLetter = "A")
        {
            // Load all system configs
            var allConfigs = SystemConfig.LoadSystemConfigs("system.xml"); // Assuming your XML path
            var targetSystemConfig = allConfigs.FirstOrDefault(sc => sc.SystemName == systemName);

            if (targetSystemConfig == null)
            {
                Console.WriteLine($"System '{systemName}' not found in config.");
                return new List<string>();
            }

            var fileExtensions = targetSystemConfig.FileFormatsToSearch.Select(ext => $"*.{ext}").ToList();
            var gameFiles = await GetFilesAsync(targetSystemConfig.SystemFolder, fileExtensions);
            var filteredGames = FilterFiles(gameFiles, filterLetter);
            return filteredGames;
        }

    }
}
